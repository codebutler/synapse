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
using System.Globalization;
using System.Text;
using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;
using jabber;
using jabber.protocol.iq;
using jabber.protocol.client;

namespace Synapse.UI
{
	public class RosterAvatarGridModel : IAvatarGridModel<RosterItem>, IAvatarGridEditableModel<RosterItem>
	{
		public event ItemEventHandler<RosterItem> ItemAdded;
		public event ItemEventHandler<RosterItem> ItemRemoved;
		public event ItemEventHandler<RosterItem> ItemChanged;
		public event EventHandler Refreshed;
		public event EventHandler ItemsChanged;

		AccountService m_AccountService;
		
		bool   m_ShowOffline    = false;
		bool   m_ModelUpdating  = false;
		bool   m_ShowTransports = false;
		string m_TextFilter     = null;

		List<RosterItem> m_Items = new List<RosterItem>();
		Dictionary<string, int> m_GroupIndexes = new Dictionary<string, int>();
		
		public RosterAvatarGridModel()
		{
			m_AccountService = ServiceManager.Get<AccountService>();
			m_AccountService.AccountAdded += OnAccountAdded;
			foreach (Account account in m_AccountService.Accounts)
				OnAccountAdded(account);
		}

		#region Public Properties
		public bool ShowTransports {
			get {
				return m_ShowTransports;
			}
			set {
				m_ShowTransports = value;
				OnItemsChanged();
			}
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
		
		public IEnumerable<RosterItem> Items {
			get {
				return m_Items;
			}
		}
		#endregion

		#region Public Methods
		public IEnumerable<string> GetItemGroups(RosterItem item)
		{
			return item.Item.GetGroups().Select(g => g.GroupName);
		}
		
		public int GetGroupOrder (string groupName)
		{
			if (!m_GroupIndexes.ContainsKey(groupName)) {
				int num = m_GroupIndexes.Count > 0 ? (m_GroupIndexes.Values.Max() + 1) : 1;
				m_GroupIndexes[groupName] = num;
			}
			
			return m_GroupIndexes[groupName];			
		}

		public void SetGroupOrder (string groupName, int groupOrder)
		{
			m_GroupIndexes[groupName] = groupOrder;
		}

		public void AddItemToGroup(RosterItem item, string groupName)
		{
			item.Item.AddGroup(groupName);
			item.Account.Roster.Modify(item.Item);
			OnItemChanged(item);
		}
		
		public void RemoveItemFromGroup(RosterItem item, string groupName)
		{
			item.Item.RemoveGroup(groupName);
			item.Account.Roster.Modify(item.Item);
			OnItemChanged(item);
		}
		
		// FIXME: This needs to be cached.
		public IEnumerable<RosterItem> GetItemsInGroup (string groupName)
		{
			foreach (var item in Items) {
				if (item.Item.HasGroup(groupName)) {
					yield return item;
				}
			}
		}

		public object GetImage (RosterItem item)
		{
			return AvatarManager.GetAvatar(item.Item.JID);
		}

		public string GetName (RosterItem item)
		{
			return item.Account.GetDisplayName(item.Item.JID);
		}

		public JID GetJID (RosterItem item)
		{
			return item.Item.JID;
		}

		public bool IsVisible (RosterItem item)
		{
			//bool showOffline = !String.IsNullOrEmpty(m_TextFilter) ? true : m_ShowOffline;
			bool showOffline = m_ShowOffline;
			return (String.IsNullOrEmpty(m_TextFilter) || MatchesFilter(item)) &&
			       (m_ShowTransports || !String.IsNullOrEmpty(item.Item.JID.User)) &&
				   (showOffline ? true : item.Account.PresenceManager.IsAvailable(item.Item.JID));
		}

		public string GetPresenceInfo (RosterItem item)
		{
			var builder = new StringBuilder();
			var presences = item.Account.PresenceManager.GetAll(item.Item.JID);
			if (presences.Length == 1) {
				var presence = presences[0];
				builder.AppendFormat("\n");
				builder.Append(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Helper.GetPresenceDisplay(presence)));
				if (!String.IsNullOrEmpty(presence.Status)) {
					builder.Append(" - ");
				    builder.Append(presence.Status);
				}	
			} else if (presences.Length > 1) {
				foreach (var presence in presences) {
					builder.AppendFormat("\n{0}: {1}",
					                     presence.From.Resource,
					                     Helper.GetPresenceDisplay(presence));
					if (!String.IsNullOrEmpty(presence.Status)) {
						builder.Append(" - ");
					    builder.Append(presence.Status);
					}
				}
			}
			return builder.ToString();
		}
		#endregion

		#region Protected Methods
		protected virtual void OnItemAdded (Account account, Item item)
		{
			var ritem = new RosterItem(account, item);
			m_Items.Add(ritem);
			
			var evnt = ItemAdded;
			if (evnt != null)
				evnt(this, ritem);
		}

		protected virtual void OnItemRemoved (Account account, Item item)
		{
			var ritem = FindRosterItem(account, item);
			m_Items.Remove(ritem);
			
			var evnt = ItemRemoved;
			if (evnt != null)
				evnt(this, ritem);
		}

		protected virtual void OnItemChanged (Account account, Item item)
		{
			OnItemChanged(FindRosterItem(account, item));
		}
		
		protected virtual void OnItemChanged (RosterItem item)
		{
			var evnt = ItemChanged;
			if (evnt != null)
				evnt(this, item);
		}

		protected virtual void OnRefreshed ()
		{
			m_Items.Clear();
			foreach (Account account in m_AccountService.Accounts) {
				foreach (JID jid in account.Roster) {
					var item = account.Roster[jid];
					var ritem = new RosterItem(account, item);
					m_Items.Add(ritem);
				}
			}
			
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
		#endregion

		#region Private Methods
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
				if (FindRosterItem(account, ri) == null) {
					OnItemAdded(account, ri);
				} else {
					OnItemChanged(account, ri);
				}
			};
			
			account.Client.OnPresence += delegate(object sender, Presence pres) {
				Item item = account.Roster[pres.From.BareJID];			
				if (item != null) {
					if (FindRosterItem(account, item) == null) {
						OnItemAdded(account, item);
					} else {
						OnItemChanged(account, item);
					}
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

		bool MatchesFilter(RosterItem item)
		{
			string name = GetName(item);
			JID jid = GetJID(item);
			bool matchesName = (name != null) ? name.ToLower().Contains(m_TextFilter.ToLower()) : false;
			bool matchesJid = (jid != null && jid.User != null) ? jid.User.Contains(m_TextFilter.ToLower()) : false;
			return matchesName || matchesJid;
		}
		
		void HandleOnDisconnect(object sender)
		{
			OnRefreshed();
		}

		RosterItem FindRosterItem (Account account, Item item)
		{
			foreach (RosterItem ritem in m_Items)
				if (ritem.Account == account && ritem.Item.JID.Equals(item.JID))
				    return ritem;
			return null;				
		}
		#endregion
	}
	
	public class RosterItem
	{
		Account m_Account;
		Item    m_Item;
		
		public RosterItem (Account account, Item item)
		{
			m_Account = account;
			m_Item    = item;
		}

		public Account Account {
			get {
				return m_Account;
			}
		}
		
		public Item Item {
			get {
				return m_Item;
			}
		}
	}
}
