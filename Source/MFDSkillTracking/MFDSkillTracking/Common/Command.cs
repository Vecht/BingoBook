using System;
using System.Windows.Input;

namespace MFDSkillTracking.Common
{
    public class Command : ICommand
    {
        private readonly Action<object> _execute;
        private readonly  Predicate<object> _canExecute;
        private bool _lastCanExecute;

        public Command(Action<object> execute)
        {
            _execute = execute;
            _canExecute = (parameter => true);
        }

        public Command(Action execute)
        {
            _execute = o => execute();
            _canExecute = (parameter => true);
        }

        public Command(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute ?? (parameter => true);
        }

        public Command(Action execute, Func<bool> canExecute)
        {
            _execute = o => execute();
            _canExecute = o => canExecute();
        }

        public bool CanExecute(object parameter)
        {
            var canExecute = _canExecute(parameter);
            if (_lastCanExecute != canExecute)
            {
                _lastCanExecute = canExecute;
                CanExecuteChanged?.Invoke(null, null);
            }
            return canExecute;
        }

        public void Execute(object parameter)
        {
            if (!_canExecute(parameter)) throw new InvalidOperationException("CanExecute precondition failed.");
            _execute(parameter);
        }

        public void Update()
        {
            CanExecuteChanged?.Invoke(null, null);
        }

        public event EventHandler CanExecuteChanged;
    }
}
