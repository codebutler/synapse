//
// IChatHandler.cs
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
using Synapse.Xmpp;

namespace Synapse.UI.Chat
{
	public delegate void ChatContentEventHandler (IChatHandler handler, AbstractChatContent content);
	
	public interface IChatHandler : IDisposable
	{
		event ChatContentEventHandler NewContent;
		event EventHandler ReadyChanged;
		
		Account Account {
			get;
		}

		bool Ready {
			get;
		}

		void FireQueued ();
		
		void Send (string html);
		void Send (XmlElement contentElement);
	}
}
