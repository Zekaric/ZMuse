using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using ZMuse.ViewModel;

namespace ZMuse.Command
{
    public class CommandHandler : ICommand
    {
        // Delegate ///////////////////////////////////////////////////////////////////////////////
        public delegate Boolean DelCanExecute(Object parameter);
        public delegate void    DelExecute(   Object parameter);

        // Variable ///////////////////////////////////////////////////////////////////////////////
        private DelCanExecute   _canExecute;
        private DelExecute      _execute;

        // Function ///////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vm"></param>
        public CommandHandler(DelCanExecute canExecute, DelExecute execute)
        {
            this._canExecute = canExecute;
            this._execute    = execute;
        }

        // ICommand ///////////////////////////////////////////////////////////////////////////////
        public event EventHandler CanExecuteChanged
        {
            add    { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public Boolean CanExecute(Object parameter)
        {
            if (_canExecute != null)
            { 
                return _canExecute(parameter);
            }

            return true;
        }

        public void Execute(Object parameter)
        {
            if (_execute != null)
            { 
                _execute(parameter);
            }
        }
    }
}
