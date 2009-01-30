//
// AbstractChatHandler.cs
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
using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.UI;
using Synapse.UI.Services;
using Synapse.Xmpp;
using jabber;
using jabber.protocol.client;

namespace Synapse.UI.Chat
{
	public abstract class AbstractChatHandler : IChatHandler
	{
		public event ChatContentEventHandler NewContent;
		public event EventHandler ReadyChanged;
		
		Account m_Account;
		bool m_Ready = true;

		protected AbstractChatHandler (Account account)
		{
			m_Account = account;
		}
		
		public Account Account {
			get {
				return m_Account;
			}
		}

		public bool Ready {
			get {
				return m_Ready;
			}
			protected set {
				m_Ready = value;
				if (ReadyChanged != null)
					ReadyChanged(this, EventArgs.Empty);
			}
		}

		public abstract void Start ();
		public abstract void Send (string html);
		public abstract void Send (XmlElement element);
		public abstract void Dispose ();
	
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
				if (this is MucHandler) {
					// FIXME: Abstract this...
					var participant = ((MucHandler)this).Room.Participants[msg.From];
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
					TypingState state = TypingState.None;
					if (child.LocalName == "active") {
						state = TypingState.Active;					
					} else if (child.LocalName == "composing") {
						state = TypingState.Composing;
					} else if (child.LocalName == "paused") {
						state = TypingState.Paused;
					} else if (child.LocalName == "inactive") {
						state = TypingState.Inactive;
					} else if (child.LocalName == "gone") {
						state = TypingState.Gone;
					} else {
						Console.WriteLine(String.Format("Unknown chatstate from {0}: {1}", from, child.LocalName));
					}
	
					var typingContent = new ChatContentTyping(m_Account, null, null, state);
					NewContent(this, typingContent);
				}
			}
			
			if (msg.Body != null || msg.Html != null) {			
				string body = null;

				var guiService = ServiceManager.Get<GuiService>();
				foreach (var formatter in guiService.MessageDisplayFormatters) {
					body = formatter.FormatMessage(msg);
					if (body != null)
						break;
				}
				
				if (body == null) {		
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
				}
					
				// FIXME: Add support for delayed message timestamps.
				DateTime date = DateTime.Now;
				
				var content = new ChatContentMessage(m_Account, fromJid, msg.To, date);
				content.IsOutgoing = !incoming;
				content.MessageHtml = body;
				
				NewContent(this, content);
			}
		}
	
		protected void AppendStatus (string message)
		{
			var content = new ChatContentStatus(m_Account, null, null, DateTime.Now, String.Empty);
			content.MessageHtml = message;

			NewContent(this, content);
		}
	}
}
