//
// ChatWindowController.cs
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
using Synapse.Xmpp;
using Synapse.UI.Views;
using jabber;
using jabber.protocol.client;

namespace Synapse.UI.Controllers
{
	public class ChatWindowController : AbstractChatWindowController
	{
		JID m_Jid;

		public override event EventHandler Closed;
		
		public ChatWindowController(Account account, JID jid)
		{
			if (account == null)
				throw new ArgumentNullException("account");

			if (jid == null)
				throw new ArgumentNullException("jid");
				
			m_Account = account;
			m_Jid = jid;

			InitializeView();
			base.View.Closed += delegate {
				if (Closed != null)
					Closed(this, EventArgs.Empty);
			};
			base.View.Show();
			base.View.TextEntered += HandleTextEntered;
		}
		
		public JID Jid {
			get {
				return m_Jid;
			}
		}

		private void HandleTextEntered (string text)
		{
			// FIXME: Create an action for this
			Message message = new Message(m_Account.Client.Document);
			message.Type = MessageType.chat;
			message.To = m_Jid;
			message.Body = text;
			AppendMessage(false, message);
			m_Account.Client.Write(message);
		}
	}
}
