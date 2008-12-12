//
// ChatWindowManager.cs
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
using Synapse.Services;
using Synapse.ServiceStack;
using Synapse.UI.Controllers;
using Synapse.Xmpp;
using jabber.connection;
using jabber.protocol.client;

namespace Synapse.UI
{
	public class ChatWindowManager
	{	
		Dictionary<Account, AccountChatWindowManager> m_AccountManagers;
		
		public ChatWindowManager()
		{
			m_AccountManagers = new Dictionary<Account, AccountChatWindowManager>();
			
			AccountService accountService = ServiceManager.Get<AccountService>();
			foreach (Account account in accountService.Accounts) {
				HandleAccountAdded(account);
			}
			accountService.AccountAdded   += HandleAccountAdded;
			accountService.AccountRemoved += HandleAccountRemoved;
		}
		
		void HandleAccountAdded(Account account)
		{
			m_AccountManagers.Add(account, new AccountChatWindowManager(account));
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
			
			public AccountChatWindowManager (Account account)
			{
				m_Account = account;
				account.ConferenceManager.OnJoin += HandleOnJoin;
			}

			public void Dispose ()
			{
				if (m_MucWindows.Count > 0)
					throw new InvalidOperationException();
				
				m_Account.ConferenceManager.OnJoin -= HandleOnJoin;
			}
			
			void HandleOnJoin(Room room)
			{
				if (!m_MucWindows.ContainsKey(room)) {
					Application.Invoke(delegate {
						m_MucWindows[room] = new MucWindowController(m_Account, room);
						m_MucWindows[room].Closed += HandleClosed;
					});
				}
			}
	
			void HandleClosed(object sender, EventArgs e)
			{
				Room room = ((MucWindowController)sender).Room;
				m_MucWindows.Remove(room);
			}
		}
	}
}
