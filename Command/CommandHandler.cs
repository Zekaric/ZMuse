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
        // Public /////////////////////////////////////////////////////////////////////////////////
        public delegate Boolean DelCanExecute(Object parameter);
        public delegate void    DelExecute(   Object parameter);

        // Variable ///////////////////////////////////////////////////////////////////////////////
        // Public /////////////////////////////////////////////////////////////////////////////////

        // Private ////////////////////////////////////////////////////////////////////////////////
        private readonly DelCanExecute   _canExecute;
        private readonly DelExecute      _execute;

        // Function ///////////////////////////////////////////////////////////////////////////////
        // Public /////////////////////////////////////////////////////////////////////////////////

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
        // Public /////////////////////////////////////////////////////////////////////////////////
        public event EventHandler CanExecuteChanged
        {
            add    { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public Boolean CanExecute(Object parameter)
        {
            if (this._canExecute != null)
            { 
                return this._canExecute(parameter);
            }

            return true;
        }

        public void Execute(Object parameter) => this._execute?.Invoke(parameter);
    }
}
