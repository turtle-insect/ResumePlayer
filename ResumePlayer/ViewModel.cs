using Microsoft.Win32;
using NAudio.Wave;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace ResumePlayer
{
	internal class ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public ICommand AppendPlaylistCommand { get; }
		public ICommand ClearPlaylistCommand { get; }
		public ICommand ChangePlayerStatusCommand { get; }

		public PlayList Playlist { get; set; } = new PlayList();

		private System.Windows.Threading.DispatcherTimer mTimer = new();
		private NAudio.Wave.WaveOut mPlayer = new();
		private NAudio.Wave.AudioFileReader? mReader;

		public float Volume
		{
			get => mPlayer.Volume;
			set => mPlayer.Volume = value;
		}

		private long mTotalPosition;
		public long TotalPosition
		{
			get => mTotalPosition;
			private set
			{
				mTotalPosition = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalPosition)));
			}
		}

		private long mCurrentPosition;
		public long CurrentPosition
		{
			get => mCurrentPosition;
			set
			{
				if (mReader != null)
				{
					mReader.CurrentTime = new TimeSpan(value);
				}
				mCurrentPosition = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentPosition)));
			}
		}

		public ViewModel()
		{
			AppendPlaylistCommand = new ActionCommand(AppendPlaylist);
			ClearPlaylistCommand = new ActionCommand(ClearPlaylist);
			ChangePlayerStatusCommand = new ActionCommand(ChangePlayerStatus);

			mTimer.Interval = new TimeSpan(0, 0, 1);
			mTimer.Tick += MTimer_Tick;
		}

		private void MTimer_Tick(object? sender, EventArgs e)
		{
			CurrentPosition = mReader?.CurrentTime.Ticks ?? 0;
		}

		public void Load()
		{
			if (!System.IO.File.Exists("config")) return;

			String[] lines = System.IO.File.ReadAllLines("config");
			Playlist.Load(lines);
			mTimer.Start();
		}

		public void Save()
		{
			mTimer.Stop();
			PauseAudio();
			System.IO.File.WriteAllText("config", Playlist.Save());
		}

		public void DropFile(String[] filenames)
		{
			foreach (var filename in filenames)
			{
				var extension = System.IO.Path.GetExtension(filename);
				if (extension == null) continue;
				extension = extension.ToLower();
				switch (extension)
				{
					case ".wav":
					case ".mp3":
						Playlist.Append(filename, 0);
						break;
				}
			}
		}

		private void AppendPlaylist(Object? objs)
		{
			var dlg = new OpenFileDialog();
			dlg.Filter = "audio|*.wav;*.mp3";
			dlg.Multiselect = true;
			if (dlg.ShowDialog() == false) return;

			foreach (var filename in dlg.FileNames)
			{
				Playlist.Append(filename, 0);
			}
		}

		private void ClearPlaylist(Object? objs)
		{
			mPlayer.Stop();
			Playlist.Clear();
		}

		public void ChangePlayerStatus(Object? objs)
		{
			if (mPlayer.PlaybackState == NAudio.Wave.PlaybackState.Playing) PauseAudio();
			else PlayAudio();
		}

		private void PlayAudio()
		{
			PauseAudio();

			var audio = Playlist.Play();
			if (audio == null) return;

			mReader = new NAudio.Wave.AudioFileReader(audio.FileName);
			mReader.CurrentTime = new TimeSpan(audio.Position);
			mPlayer.Init(mReader);
			TotalPosition = mReader.TotalTime.Ticks;
			mPlayer.Play();
		}

		private void PauseAudio()
		{
			if (mPlayer.PlaybackState != NAudio.Wave.PlaybackState.Playing) return;

			mPlayer.Pause();
			Playlist.Pause(mReader?.CurrentTime.Ticks ?? 0);
		}
	}
}
