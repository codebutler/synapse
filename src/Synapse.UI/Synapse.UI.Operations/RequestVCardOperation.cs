//
// VCardRequest.cs
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
using jabber;
using jabber.protocol.client;
using jabber.protocol.iq;
using jabber.protocol;
using Synapse.Xmpp;
using Synapse.UI.Controllers;

namespace Synapse.UI.Operations
{
	public class RequestVCardOperation : AbstractIqOperation
	{
		JID m_JID;
		UserProfileWindowController m_Controller;
		
		public RequestVCardOperation (Account account, JID jid) : base (account)
		{
			m_JID = jid;
			base.ReplyReceived += HandleReplyReceived;
		}

		public override string Name { 
			get { return "Request VCard"; } 
		}
		
		public override string Description { 
			get { return m_JID.ToString(); }
		}

		public override void Start ()
		{
			base.Status = OperationStatus.WaitingForReply;

			m_Controller = new UserProfileWindowController();
			
			VCardIQ iq = new VCardIQ(base.Account.Client.Document);
			base.ID = iq.ID;
			iq.Type = IQType.get;
			iq.To = m_JID;
			iq.AddChild(new VCard(base.Account.Client.Document));
			base.Account.Client.Write(iq);
		}

		void HandleReplyReceived(IQ iq)
		{
			if (iq.Type == IQType.result) {
				m_Controller.Populate((VCard)iq.FirstChild);
			} else if (iq.Type == IQType.error) {
				m_Controller.Populate(null);
			}
		}
	}
}
