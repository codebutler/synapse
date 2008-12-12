//
// AbstractIqOperation.cs
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
using jabber.protocol.client;
using jabber.protocol;

namespace Synapse.Xmpp
{
	public delegate void IqEventHandler (IQ iq);
	
	public abstract class AbstractIqOperation : AbstractOperation
	{
		string m_ID;

		protected event IqEventHandler ReplyReceived;

		public AbstractIqOperation (Account account) : base (account)
		{
		}

		public abstract override string Description { get; }
		public abstract override string Name { get; }
		
		public abstract override void Start();
		
		protected string ID {
			get {
				return m_ID;
			}
			set {
				if (String.IsNullOrEmpty(value))
					throw new ArgumentNullException("value");
				m_ID = value;
			}
		}
		
		public override bool CheckReply (Packet packet)
		{
			if (String.IsNullOrEmpty(m_ID))
				throw new InvalidOperationException("ID should have been set.");
			
			IQ iq = packet as IQ;

			bool isReply = (iq != null && iq.ID == m_ID && (iq.Type == IQType.result || iq.Type == IQType.error));
			                
			if (!isReply) {
				return false;
			}
		
			if (ReplyReceived != null)
				ReplyReceived(iq);

			if (iq.Type == IQType.result) {
				base.Status = OperationStatus.Finished;
			} else if (iq.Type == IQType.error) {
				base.Status = OperationStatus.Failed;
			}
			
			return true;
		}
	}
}
