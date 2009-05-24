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
using System.Text;
using Synapse.Xmpp;
using jabber;
using jabber.connection;
using jabber.protocol;
using jabber.protocol.client;
using jabber.protocol.iq;
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
			
			m_Room.OnRoomMessage      += HandleOnRoomMessage;
			m_Room.OnSelfMessage      += HandleOnSelfMessage;
			m_Room.OnJoin             += HandleOnJoin;
			m_Room.OnLeave            += HandleOnLeave;
			m_Room.OnParticipantJoin  += HandleOnParticipantJoin;
			m_Room.OnParticipantLeave += HandleOnParticipantLeave;
			m_Room.OnSubjectChange    += HandleOnSubjectChange;
			m_Room.OnPresenceError    += HandleOnPresenceError;
			m_Room.OnRoomConfig       += HandleOnRoomConfig;
			m_Room.OnAdminMessage     += HandleOnAdminMessage;
			m_Room.OnParticipantPresenceChange += HandleOnPresenceChange;
			m_Room.OnPresenceChange  += HandleOnPresenceChange;
			account.Client.OnIQ += HandleOnIQ;
			account.ConnectionStateChanged +=HandleConnectionStateChanged; 
		}

		void HandleConnectionStateChanged(Account account)
		{
			if (account.ConnectionState == AccountConnectionState.Disconnected) {
				if (base.Ready) {
					base.AppendStatus("You have been disconnected.");
					base.Ready = false;
				}
			}
		}

		void HandleOnIQ(object sender, IQ iq)
		{
			if (iq.From == m_Room.JID && iq.Type == IQType.error) {
				base.AppendStatus(String.Format("Error: {0}", iq.Error.Condition));
			}
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
			m_Room.OnRoomMessage      -= HandleOnRoomMessage;
			m_Room.OnSelfMessage      -= HandleOnSelfMessage;
			m_Room.OnJoin             -= HandleOnJoin;
			m_Room.OnLeave            -= HandleOnLeave;
			m_Room.OnParticipantJoin  -= HandleOnParticipantJoin;
			m_Room.OnParticipantLeave -= HandleOnParticipantLeave;
			m_Room.OnSubjectChange    -= HandleOnSubjectChange;
			m_Room.OnPresenceError    -= HandleOnPresenceError;
			m_Room.OnRoomConfig       -= HandleOnRoomConfig;
			m_Room.OnAdminMessage     -= HandleOnAdminMessage;
			m_Room.OnPresenceChange   -= HandleOnPresenceChange;
			m_Room.OnParticipantPresenceChange -= HandleOnPresenceChange;
			base.Account.Client.OnIQ -= HandleOnIQ;
			
			if (m_Room.IsParticipating)
				m_Room.Leave(String.Empty); // FIXME: Add option to set reason?
		}

		void HandleOnPresenceChange(Room room, RoomParticipant participant, Presence oldPresence)
		{
			var x1 = (UserX)oldPresence["x", URI.MUC_USER];
			var x2 = (UserX)participant.Presence["x", URI.MUC_USER];
			
			var oldAffiliation = x1.RoomItem.Affiliation;
			var newAffiliation = x2.RoomItem.Affiliation;
			
			var oldRole = x1.RoomItem.Role;
			var newRole = x2.RoomItem.Role;
			
			var nick = (room.Nickname == participant.Nick) ? "You are" :  participant.Nick + " is";
			
			if (newAffiliation != oldAffiliation) {
				switch (newAffiliation) {
				case RoomAffiliation.admin:
					base.AppendStatus(String.Format("{0} now a room admin.", nick));
					break;
				case RoomAffiliation.member:
					// "is now a room member" sounds weird...so we show something more useful if possible.
					switch (oldAffiliation) {
					case RoomAffiliation.admin:
						base.AppendStatus(String.Format("{0} longer a room admin.", nick));
						break;
					case RoomAffiliation.owner:
						base.AppendStatus(String.Format("{0} no longer a room owner.", nick));
						break;
					default:
						base.AppendStatus(String.Format("{0} now a room member.", nick));
						break;
					}
					break;
				case RoomAffiliation.outcast:
					base.AppendStatus(String.Format("{0} now banned from the room.", nick));
					break;
				case RoomAffiliation.owner:
					base.AppendStatus(String.Format("{0}  now a room owner.", nick));
					break;
				case RoomAffiliation.none:
					switch (oldAffiliation) {
					case RoomAffiliation.admin:
						base.AppendStatus(String.Format("{0} no longer a room admin.", nick));
						break;
					case RoomAffiliation.owner:
						base.AppendStatus(String.Format("{0} no longer a room owner.", nick));
						break;
					case RoomAffiliation.member:
						base.AppendStatus(String.Format("{0} no longer a room member.", nick));
						break;
					case RoomAffiliation.outcast:
						base.AppendStatus(String.Format("{0} no longer banned from the room.", nick));
						break;
					}
					break;
				}
			} else if (newRole != oldRole) {
				switch (newRole) {
				case RoomRole.moderator:
					base.AppendStatus(String.Format("{0} now a moderator.", nick));
					break;
				case RoomRole.participant:
					base.AppendStatus(String.Format("{0} now a participant.", nick));
					break;
				case RoomRole.visitor:
					base.AppendStatus(String.Format("{0} now a visitor.", nick));
					break;
				}
			} else if (participant.Presence.Show != oldPresence.Show || participant.Presence.Status != oldPresence.Status) {
				if (participant.Presence.Type == PresenceType.available) {
					if (!String.IsNullOrEmpty(participant.Presence.Status))
						base.AppendStatus(String.Format("{0} now {1}: {2}.", nick, Helper.GetPresenceDisplay(participant.Presence), participant.Presence.Status));
					else
						base.AppendStatus(String.Format("{0} now {1}.", nick, Helper.GetPresenceDisplay(participant.Presence)));
				}
			}
		}
		
		void HandleOnSubjectChange(object sender, Message msg)
		{
			// FIXME: Show who set the subject. Handle null subject.
			base.AppendStatus(String.Format("Subject is now: {0}", m_Room.Subject));
		}

		void HandleOnParticipantLeave(Room room, RoomParticipant participant)
		{
			string reason = null;
			RoomStatus status = RoomStatus.UNKNOWN;
			GetReasonAndStatus(participant.Presence, out reason, out status);
			
			var builder = new StringBuilder();
			builder.Append(participant.Nick);
			
			if (status == RoomStatus.KICKED)
				builder.Append(" was kicked from the room");	
			else if (status == RoomStatus.BANNED)
				builder.Append(" has been banned from the room");
			else
				builder.Append(" left the room");
			
			if (!String.IsNullOrEmpty(reason)) {
				builder.Append(": ");
				builder.Append(reason);
			}
			
			builder.Append(".");
			
			base.AppendStatus(builder.ToString());
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
		
		void HandleOnAdminMessage(object sender, Message msg)
		{
			// FIXME: When does this happen?
			base.AppendStatus("Admin Message: " + msg.Body);
		}

		IQ HandleOnRoomConfig(Room room, IQ parent)
		{
			// FIXME:
			Console.WriteLine("Show room configuration dialog...");
			return null;
		}

		void HandleOnPresenceError(Room room, Presence pres)
		{
			base.AppendStatus("Error: " + pres.Error.Condition);
		}

		void HandleOnJoin(Room room)
		{
			base.Ready = true;
		}
		
		void HandleOnLeave(Room room, Presence presence)
		{
			string reason = null;
			RoomStatus status = RoomStatus.UNKNOWN;
			GetReasonAndStatus(presence, out reason, out status);
			
			var builder = new StringBuilder();
			builder.Append("You have");
			if (status == RoomStatus.KICKED)
				builder.Append(" been kicked from the room");	
			else if (status == RoomStatus.BANNED)
				builder.Append(" been banned from the room");
			else
				builder.Append(" left the room");
			
			if (!String.IsNullOrEmpty(reason)) {
				builder.Append(": ");
				builder.Append(reason);
			} 
			
			builder.Append(".");
			
			base.AppendStatus(builder.ToString());
			
			base.Ready = false;
		}
		
		void GetReasonAndStatus (Presence presence, out string reason, out RoomStatus status)
		{
			reason = null;
			status = RoomStatus.UNKNOWN;
			
			UserX x = (UserX)presence["x", jabber.protocol.URI.MUC_USER];
			if (x != null) {
				if (x.RoomItem != null)
					reason = x.RoomItem.Reason;
				
				if (x.Status != null && x.Status.Length > 0)
					status = x.Status[0];
			}
		}
	}
}
