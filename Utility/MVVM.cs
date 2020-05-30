using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ZUtility
{
    /// <summary>
    /// View Model window handling.
    /// </summary>
    public class ZMVVM_VM: INotifyPropertyChanged
    {
        // INotifyPropertyChanged /////////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// 
    /// </summary>
    public class ZMVVM_Command : ICommand
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
        public ZMVVM_Command(DelCanExecute canExecute, DelExecute execute)
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
