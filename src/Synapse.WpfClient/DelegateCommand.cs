using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Synapse.WpfClient
{
    class DelegateCommand : ICommand
    {
        Action<object> m_ExecuteHandler;

        public DelegateCommand(Action<object> executeHandler)
        {
            m_ExecuteHandler = executeHandler;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            m_ExecuteHandler(parameter);
        }
    }
}
