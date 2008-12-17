//
// GuiService.cs
//
// Copyright (C) 2008 Eric Butler
//
// Authors:
//   Eric Butler <eric@extremeboredom.net>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Threading;
using Synapse.Xmpp;
using Synapse.ServiceStack;
using Synapse.UI;
using Synapse.UI.Controllers;
using jabber;
using jabber.connection;
using jabber.protocol.client;
using Mono.Addins;

using Synapse.UI.Actions.ExtensionNodes;

namespace Synapse.UI.Services
{
	public class GuiService : IService, IRequiredService, IInitializeService
	{
		MainWindowController  m_MainWindow;
		TrayIconController    m_TrayIcon;
		DebugWindowController m_DebugWindow;
		TabbedChatsWindowController m_ChatsWindow;

		Dictionary<Account, AccountChatWindowManager> m_AccountManagers;

		public event ChatWindowOpenEvent ChatWindowOpened;
		public event ChatWindowCloseEvent ChatWindowClosed;
		
		public void Initialize()
		{
			m_AccountManagers = new Dictionary<Account, AccountChatWindowManager>();
			Application.ClientStarted += OnClientStarted;
		}

		public string ServiceName {
			get { return "GuiService"; }
		}

		public MainWindowController MainWindow {
			get {
				return m_MainWindow;
			}
		}

		public DebugWindowController DebugWindow {
			get {
				return m_DebugWindow;
			}
		}

		public TabbedChatsWindowController ChatsWindow {
			get {
				return m_ChatsWindow;
			}
		}
		
		public void OpenChatWindow(Account account, JID jid)
		{
			m_AccountManagers[account].OpenChatWindow(jid, true);
		}
		
		void OnClientStarted (Synapse.ServiceStack.Client client)
		{
			m_MainWindow  = new MainWindowController();
			m_TrayIcon    = new TrayIconController();
			m_DebugWindow = new DebugWindowController();
			m_ChatsWindow = new TabbedChatsWindowController();
		
			AccountService accountService = ServiceManager.Get<AccountService>();
			foreach (Account account in accountService.Accounts) {
				HandleAccountAdded(account);
			}
			accountService.AccountAdded   += HandleAccountAdded;
			accountService.AccountRemoved += HandleAccountRemoved;
		}
			
		void HandleAccountAdded(Account account)
		{
			var manager = new AccountChatWindowManager(account);
			manager.ChatWindowOpened += HandleChatWindowOpened;
			manager.ChatWindowClosed += HandleChatWindowClosed;
			m_AccountManagers.Add(account, manager);
		}

		void HandleChatWindowOpened(AbstractChatWindowController window, bool focus)
		{
			if (ChatWindowOpened != null)
				ChatWindowOpened(window, focus);
		}

		void HandleChatWindowClosed(AbstractChatWindowController window)
		{
			if (ChatWindowClosed != null)
				ChatWindowClosed(window);
		}

		void HandleAccountRemoved(Account account)
		{
			m_AccountManagers[account].Dispose();
			m_AccountManagers.Remove(account);
		}
	}
}