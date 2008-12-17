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
using Synapse.UI.Controllers;
using jabber;
using jabber.connection;
using jabber.protocol.client;
using Mono.Addins;

using Synapse.UI.Actions.ExtensionNodes;

namespace Synapse.UI.Services
{
	public delegate void ChatWindowOpenEvent (AbstractChatWindowController window, bool focus);
	public delegate void ChatWindowCloseEvent (AbstractChatWindowController window);
	
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
		
		class AccountChatWindowManager : IDisposable
		{
			Account m_Account;
			Dictionary<Room, MucWindowController> m_MucWindows;
			Dictionary<string, ChatWindowController> m_ChatWindows;
			
			public event ChatWindowOpenEvent ChatWindowOpened;
			public event ChatWindowCloseEvent ChatWindowClosed;
			
			public AccountChatWindowManager (Account account)
			{
				m_MucWindows  = new Dictionary<Room, MucWindowController>();
				m_ChatWindows = new Dictionary<string, ChatWindowController>();
				
				m_Account = account;
				account.ConferenceManager.OnJoin += HandleOnJoin;
				account.Client.OnMessage += HandleOnMessage;
			}

			public ChatWindowController OpenChatWindow (JID jid, bool focus)
			{
				ChatWindowController window = null;
				if (!m_ChatWindows.ContainsKey(jid.Bare)) {
					window = new ChatWindowController(m_Account, jid);
					window.Closed += HandleChatWindowClosed;
					m_ChatWindows.Add(jid.Bare, window);

					if (ChatWindowOpened != null)
						ChatWindowOpened(window, focus);
				} else {
					window = m_ChatWindows[jid.Bare];
				}
				return window;
			}

			public void Dispose ()
			{
				if (m_MucWindows.Count > 0)
					throw new InvalidOperationException();
				
				m_Account.ConferenceManager.OnJoin -= HandleOnJoin;
			}

			void HandleOnMessage (object sender, Message message)
			{
				if (message.Type == MessageType.chat) {
					ChatWindowController window = OpenChatWindow(message.From, false);
					window.AppendMessage(message);
				}
			}
			
			void HandleOnJoin(Room room)
			{
				if (!m_MucWindows.ContainsKey(room)) {
					var window = new MucWindowController(m_Account, room);
					window.Closed += HandleMucWindowClosed;
					m_MucWindows[room] = window;
					
					if (ChatWindowOpened != null)
						ChatWindowOpened(window, true);
				}
			}

			void HandleChatWindowClosed(object sender, EventArgs e)
			{
				var window = (ChatWindowController)sender;
				m_ChatWindows.Remove(window.Jid.Bare);

				if (ChatWindowClosed != null)
					ChatWindowClosed(window);
			}
			
			void HandleMucWindowClosed(object sender, EventArgs e)
			{
				Room room = ((MucWindowController)sender).Room;
				var window = m_MucWindows[room];
				m_MucWindows.Remove(room);
				
				if (ChatWindowClosed != null)
					ChatWindowClosed(window);
			}
		}
	}
}