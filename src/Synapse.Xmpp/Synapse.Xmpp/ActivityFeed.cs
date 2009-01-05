//
// ActivityFeed.cs
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
using System.Text;
using System.Collections.Generic;
using jabber;
using jabber.protocol.iq;
using Synapse.Core;

namespace Synapse.Xmpp
{
	public delegate void ActivityFeedItemEventHandler (Account account, ActivityFeedItem item);
	
	public class ActivityFeed
	{	
		Account                  m_Account;	
		Queue<ActivityFeedItem> m_Queue = new Queue<ActivityFeedItem>();
		
		public event ActivityFeedItemEventHandler NewItem;
		
		public ActivityFeed(Account account)
		{
			m_Account = account;
			
			PostItem(new ActivityFeedItem(account, null, "synapse", "Welcome to Synapse!", null));
		}
		
		public void PostItem (ActivityFeedItem item)
		{
			if (NewItem == null) {
				lock (m_Queue)
					m_Queue.Enqueue(item);
			} else {
				NewItem(m_Account, item);
			}
		}
		
		public void FireQueued () 
		{
			if (NewItem == null) {
				throw new InvalidOperationException("No event handler for NewItem");
			}
			lock (m_Queue) {
				while (m_Queue.Count > 0)
					NewItem(m_Account, m_Queue.Dequeue());
			}
		}
	}
	
	public class ActivityFeedItem
	{
		Account  m_Account;
		JID 	 m_From;
		string   m_Type;
		string   m_ActionItem;
		string   m_Content;
		Uri      m_ContentUrl;
		
		public ActivityFeedItem (Account account, JID from, string type, string actionItem, string content)
			: this (account, from, type, actionItem, content, null)
		{
		}
		
		public ActivityFeedItem (Account account, JID from, string type, string actionItem, string content, string contentUrl)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			
			m_Account    = account;
			m_From       = from;
			m_Type       = type;
			m_ActionItem = Util.EscapeHtml(actionItem);
			m_Content    = Util.EscapeHtml(content);

			if (!String.IsNullOrEmpty(contentUrl)) {
				try {
					m_ContentUrl = new Uri(contentUrl);
					// Anything else would be scary.
					if (m_ContentUrl.Scheme != "http" && m_ContentUrl.Scheme != "https")
						m_ContentUrl = null;
				} catch {}
			}
		}

		public Account Account {
			get {
				return m_Account;
			}
		}

		public string FromJid {
			get {
				return (m_From == null) ? null : m_From.ToString();
			}
		}

		public string FromName {
			get {
				return (m_From == null) ? null : m_Account.GetDisplayName(m_From);
			}
		}

		public string AvatarUrl {
			get {
				string avatarHash = (m_From != null) ? AvatarManager.GetAvatarHash(m_From) : "octy";
				return "avatar:/" + avatarHash;
			}
		}

		public string Type {
			get {
				return m_Type;
			}
		}

		public string ActionItem {
			get {
				return m_ActionItem;
			}
		}

		public string Content {
			get {
				return m_Content;
			}
		}

		public Uri ContentUrl {
			get {
				return m_ContentUrl;
			}
		}
	}
}