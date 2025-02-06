using Microsoft.Win32;
using NAudio.Wave;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace ResumePlayer
{
	internal class ViewModel
	{
		public ICommand AppendPlaylistCommand { get; }
		public ICommand ClearPlaylistCommand { get; }
		public ICommand PlayAudioCommand { get; }
		public ICommand PauseAudioCommand { get; }

		public PlayList Playlist { get; set; } = new PlayList();

		private NAudio.Wave.WaveOut mPlayer = new NAudio.Wave.WaveOut();
		private NAudio.Wave.AudioFileReader? mReader;

		public float Volume
		{
			get => mPlayer.Volume;
			set => mPlayer.Volume = value;
		}

		public ViewModel()
		{
			AppendPlaylistCommand = new ActionCommand(AppendPlaylist);
			ClearPlaylistCommand = new ActionCommand(ClearPlaylist);
			PlayAudioCommand = new ActionCommand(PlayAudio);
			PauseAudioCommand = new ActionCommand(PauseAudio);
		}

		public void Load()
		{
			if (!System.IO.File.Exists("config")) return;

			String[] lines = System.IO.File.ReadAllLines("config");
			Playlist.Load(lines);
		}

		public void Save()
		{
			PauseAudio(null);
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

		private void PlayAudio(Object? objs)
		{
			PauseAudio(null);

			var audio = Playlist.Play();
			if (audio == null) return;

			mReader = new NAudio.Wave.AudioFileReader(audio.FileName);
			mReader.Position = audio.Position;
			mPlayer.Init(mReader);
			mPlayer.Play();
		}

		private void PauseAudio(Object? objs)
		{
			if(mPlayer.PlaybackState == NAudio.Wave.PlaybackState.Playing)
			{
				mPlayer.Pause();
				Playlist.Pause(mReader?.Position ?? 0);
			}
		}
	}
}
