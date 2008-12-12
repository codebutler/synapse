//
// OperationService.cs
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
using System.Xml;
using System.Collections.Generic;
using Synapse.ServiceStack;
using jabber.protocol;

namespace Synapse.Xmpp
{
	public delegate void OperationEventHandler (IOperation Operation);
	
	public class OperationService : IRequiredService
	{
		List<IOperation> m_Operations;

		public event OperationEventHandler OperationAdded;
		public event OperationEventHandler OperationUpdated;

		public OperationService ()
		{
			m_Operations = new List<IOperation>();
			
			AccountService accountService = ServiceManager.Get<AccountService>();
			accountService.AccountAdded   += HandleAccountAdded;
			accountService.AccountRemoved += HandleAccountRemoved;
			foreach (Account account in accountService.Accounts)
				HandleAccountAdded(account);
		}

		void HandleAccountAdded(Account account)
		{
			account.Client.OnProtocol += HandleOnProtocol;
		}

		void HandleAccountRemoved(Account account)
		{
			account.Client.OnProtocol -= HandleOnProtocol;
		}

		void HandleOnProtocol(object sender, XmlElement rp)
		{
			m_Operations.FindAll(a => a.Status == OperationStatus.WaitingForReply).ForEach(delegate(IOperation a) {
				if (a.CheckReply((Packet)rp)) {
					// FIXME: Remove service from dict
					return;
				}
			});
		}

		public void Start (IOperation operation)
		{
			if (operation.Status != OperationStatus.Pending)
				throw new InvalidOperationException("Status must be Pending");

			operation.StatusChanged += HandleStatusChanged;
			
			m_Operations.Add(operation);
			if (OperationAdded != null)
				OperationAdded(operation);
				
			operation.Start();
		}

		void HandleStatusChanged(IOperation operation)
		{
			if (OperationUpdated != null)
				OperationUpdated(operation);
		}

		public string ServiceName {
			get {
				return "OperationService";
			}
		}

		public IList<IOperation> Operations {
			get {
				return m_Operations.AsReadOnly();
			}
		}
	}
}