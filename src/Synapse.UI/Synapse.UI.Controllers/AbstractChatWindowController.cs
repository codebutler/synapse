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
using Synapse.Core;
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

		public Account Account {
			get {
				return m_Account;
			}
		}

		// FIXME: I don't really like this method being here.
		public void AppendMessage (Message msg)
		{
			AppendMessage(true, msg);
		}
		
		protected void AppendMessage (bool incoming, Message msg)
		{
			string iconPath = null;
			string from = null;
			JID fromJid = null;
			
			if (msg.From == null) {
				from = m_Account.User;
				fromJid = m_Account.Jid;
			} else {
				if (this is MucWindowController) {
					// FIXME: Abstract this...
					var participant = ((MucWindowController)this).Room.Participants[msg.From];
					if (participant != null) {
						fromJid = (!String.IsNullOrEmpty(participant.RealJID)) ? participant.RealJID : participant.NickJID;
						from = participant.Nick;
					} else {
						fromJid = msg.From;
						from = msg.From.Resource;
					}
				} else {
					from = m_Account.GetDisplayName(msg.From);
					fromJid = msg.From;
				}
			}
			
			foreach (XmlNode child in msg) {
				if (child.NamespaceURI == Namespace.ChatStates) {
					if (child.Name == "active") {
						AppendStatus("active", String.Format("{0} is paying attention.", from));
					} else if (child.Name == "composing") {
						AppendStatus("composing", String.Format("{0} is typing...", from));
					} else if (child.Name == "paused") {
						AppendStatus("paused", String.Format("{0} stopped typing.", from));
					} else if (child.Name == "inactive") {
						AppendStatus("inactive", String.Format("{0} is not paying attention.", from));
					} else if (child.Name == "gone") {
						AppendStatus("gone", String.Format("{0} has left the conversation.", from));
					} else {
						Console.WriteLine(String.Format("Unknown chatstate from {0}: {1}", from, child.Name));
					}
				}
			}
			
			if (msg.Body != null || msg.Html != null) {			
				string body = null;
				if (!String.IsNullOrEmpty(msg.Html)) {
					// FIXME: Better sanitize this somehow...
					body = msg.Html;
				} else {
					body = Util.EscapeHtml(msg.Body);
					body = Util.Linkify(body);
					body = body.Replace("  ", " &nbsp;");
					body = body.Replace("\t", " &nbsp;&nbsp;&nbsp;");
					body = body.Replace("\r\n", "<br/>");
					body = body.Replace("\n", "<br/>");
				}
			
				iconPath = String.Format("avatar:/{0}", AvatarManager.GetAvatarHash(fromJid.Bare));
			
				Application.Invoke(delegate {	
					bool isNext = (m_LastMessageJid == fromJid);
					base.View.AppendMessage(incoming, isNext, iconPath, String.Empty, from, String.Empty,
					                        String.Empty, from, body);
					m_LastMessageJid = fromJid;
				});
			}
		}
		
		public void AppendStatus (string status, string message)
		{
			m_LastMessageJid = null;
			Application.Invoke(delegate {
				base.View.AppendStatus(status, message);
			});
		}
	}
}