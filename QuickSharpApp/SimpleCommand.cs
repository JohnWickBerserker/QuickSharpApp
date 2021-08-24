using System;
using System.Windows.Input;

namespace QuickSharpApp
{
    public class SimpleCommand : ICommand
    {
        private Action _action;

        public SimpleCommand(Action action)
        {
            _action = action;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_action != null)
            {
                _action();
            }
        }
    }
}
