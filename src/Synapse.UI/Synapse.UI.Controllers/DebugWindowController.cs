//
// DebugWindowController.cs
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
using Synapse.Xmpp.Services;
using Synapse.UI.Views;

namespace Synapse.UI.Controllers
{
	public class DebugWindowController : AbstractController<IDebugWindowView>
	{
		public DebugWindowController()
		{
			Application.InvokeAndBlock(delegate {
				InitializeView();
	
				AccountService m_AccountService = ServiceManager.Get<AccountService>();
				
				m_AccountService.AccountAdded   += HandleAccountAdded;
				m_AccountService.AccountRemoved += HandleAccountRemoved;
				
				foreach (Account account in m_AccountService.Accounts)
					HandleAccountAdded(account);
			});
		}

		void HandleAccountAdded(Account account)
		{
			Application.Invoke(delegate {
				View.AddAccount(account);
			});
		}
		
		void HandleAccountRemoved(Account account)
		{
			Application.Invoke(delegate {
				View.RemoveAccount(account);
			});
		}
	}
}
