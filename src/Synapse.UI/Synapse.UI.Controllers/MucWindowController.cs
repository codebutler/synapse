//
// MucWindowController.cs
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
using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.UI.Views;
using jabber;
using jabber.connection;
using jabber.protocol.client;

namespace Synapse.UI.Controllers
{
	public class MucWindowController : AbstractChatWindowController
	{
		public override event EventHandler Closed;

		MucAvatarGridModel m_GridModel;
		Room 		       m_Room;
		
		public MucWindowController(Account account, Room room)
		{
			if (account == null)
				throw new ArgumentNullException("account");
			
			if (room == null)
				throw new ArgumentNullException("room");

			m_Account = account;
			m_Room = room;
			
			room.OnRoomMessage += HandleOnRoomMessage;
			room.OnSelfMessage += HandleOnSelfMessage;

			m_GridModel = new MucAvatarGridModel(account, room);

			Application.InvokeAndBlock(delegate {base.InitializeView();
				base.View.TextEntered += HandleTextEntered;
				base.View.Closed += delegate {
					m_Room.Leave(String.Empty);
					if (Closed != null)
						Closed(this, EventArgs.Empty);
				};
				base.View.Show();
			});
		}

		public Room Room {
			get {
				return m_Room;
			}
		}

		public MucAvatarGridModel GridModel {
			get {
				return m_GridModel;
			}
		}
		
		void HandleTextEntered (string text)
		{
			m_Room.PublicMessage(text);
		}

		void HandleOnSelfMessage(object sender, Message msg)
		{
			Application.Invoke(delegate {
				AppendMessage(false, msg);
			});
		}

		void HandleOnRoomMessage(object sender, Message msg)
		{
			Application.Invoke(delegate {
				AppendMessage(true, msg);
			});
		}
	}
}