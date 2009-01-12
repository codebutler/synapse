//
// ChatContentMessage.cs
// 
// Copyright (C) 2009 Eric Butler
//
// Authors:
//   Eric Butler <eric@extremeboredom.net>
//
// Based on code from the Adium project.
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
using jabber;

namespace Synapse.UI.Chat
{	
	public class ChatContentMessage : AbstractChatContent
	{
		bool m_IsAutoReply = false;

		public ChatContentMessage (Account account, JID source, JID destination, DateTime date)
			 : this (account, source, destination, date, false)
		{			
		}
		
		public ChatContentMessage (Account account, JID source, JID destination, DateTime date, bool isAutoReply)
			: base (account, source, destination, date)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			if (destination == null)
				throw new ArgumentNullException("destination");
			
			m_IsAutoReply = isAutoReply;
		}

		public bool IsAutoReply {
			get {
				return m_IsAutoReply;
			}
		}

		public override ChatContentType Type {
			get {
				return ChatContentType.Message;
			}
		}
	}
}
