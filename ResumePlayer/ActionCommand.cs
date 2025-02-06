using System.Windows.Input;

namespace ResumePlayer
{
	internal class ActionCommand : ICommand
	{
		private readonly Action<Object?> mAction;

#pragma warning disable CS0067
		public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

		public ActionCommand(Action<Object?> action) => mAction = action;
		public bool CanExecute(object? parameter) => true;
		public void Execute(object? parameter) => mAction(parameter);
	}
}
