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
using System.Xml;
using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.UI.Views;
using jabber;
using jabber.connection;
using jabber.protocol.client;

namespace Synapse.UI.Controllers
{	
	public abstract class AbstractChatWindowController : AbstractController<IChatWindowView>
	{
		JID m_LastMessageJid;
		
		protected Account m_Account;
		
		public abstract event EventHandler Closed;

		// FIXME: I don't really like this method being here.
		public void AppendMessage (Message msg)
		{
			Application.Invoke(delegate {
				AppendMessage(true, msg);
			});
		}
		
		protected void AppendMessage (bool incoming, Message msg)
		{
			string iconPath = null;
			string from = null;
			string fromJid = null;

			foreach (XmlNode child in msg) {
				if (child.NamespaceURI == Namespace.ChatStates) {
					if (child.Name == "active") {
						base.View.AppendStatus("Active??", "message??");
					} else if (child.Name == "composing") {
						base.View.AppendStatus("composing??", "message??");
					} else if (child.Name == "paused") {
						base.View.AppendStatus("paused??", "message??");
					} else if (child.Name == "inactive") {
						base.View.AppendStatus("inactive??", "message??");
					} else if (child.Name == "gone") {
						base.View.AppendStatus("gone??", "message??");
					}
					
					Console.WriteLine("GOT CHAT STATE " + child.Name);
				}
			}

			if (msg.Body != null) {			
				if (msg.From == null) {
					from = m_Account.User;
					fromJid = m_Account.Jid;
					// FIXME: Set iconPath
				} else {
				 	iconPath = AvatarManager.GetAvatarPath(msg.From);
					fromJid = msg.From;
					from = (this is ChatWindowController) ? msg.From.User : msg.From.Resource;
				}
				
				Application.InvokeAndBlock(delegate {
					bool isNext = (m_LastMessageJid == fromJid);
					base.View.AppendMessage(incoming, isNext, iconPath, String.Empty, from, String.Empty,
					                        String.Empty, from, msg.Body);
					m_LastMessageJid = fromJid;
				});
			}
		}
	}
}