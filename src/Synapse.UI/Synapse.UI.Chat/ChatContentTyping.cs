//
// ChatContentTyping.cs
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
	public class ChatContentTyping : AbstractChatContent
	{
		TypingState m_TypingState;
		
		public ChatContentTyping (Account account, JID source, string sourceDisplayName, JID destination, TypingState state)
			: base (account, source, sourceDisplayName, destination)
		{
			m_TypingState = state;
		}

		public TypingState TypingState {
			get {
				return m_TypingState;
			}
		}
		
		public override ChatContentType Type {
			get {
				return ChatContentType.Typing;
			}
		}

		public override string MessageHtml {
			get {
				var from = base.Account.GetDisplayName(base.Source);
				switch (m_TypingState) {
				case TypingState.Active:
					return String.Format("{0} is paying attention.", from);
				case TypingState.Composing:
					return String.Format("{0} is typing...", from);
				case TypingState.Gone:
					return String.Format("{0} has left the conversation.", from);
				case TypingState.Inactive:
					return String.Format("{0} is not paying attention.", from);
				case TypingState.Paused:
					return String.Format("{0} stopped typing.", from);
				default:
					return null;
				}
			}
			set {
				throw new InvalidOperationException();
			}
		}
	}

	public enum TypingState
	{
		Active,
		Composing,
		Paused,
		Inactive,
		Gone,
		None
	}
}
