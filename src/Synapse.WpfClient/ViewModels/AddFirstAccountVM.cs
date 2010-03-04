using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;

namespace Synapse.WpfClient.ViewModels
{
    public class AddFirstAccountVM : ViewModelBase
    {
        NoAccountsVM m_NoAccountsVM;

        ICommand m_AddAccountCommand;

        public AddFirstAccountVM (NoAccountsVM noAccountsVM)
        {
            m_NoAccountsVM = noAccountsVM;

            m_AddAccountCommand = new DelegateCommand(delegate {
                var accountService = ServiceManager.Get<AccountService>();
                
            });
        }

        public ICommand AddAccountCommand
        {
            get
            {
                return m_AddAccountCommand;
            }
        }
    }
}
