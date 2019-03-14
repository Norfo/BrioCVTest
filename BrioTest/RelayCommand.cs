using System;
using System.Diagnostics;
using System.Windows.Input;

namespace BrioTest
{
    /// <summary>
    /// This class provide the interaction with WPF window buttons.
    /// </summary>
    public class RelayCommand : ICommand
    {
        readonly Action<object> executeAct;
        readonly Predicate<object> canExecute;

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> exec, Predicate<object> canExec)
        {
            if (exec == null)
                throw new ArgumentNullException("execute");

            executeAct = exec;
            this.canExecute = canExec;
        }


        [DebuggerStepThrough]
        public bool CanExecute(object parameters)
        {
            return canExecute == null ? true : canExecute(parameters);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameters)
        {
            executeAct(parameters);
        }

    }
}
