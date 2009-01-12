//
// ChatContentNotification.cs
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
	public class ChatContentNotification : ChatContentMessage
	{
		NotificationType m_NotificationType;
		
		public ChatContentNotification(Account account, JID source, JID destination, DateTime date, NotificationType type)
			: base (account, source, destination, date)
		{
			m_NotificationType = type;
		}

		public NotificationType NotificationType {
			get {
				return m_NotificationType;
			}
		}
		
		public override ChatContentType Type {
			get {
				return ChatContentType.Notification;
			}
		}
	}

	public enum NotificationType {
		Default
	}
}
