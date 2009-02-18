//
// MucHandler.cs
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
using jabber;
using jabber.connection;
using jabber.protocol.client;
using Synapse.UI;

namespace Synapse.UI.Chat
{
	public class MucHandler : AbstractChatHandler
	{
		Room m_Room;
		MucAvatarGridModel m_GridModel;

		public MucHandler (Account account, Room room)
			: base (account)
		{
			m_Room = room;
			m_GridModel = new MucAvatarGridModel(account, room);
			
			m_Room.OnRoomMessage += HandleOnRoomMessage;
			m_Room.OnSelfMessage += HandleOnSelfMessage;
			m_Room.OnParticipantJoin += HandleOnParticipantJoin;
			m_Room.OnParticipantLeave += HandleOnParticipantLeave;
			m_Room.OnSubjectChange += HandleOnSubjectChange;
			// FIXME: Handle room.OnLeave .. kicks... everything else..
		}
		
		public override void Send (string html)
		{
			if (!String.IsNullOrEmpty(html)) {
				Message message = new Message(base.Account.Client.Document);
				message.Type = MessageType.groupchat;
				message.To = m_Room.JID;
				message.Html = html;

				var activeElem = base.Account.Client.Document.CreateElement("active");
				activeElem.SetAttribute("xmlns", "http://jabber.org/protocol/chatstates");
				message.AppendChild(activeElem);

				base.Account.Client.Write(message);
				//base.AppendMessage(false, message);
			}
		}

		public override void Send (XmlElement contentElement)
		{
			Message message = new Message(base.Account.Client.Document);
			message.Type = MessageType.groupchat;
			message.To = m_Room.JID;

			message.AppendChild(contentElement);

			var activeElem = base.Account.Client.Document.CreateElement("active");
			activeElem.SetAttribute("xmlns", "http://jabber.org/protocol/chatstates");
			message.AppendChild(activeElem);

			base.Account.Client.Write(message);
			//base.AppendMessage(false, message);
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

		public override void Dispose ()
		{
			m_Room.OnRoomMessage -= HandleOnRoomMessage;
			m_Room.OnSelfMessage -= HandleOnSelfMessage;
			m_Room.OnParticipantJoin -= HandleOnParticipantJoin;
			m_Room.OnParticipantLeave -= HandleOnParticipantLeave;
			m_Room.OnSubjectChange -= HandleOnSubjectChange;
			m_Room.Leave(String.Empty); // FIXME: Add option to set reason?
		}

		void HandleOnSubjectChange(object sender, Message msg)
		{
			// FIXME: Show who set the subject. Handle null subject.
			base.AppendStatus(String.Format("Subject is now: {0}", m_Room.Subject));
		}

		void HandleOnParticipantLeave(Room room, RoomParticipant participant)
		{
			base.AppendStatus(String.Format("{0} has left the room.", participant.Nick));	
		}

		void HandleOnParticipantJoin(Room room, RoomParticipant participant)
		{
			base.AppendStatus(String.Format("{0} has joined the room.", participant.Nick));
		}

		void HandleOnSelfMessage(object sender, Message msg)
		{
			base.AppendMessage(false, msg);
		}

		void HandleOnRoomMessage(object sender, Message msg)
		{
			base.AppendMessage(true, msg);
		}
	}
}
