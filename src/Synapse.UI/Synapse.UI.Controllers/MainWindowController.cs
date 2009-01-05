//
// MainWindowController.cs
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
using Synapse.UI.Views;
using Synapse.ServiceStack;
using Synapse.Services;
using Synapse.Xmpp;
using jabber;
using jabber.protocol.client;

namespace Synapse.UI.Controllers
{
	public class MainWindowController : AbstractController<IMainWindowView>
	{
		public MainWindowController()
		{
			Application.InvokeAndBlock(delegate {
				InitializeView();
				
				View.AddNewAccount   += OnAddNewAccount;
				View.PresenceChanged += OnPresenceChanged;
				View.ActivityFeedReady += HandleActivityFeedReady;
	
				AccountService accountService = ServiceManager.Get<AccountService>();
				accountService.AccountAdded   += AddAccount;
				accountService.AccountRemoved += View.RemoveAccount;
	
				foreach (Account account in accountService.Accounts)
					AddAccount(account);

				View.Show();
			});
		}

		void HandleActivityFeedReady(object sender, EventArgs e)
		{
			AccountService accountService = ServiceManager.Get<AccountService>();
			foreach (Account account in accountService.Accounts)
				account.ActivityFeed.FireQueued();
		}

		void AddAccount (Account account)
		{
			View.AddAccount(account);
			account.ActivityFeed.NewItem += delegate (Account a, ActivityFeedItem item) {
				Application.Invoke(delegate {
					View.AddActivityFeedItem(a, item);
				});
			};
		}

		#region No accounts
		private DialogValidationResult OnAddNewAccount()
		{
			DialogValidationResult result = new DialogValidationResult();

			if (String.IsNullOrEmpty(View.Login))
				result.Errors.Add("Login", "may not be empty");
			//else
			//	if (JID.IsValid(View.Login)
			//		result.Errors.Add("Login", "is not valid Jabber ID");
			
			if (String.IsNullOrEmpty(View.Password))
				result.Errors.Add("Password", "may not be empty");
			
			if (result.IsValid) {
				JID jid = new JID(View.Login);
				
				Account account = new Account(jid.User, jid.Server, "Synapse");
				account.Password = View.Password;
				AccountService service = ServiceManager.Get<AccountService>();
				service.AddAccount(account);
			}
			return result;
		}
		#endregion
		
		private void OnPresenceChanged (Account account, string presence, string statusText)
		{
			account.Status = new ClientStatus(presence, statusText);
		}
	}
}