//
// AvatarGridModel.cs
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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Xmpp;
using jabber;
using jabber.protocol.iq;
using jabber.protocol.client;

namespace Synapse.UI
{
	public class RosterAvatarGridModel : IAvatarGridModel<AccountItemPair>
	{
		public event ItemEventHandler<AccountItemPair> ItemAdded;
		public event ItemEventHandler<AccountItemPair> ItemRemoved;
		public event ItemEventHandler<AccountItemPair> ItemChanged;
		public event EventHandler Refreshed;
		public event EventHandler ItemsChanged;

		AccountService m_AccountService;
		
		bool   m_ShowOffline   = false;
		bool   m_ModelUpdating = false;
		string m_TextFilter    = null;
			
		public RosterAvatarGridModel()
		{
			m_AccountService = ServiceManager.Get<AccountService>();
			m_AccountService.AccountAdded += OnAccountAdded;
			foreach (Account account in m_AccountService.Accounts)
				OnAccountAdded(account);
		}

		public bool ModelUpdating {
			get {
				return m_ModelUpdating;
			}
		}

		public bool ShowOffline {
			get {
				return m_ShowOffline;
			}
			set {
				m_ShowOffline = value;
				OnItemsChanged();
			}
		}

		public string TextFilter {
			get {
				return m_TextFilter;
			}
			set {
				m_TextFilter = value;
				OnItemsChanged();
			}
		}
		
		public IEnumerable<AccountItemPair> Items {
			get {
				foreach (Account account in m_AccountService.Accounts) {
					foreach (JID jid in account.Roster) {
						Item item = account.Roster[jid];
						yield return new AccountItemPair(account, item);
					}
				}
			}
		}
		
		public IEnumerable<string> GetItemGroups(AccountItemPair pair)
		{
			return pair.Item.GetGroups().Select(g => g.GroupName);
		}

		public int GetGroupOrder (string groupName)
		{
			return 0;
		}

		public object GetImage (AccountItemPair pair)
		{
			return AvatarManager.GetAvatar(pair.Item.JID);
		}

		public string GetName (AccountItemPair pair)
		{
			return !String.IsNullOrEmpty(pair.Item.Nickname) ? pair.Item.Nickname : pair.Item.JID.User;
		}

		public JID GetJID (AccountItemPair pair)
		{
			return pair.Item.JID;
		}

		public string GetPresence (AccountItemPair pair)
		{
			return String.Empty;
		}

		public string GetPresenceMessage (AccountItemPair pair)
		{
			return String.Empty;
		}
		
		public bool IsVisible (AccountItemPair pair)
		{
			bool showOffline = !String.IsNullOrEmpty(m_TextFilter) ? true : m_ShowOffline;
			return (String.IsNullOrEmpty(m_TextFilter) || MatchesFilter(pair)) && (showOffline ? true : pair.Account.PresenceManager.IsAvailable(pair.Item.JID));
		}

		protected virtual void OnItemAdded (Account account, Item item)
		{
			var evnt = ItemAdded;
			if (evnt != null)
				evnt(this, new AccountItemPair(account, item));
		}

		protected virtual void OnItemRemoved (Account account, Item item)
		{
			var evnt = ItemRemoved;
			if (evnt != null)
				evnt(this, new AccountItemPair(account, item));
		}

		protected virtual void OnItemChanged (Account account, Item item)
		{
			var evnt = ItemChanged;
			if (evnt != null)
				evnt(this, new AccountItemPair(account, item));
		}

		protected virtual void OnRefreshed ()
		{
			var evnt = Refreshed;
			if (evnt != null)
				evnt(this, EventArgs.Empty);
		}

		protected virtual void OnItemsChanged ()
		{
			var evnt = ItemsChanged;
			if (evnt != null)
				evnt(this, EventArgs.Empty);
		}
		
		void OnAccountAdded (Account account)
		{
			account.Roster.OnRosterBegin += delegate(object sender) {
				m_ModelUpdating = true;
			};

			account.Roster.OnRosterEnd += delegate(object sender) {
				m_ModelUpdating = false;
				OnRefreshed();
			};
			
			account.Roster.OnRosterItem += delegate(object sender, Item ri) {
				OnItemAdded(account, ri);
			};
			
			account.Client.OnPresence += delegate(object sender, Presence pres) {
				Item item = account.Roster[pres.From.BareJID];
				if (item != null) {
					OnItemChanged(account, item);
				}
			};
			
			account.Client.OnDisconnect += HandleOnDisconnect;
			
			account.PresenceManager.OnPrimarySessionChange += delegate(object sender, JID bare) {
				/*
				if (account.PresenceManager.IsAvailable(bare)) {
					if (ItemAdded != null)
						ItemAdded(this, account, 
				}
				*/
				//Console.WriteLine("Primary !?!?! " + bare + " " + account.PresenceManager.IsAvailable(bare));
			};
		}

		bool MatchesFilter(AccountItemPair pair)
		{
			string name = GetName(pair);
			JID jid = GetJID(pair);
			bool matchesName = (name != null) ? name.ToLower().Contains(m_TextFilter.ToLower()) : false;
			bool matchesJid = (jid != null && jid.User != null) ? jid.User.Contains(m_TextFilter.ToLower()) : false;
			return matchesName || matchesJid;
		}
		
		void HandleOnDisconnect(object sender)
		{
			OnRefreshed();
		}
	}
	
	public class AccountItemPair : Pair<Account, Item>
	{
		public AccountItemPair (Account account, Item item) : base (account, item)
		{
		}

		public Account Account {
			get {
				return base.First;
			}
		}
		
		public Item Item {
			get {
				return base.Second;
			}
		}
	}
}
