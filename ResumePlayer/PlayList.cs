using System.Collections.ObjectModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Text;
using System.ComponentModel;

namespace ResumePlayer
{
	internal class PlayList : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public ObservableCollection<Audio> Items { get; set; } = new ObservableCollection<Audio>();
		public int SelectIndex { get; set; } = -1;
		private int mCurrentIndex = -1;

		public bool Append(String filename, long position)
		{
			if (!System.IO.File.Exists(filename)) return false;

			Items.Add(new Audio() { FileName = filename, Position = position });
			return true;
		}

		public String Save()
		{
			var sb = new StringBuilder();
			sb.AppendLine(Items.Count.ToString());
			foreach (var item in Items)
			{
				sb.AppendLine(item.FileName);
				sb.AppendLine(item.Position.ToString());
			}
			sb.AppendLine(mCurrentIndex.ToString());
			sb.AppendLine(SelectIndex.ToString());

			return sb.ToString();
		}

		public void Load(String[] lines)
		{
			int count = int.Parse(lines[0]);
			for (int i = 0; i < count; i++)
			{
				String filename = lines[i * 2 + 1];
				long position = long.Parse(lines[i * 2 + 2]);
				Append(filename, position);
			}
			mCurrentIndex = int.Parse(lines[count * 2 + 1]);
			SelectIndex = int.Parse(lines[count * 2 + 2]);

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectIndex)));
		}

		public void Clear()
		{
			Items.Clear();
			SelectIndex = -1;
			mCurrentIndex = -1;
		}

		public Audio? Play()
		{
			mCurrentIndex = SelectIndex;
			return CurrentAudio();
		}

		public void Pause(long position)
		{
			var audio = CurrentAudio();
			if (audio == null) return;

			audio.Position = position;
		}

		public void Stop()
		{
			var audio = CurrentAudio();
			if (audio == null) return;

			audio.Position = 0;
		}

		private Audio? CurrentAudio()
		{
			if (Items.Count == 0) return null;
			if (mCurrentIndex < 0) return null;
			if (mCurrentIndex >= Items.Count) return null;

			return Items[mCurrentIndex];
		}
	}
}
