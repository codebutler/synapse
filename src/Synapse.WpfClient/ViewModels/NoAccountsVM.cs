using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Synapse.WpfClient.ViewModels
{
    public class NoAccountsVM : ViewModelBase
    {
        MainWindowVM m_MainWindowVM;

        WelcomeVM m_WelcomeVM;
        AddFirstAccountVM m_AddAccountVM;

        DelegateCommand m_ShowAddAccountViewCommand;

        object m_BottomStuff;

        public NoAccountsVM(MainWindowVM mainWindow)
        {
            m_MainWindowVM = mainWindow;

            m_WelcomeVM = new WelcomeVM(this);
            m_AddAccountVM = new AddFirstAccountVM(this);

            m_ShowAddAccountViewCommand = new DelegateCommand(delegate
            {
                if (BottomStuff == m_AddAccountVM)
                    BottomStuff = m_WelcomeVM;
                else
                    BottomStuff = m_AddAccountVM;
            });

            BottomStuff = m_WelcomeVM;
        }
        
        public object BottomStuff
        {
            get
            {
                return m_BottomStuff;
            }

            set
            {
                m_BottomStuff = value;

                OnPropertyChanged("BottomStuff");
            }
        }

        public ICommand ShowAddAccountViewCommand
        {
            get
            {
                return m_ShowAddAccountViewCommand;
            }
        }

    }
}
