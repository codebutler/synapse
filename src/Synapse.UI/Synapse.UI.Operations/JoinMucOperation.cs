//
// JoinMucAction.cs
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
using Synapse.Core;
using Synapse.Xmpp;
using jabber;
using jabber.connection;
using jabber.protocol.iq;
using jabber.protocol;

namespace Synapse.UI.Operations
{
	public class JoinMucOperation : AbstractOperation
	{
		JID m_Jid;
		
		public JoinMucOperation(Account account, JID roomJid) : base (account)
		{
			if (roomJid == null)
				throw new ArgumentNullException("roomJid");
			
			m_Jid = roomJid;
		}

		public override string Name { 
			get { return "Join MUC"; } 
		}
		
		public override string Description { 
			get { return m_Jid.ToString(); }
		}

		public override void Start ()
		{
			base.Status = OperationStatus.WaitingForReply;
			
			Room room = Account.ConferenceManager.GetRoom(m_Jid);
			if (!room.IsParticipating)
				room.Join();
			else
				throw new UserException("Already in this room");
		}

		public override bool CheckReply (Packet packet)
		{
			if (packet.Name == "presence" && packet.From == m_Jid) {
				base.Status = OperationStatus.Finished;
				return true;
			}
			return false;
		}
	}
}