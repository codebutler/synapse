//
// ChatContentContext.cs
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
using Synapse.Xmpp;
using jabber;

namespace Synapse.UI.Chat
{
	public class ChatContentContext : AbstractChatContent
	{
		public ChatContentContext(Account account, JID source, JID destination, DateTime date)
			: base (account, source, destination, date)
		{
		}
		
		public override ChatContentType Type {
			get {
				return ChatContentType.Context;
			}
		}

		public override string[] DisplayClasses {
			get {
				var classes = new List<string>();
				classes.AddRange(base.DisplayClasses);
				classes.Add("history");
				return classes.ToArray();
			}
		}
	}
}
