//
// MucAffiliationDialog.cs
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

using Qyoto;

using jabber;
using jabber.connection;
using jabber.protocol.client;
using jabber.protocol.iq;

namespace Synapse.QtClient.Windows
{
	public partial class MucAffiliationDialog : QDialog
	{
		Room m_Room;
		RoomParticipant m_Participant;
		
		public MucAffiliationDialog (Room room, RoomParticipant participant, QWidget parentWindow) : base (parentWindow)
		{
			SetupUi();
			
			m_Room = room;
			m_Participant = participant;	
			
			roomLabel.Text = room.JID.Bare;
			userLabel.Text = participant.RealJID.Bare;
			
			switch (participant.Affiliation) {
			case RoomAffiliation.owner:
				ownerRadioButton.Checked = true;
				break;
			case RoomAffiliation.admin:
				adminRadioButton.Checked = true;
				break;
			case RoomAffiliation.member:
				memberRadioButton.Checked = true;
				break;
			case RoomAffiliation.outcast:
				outcastRadioButton.Checked = true;
				break;
			default:
				noneRadioButton.Checked = true;
				break;
			}
			
			Gui.CenterWidgetOnScreen(this);
		}

		public override void Accept ()
		{
			base.Accept ();
			
			RoomAffiliation affiliation = RoomAffiliation.none;
			
			if (ownerRadioButton.Checked)
				affiliation = RoomAffiliation.owner;
			else if (adminRadioButton.Checked)
				affiliation = RoomAffiliation.admin;
			else if (memberRadioButton.Checked)
				affiliation = RoomAffiliation.member;
			else if (outcastRadioButton.Checked)
				affiliation = RoomAffiliation.outcast;
			 
			m_Room.ChangeAffiliation(m_Participant.RealJID, affiliation, reasonLineEdit.Text);		
			
			Gui.CenterWidgetOnScreen(this);
		}
	}
}