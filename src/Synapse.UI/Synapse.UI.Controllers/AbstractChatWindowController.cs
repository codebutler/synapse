
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
using Synapse.UI.Chat;
using jabber;
using jabber.connection;
using jabber.protocol.client;

namespace Synapse.UI.Controllers
{	
	public abstract class AbstractChatWindowController : AbstractController<IChatWindowView>
	{
		AbstractChatContent m_PreviousContent;
		ChatContentTyping   m_RemoteTypingState;
		
		protected Account m_Account;
		
		public abstract event EventHandler Closed;

		public Account Account {
			get {
				return m_Account;
			}
		}

		public ChatContentTyping RemoteTypingState {
			get {
				return m_RemoteTypingState;
			}
			set {
				m_RemoteTypingState = value;
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
					TypingState? state = null;
					if (child.Name == "active") {
						state = TypingState.Active;
						
					} else if (child.Name == "composing") {
						state = TypingState.Composing;
					} else if (child.Name == "paused") {
						state = TypingState.Paused;
					} else if (child.Name == "inactive") {
						state = TypingState.Inactive;
					} else if (child.Name == "gone") {
						state = TypingState.Gone;
					} else {
						Console.WriteLine(String.Format("Unknown chatstate from {0}: {1}", from, child.Name));
					}

					if (state != null) {
						var typingContent = new ChatContentTyping(m_Account, null, null, state.Value);
						
						RemoteTypingState = typingContent;

						// FIXME: It might be nice to offer this as an option, 
						// but without a JS method to remove the last object, 
						// it doesnt work well at all.
						//AppendContent(content);
					} else {
						RemoteTypingState = null;
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

				// FIXME: Add support for delayed message timestamps.
				DateTime date = DateTime.Now;
				
				var content = new ChatContentMessage(m_Account, fromJid, msg.To, date);
				content.IsOutgoing = (msg.From == null);
				content.MessageHtml = body;
				AppendContent(content);
			}
		}

		public void AppendStatus (string message)
		{
			var content = new ChatContentStatus(m_Account, null, null, DateTime.Now, String.Empty);
			content.MessageHtml = message;
			AppendContent(content);
		}

		void AppendContent (AbstractChatContent content)
		{			
			bool isSimilar   = m_PreviousContent != null && content.IsSimilarToContent(m_PreviousContent);
			//bool replaceLast = m_PreviousContent is ChatContentStatus && 
			//	               content is ChatContentStatus && 
			//	               ((ChatContentStatus)m_PreviousContent).CoalescingKey == ((ChatContentStatus)content).CoalescingKey;
			bool replaceLast = m_PreviousContent is ChatContentTyping;
			
			m_PreviousContent = content;
			
			Application.Invoke(delegate {
				base.View.AppendContent(content, isSimilar, false, replaceLast);
			});
		}
	}
}