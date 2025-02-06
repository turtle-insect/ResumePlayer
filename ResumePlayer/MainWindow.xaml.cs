using System.Windows;

namespace ResumePlayer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			vm.Load();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			vm.Save();
		}

		private void Window_Drop(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

			var vm = DataContext as ViewModel;
			if (vm == null) return;

			var filenames = e.Data.GetData(DataFormats.FileDrop) as String[];
			if (filenames == null) return;

			vm.DropFile(filenames);
		}
    }
}