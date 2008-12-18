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
using Synapse.Xmpp;
using jabber;
using jabber.connection;
using jabber.protocol.client;

namespace Synapse.UI
{
	public delegate void ChatWindowOpenEvent (AbstractChatWindowController window, bool focus);
	public delegate void ChatWindowCloseEvent (AbstractChatWindowController window);

	public class AccountChatWindowManager : IDisposable
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
			account.Client.OnPresence += HandleOnPresence;
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
			// FIXME: Don't open a new window if the message is just a chatstate.
			if (message.Type == MessageType.chat) {
				ChatWindowController window = OpenChatWindow(message.From, false);
				window.AppendMessage(message);
			}
		}	
		
		void HandleOnPresence (object o, Presence presence)
		{
			if (m_ChatWindows.ContainsKey(presence.From.Bare)) {
				var window = m_ChatWindows[presence.From.Bare];

				string fromName = presence.From.User; // FIXME: Use roster nickname...

				// FIXME: Abstract this out somewhere else so it can be used in the activity feed too.
				var builder = new StringBuilder();
				if (presence.Type == PresenceType.available) {
					string show = null;
					switch (presence.Show) {
					case "away":
						show = "away";
						break;
					case "chat":
						show = "free to chat";
						break;
					case "dnd":
						show = "do not disturb";
						break;
					case "xa":
						show = "extended away";
						break;
					default:
						// Display this even though we don't know what it is.
						if (!String.IsNullOrEmpty(presence.Show))
						    show = presence.Show;
						break;
					}			
					if (show == null)
						 builder.Append(String.Format("{0} is now available", fromName));
					else
						builder.Append(String.Format("{0} is now {1}", fromName, show));
				} else if (presence.Type == PresenceType.unavailable) {
					builder.Append(String.Format("{0} is now offline", fromName));
				}

				if (!String.IsNullOrEmpty(presence.Status)) {
					builder.Append(": ");
					builder.Append(presence.Status);
				}
				builder.Append(".");
				
				window.AppendStatus(presence.Type.ToString(), builder.ToString());
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
