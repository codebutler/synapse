using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using Synapse.ServiceStack;
using Synapse.Xmpp.Services;

namespace Synapse.WpfClient.ViewModels
{
    public class MainWindowVM : ViewModelBase
    {
        static MainWindowVM s_Instance;

        DelegateCommand m_CloseCommand;
        object m_ContentVM;

        NoAccountsVM m_NoAccountsVM;
        RosterVM m_RosterVM;

        public static MainWindowVM Instance
        {
            get
            {
                return s_Instance;
            }
        }

        public MainWindowVM()
        {
            if (s_Instance != null)
                throw new InvalidOperationException("MainWindowVM already exists");
            s_Instance = this;

            m_CloseCommand = new DelegateCommand(delegate
            {
                System.Windows.Application.Current.Shutdown();
            });

            m_NoAccountsVM = new NoAccountsVM(this);
            m_RosterVM = new RosterVM(this);

            var accountService = ServiceManager.Get<AccountService>();
            accountService.AccountAdded += new Xmpp.AccountEventHandler(accountService_AccountAdded);
            accountService.AccountRemoved += new Xmpp.AccountEventHandler(accountService_AccountRemoved);

            ToggleContent();
        }

        void ToggleContent()
        {
            var accountService = ServiceManager.Get<AccountService>();
            if (accountService.Accounts.Count == 0)
                ContentVM = m_NoAccountsVM;
            else
                ContentVM = m_RosterVM;
        }

        void accountService_AccountRemoved(Xmpp.Account account)
        {
            ToggleContent();
        }

        void accountService_AccountAdded(Xmpp.Account account)
        {
            ToggleContent();
        }

        public ICommand CloseCommand
        {
            get
            {
                return m_CloseCommand;
            }
        }

        public object ContentVM
        {
            get
            {
                return m_ContentVM;
            }

            set
            {
                m_ContentVM = value;
                OnPropertyChanged("ContentVM");
            }
        }
    }
}
