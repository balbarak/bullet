using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Bullet.Wpf
{
    public class BulletCommand : ICommand
    {
        readonly Func<object, bool> _canExecute;
        readonly Action<object> _execute;
        private event EventHandler CanExecuteChangedInternal;

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                this.CanExecuteChangedInternal += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
                this.CanExecuteChangedInternal -= value;
            }
        }

        public BulletCommand(Action<object> execute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
        }

        public BulletCommand(Action execute) : this(o => execute())
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
        }

        public BulletCommand(Action<object> execute, Func<object, bool> canExecute) : this(execute)
        {
            if (canExecute == null)
                throw new ArgumentNullException(nameof(canExecute));

            _canExecute = canExecute;
        }

        public BulletCommand(Action execute, Func<bool> canExecute) : this(o => execute(), o => canExecute())
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            if (canExecute == null)
                throw new ArgumentNullException(nameof(canExecute));
        }

        

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute(parameter);

            return true;
        }

        public void OnCanExecuteChanged()
        {
            EventHandler handler = this.CanExecuteChangedInternal;
            if (handler != null)
            {
                //DispatcherHelper.BeginInvokeOnUIThread(() => handler.Invoke(this, EventArgs.Empty));
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        private static bool DefaultCanExecute(object parameter)
        {
            return true;
        }

    }
}
