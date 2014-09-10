using System;
using System.Windows.Input;

namespace MultiType.Commands
{
    public class LambdaCommand : ICommand
    {
        private readonly Action _action;
        private readonly Action<object> _parameterizedAction;

        /// <summary>
        /// Initializes a new instance of the LambdaCommand class that does not accept any arguments.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        public LambdaCommand(Action action)
        {
            //  Set the action.
            _action = action;
        }

        /// <summary>
        /// Initializes a new instance of the LambdaCommand class that accepts a parameter.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        public LambdaCommand(Action<object> action)
        {
            //  Set the action.
            _parameterizedAction = action;
        }

        /// <summary>
        /// Determine whether the command may be executed with a given argument
        /// The command may be executed if
        ///     A) Both an argument and an argument callback have been provided
        ///     B) No argument has been provided and a 0-argument callback has been provided
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public bool CanExecute(object arg)
        {
            if (arg == null)
            {
                return _action != null;
            }
            else
            {
                return _parameterizedAction != null;
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object arg)
        {
            if (arg == null)
            {
                _action();
            }
            else
            {
                _parameterizedAction(arg);
            }
        }
    }
}
