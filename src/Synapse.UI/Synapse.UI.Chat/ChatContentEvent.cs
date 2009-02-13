//
// ChatContentEvent.cs
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
using System.Collections.Generic;
using jabber;
using Synapse.Xmpp;

namespace Synapse.UI.Chat
{
	public class ChatContentEvent : ChatContentStatus
	{
		public ChatContentEvent(Account account, JID source, string sourceDisplayName, JID destination, DateTime date, string eventType)
			: base (account, source, sourceDisplayName, destination, date, eventType)
		{
		}
		
		public override ChatContentType Type {
			get {
				return ChatContentType.Event;
			}
		}

		public override string[] DisplayClasses {
			get {
				var classes = new List<string>();
				classes.AddRange(base.DisplayClasses);
				classes.Remove("status");
				classes.Add("event");
				return classes.ToArray();;
			}
		}

		public string EventType {
			get {
				return base.StatusType;
			}
		}
	}
}
