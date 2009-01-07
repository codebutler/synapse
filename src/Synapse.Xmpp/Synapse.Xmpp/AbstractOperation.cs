//
// ActionBase.cs
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
using jabber.protocol;
using Synapse.Xmpp.Services;

namespace Synapse.Xmpp
{
	public abstract class AbstractOperation : IOperation
	{
		OperationStatus m_Status;
		Account      m_Account;
		string       m_MessageID;
		string       m_StackTrace;
		DateTime     m_StartedAt;

		public event OperationEventHandler StatusChanged;
		
		public AbstractOperation(Account account)
		{
			m_Account    = account;
			m_Status     = OperationStatus.Pending;
			m_StackTrace = Environment.StackTrace;
			m_StartedAt  = DateTime.Now;
		}

		public virtual void Start ()
		{
			Status = OperationStatus.WaitingForReply;
		}

		public abstract bool CheckReply(Packet packet);

		public Account Account {
			get {
				return m_Account;
			}
		}

		public string MessageID {
			get {
				return m_MessageID;
			}
			protected set {
				if (String.IsNullOrEmpty(value))
					throw new ArgumentNullException("value");
				m_MessageID = value;
			}
		}

		public OperationStatus Status {
			get {
				return m_Status;
			}
			protected set {
				m_Status = value;
				if (StatusChanged != null)
					StatusChanged(this);
			}
		}

		public DateTime StartedAt {
			get {
				return m_StartedAt;
			}
		}

		public string StackTrace {
			get {
				return m_StackTrace;
			}
		}

		public abstract string Name {
			get;
		}

		public abstract string Description {
			get;
		}
	}
}
