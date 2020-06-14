using System;
using System.Windows.Input;

namespace SignGen.WpfApp.Command
{
    /// <summary>
    /// Einfache ICommand-Implementierung, um Installation weiterer Packages zu vermeiden
    /// </summary>
    public class SimpleActionCommand : ICommand
    {
        private Action _executeAction;
        private Action<object> _executeActionParameter;

        public SimpleActionCommand(Action execAction)
        {
            _executeAction = execAction;
        }

        public SimpleActionCommand(Action<object> execAction)
        {
            _executeActionParameter = execAction;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _executeAction?.Invoke();
            _executeActionParameter?.Invoke(parameter);
        }
    }
}
