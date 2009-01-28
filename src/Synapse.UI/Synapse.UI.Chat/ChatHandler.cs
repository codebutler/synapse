//
// ChatHandler.cs
// 
// Copyright (C) 2009 Eric Butler
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
using System.Xml;
using Synapse.Xmpp;
using jabber;
using jabber.protocol.client;

namespace Synapse.UI.Chat
{
	public class ChatHandler : AbstractChatHandler
	{
		JID m_Jid;
		
		public ChatHandler (Account account, JID jid)
			: base (account)
		{
			m_Jid = jid;
		}

		public override void Start()
		{
			base.Account.ConnectionStateChanged += HandleConnectionStateChanged;
			base.Ready = (base.Account.ConnectionState == AccountConnectionState.Connected);
		}
		
		public JID Jid {
			get {
				return m_Jid;
			}
		}
		
		public void SetPresence (Presence presence)
		{
			string message = null;
			string fromName = base.Account.GetDisplayName(presence.From);
			if (!String.IsNullOrEmpty(presence.Status)) {
				message = String.Format("{0} is now {1}: {2}.", fromName, Helper.GetPresenceDisplay(presence), presence.Status);
			} else {
				message = String.Format("{0} is now {1}.", fromName, Helper.GetPresenceDisplay(presence));
			}
			
			base.AppendStatus(message);
		}

		public override void Send (string text)
		{	
			if (!String.IsNullOrEmpty(text)) {
				Message message = new Message(base.Account.Client.Document);
				message.Type = MessageType.chat;
				message.To = m_Jid;
				message.Body = text;

				var activeElem = base.Account.Client.Document.CreateElement("active");
				activeElem.SetAttribute("xmlns", "http://jabber.org/protocol/chatstates");
				message.AppendChild(activeElem);

				base.Account.Client.Write(message);
				base.AppendMessage(false, message);
			}			
		}

		public override void Send (XmlElement contentElement)
		{
			Message message = new Message(base.Account.Client.Document);
			message.Type = MessageType.chat;
			message.To = m_Jid;

			message.AppendChild(contentElement);

			var activeElem = base.Account.Client.Document.CreateElement("active");
			activeElem.SetAttribute("xmlns", "http://jabber.org/protocol/chatstates");
			message.AppendChild(activeElem);

			base.Account.Client.Write(message);
			base.AppendMessage(false, message);
		}

		public override void Dispose ()
		{
			base.Account.ConnectionStateChanged -= HandleConnectionStateChanged;
		}

		void HandleConnectionStateChanged(Account account)
		{
			if (account.ConnectionState == AccountConnectionState.Connected) {
				AppendStatus("You are now online.");
				base.Ready = true;
			} else if (account.ConnectionState == AccountConnectionState.Disconnected) {
				AppendStatus("You are now offline.");
				base.Ready = false;
			}
		}
	}
}
