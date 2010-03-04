using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Synapse.WpfClient.ViewModels
{
    public class WelcomeVM : ViewModelBase
    {
        NoAccountsVM m_NoAccountsVM;

        public WelcomeVM(NoAccountsVM noAccountsVM)
        {
            m_NoAccountsVM = noAccountsVM;
        }

        public ICommand ShowAddAccountViewCommand
        {
            get
            {
                return m_NoAccountsVM.ShowAddAccountViewCommand;
            }
        }
    }
}
