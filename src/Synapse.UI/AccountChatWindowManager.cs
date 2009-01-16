//
// AccountChatWindowManager.cs
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
using System.Text;
using Synapse.Services;
using Synapse.ServiceStack;
using Synapse.UI.Controllers;
using Synapse.UI.Services;
using Synapse.Xmpp;
using jabber;
using jabber.connection;
using jabber.protocol.client;

namespace Synapse.UI
{
	public class AccountChatWindowManager : IDisposable
	{
		Account m_Account;
		Dictionary<Room, MucWindowController> m_MucWindows;
		Dictionary<string, ChatWindowController> m_ChatWindows;
		
		public AccountChatWindowManager (Account account)
		{
			m_MucWindows  = new Dictionary<Room, MucWindowController>();
			m_ChatWindows = new Dictionary<string, ChatWindowController>();
			
			m_Account = account;
			account.ConferenceManager.OnJoin += HandleOnJoin;
			account.Client.OnMessage += HandleOnMessage;
			account.Client.OnPresence += HandleOnPresence;
		}

		public ChatWindowController OpenChatWindow (JID jid, bool focus)
		{
			ChatWindowController window = null;
			if (!m_ChatWindows.ContainsKey(jid.Bare)) {
				window = new ChatWindowController(m_Account, jid);
				window.Closed += HandleChatWindowClosed;
				m_ChatWindows.Add(jid.Bare, window);

				ServiceManager.Get<GuiService>().RaiseChatWindowOpened(window, focus);
			} else {
				window = m_ChatWindows[jid.Bare];
				if (focus) {
					ServiceManager.Get<GuiService>().RaiseChatWindowFocused(window);
				}
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
				// Make sure we don't open a new window if all we've got is a chatstate.
				// Some people like a "psycic" mode though, so this should be configurable.
				if (m_ChatWindows.ContainsKey(message.From.Bare) || (message.Body != null || message.Html != null )) {
					ChatWindowController window = OpenChatWindow(message.From, false);
					window.AppendMessage(message);
				}
			}
		}	
		
		void HandleOnPresence (object o, Presence presence)
		{
			if (m_ChatWindows.ContainsKey(presence.From.Bare)) {
				var window = m_ChatWindows[presence.From.Bare];
				window.SetPresence(presence);
			}
		}
			
		void HandleOnJoin(Room room)
		{
			if (!m_MucWindows.ContainsKey(room)) {
				var window = new MucWindowController(m_Account, room);
				window.Closed += HandleMucWindowClosed;
				m_MucWindows[room] = window;
				
				ServiceManager.Get<GuiService>().RaiseChatWindowOpened(window, true);
			}
		}

		void HandleChatWindowClosed(object sender, EventArgs e)
		{
			var window = (ChatWindowController)sender;
			m_ChatWindows.Remove(window.Jid.Bare);

			ServiceManager.Get<GuiService>().RaiseChatWindowClosed(window);
		}
		
		void HandleMucWindowClosed(object sender, EventArgs e)
		{
			Room room = ((MucWindowController)sender).Room;
			var window = m_MucWindows[room];
			m_MucWindows.Remove(room);
			
			ServiceManager.Get<GuiService>().RaiseChatWindowClosed(window);
		}
	}
}
