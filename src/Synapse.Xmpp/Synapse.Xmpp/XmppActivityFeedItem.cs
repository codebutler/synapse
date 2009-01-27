//
// XmppActivityFeedItem.cs
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
using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Services;
using Synapse.Xmpp.Services;
using jabber;

namespace Synapse.Xmpp
{
	public class XmppActivityFeedItem : AbstractActivityFeedItem
	{
		Account  m_Account;
		JID 	 m_From;
		string   m_Type;
		string   m_ActionItem;
		string   m_Content;
		Uri      m_ContentUrl;

		public XmppActivityFeedItem (Account account, JID from, string type, string actionItem, string content)
			: this (account, from, type, actionItem, content, null)
		{
		}

		public XmppActivityFeedItem (Account account, JID from, string type, string actionItem, string content, string contentUrl)
			: this (account, from, type, actionItem, content, contentUrl, null)
		{
		}
		
		public XmppActivityFeedItem (Account account, JID from, string type, string actionItem, string content, string contentUrl, object data)
			: base (data)
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
		
		public override string FromUrl {
			get {
				return null;
			}
		}

		public string FromJid {
			get {
				return (m_From == null) ? null : m_From.ToString();
			}
		}

		public override string FromName {
			get {
				return (m_From == null) ? null : m_Account.GetDisplayName(m_From);
			}
		}

		public override string AvatarUrl {
			get {
				return "avatar:/" + AvatarManager.GetAvatarHash(m_From);
			}
		}

		public override string Type {
			get {
				return m_Type;
			}
		}

		public override string ActionItem {
			get {
				return m_ActionItem;
			}
		}

		public override string Content {
			get {
				return m_Content;
			}
		}

		public override Uri ContentUrl {
			get {
				return m_ContentUrl;
			}
		}
	}

	public static class ActivityFeedServiceXmppExtensions
	{		
		public static void PostItem (this ActivityFeedService self, Account account, JID from, string type, string actionItem, string content)
		{
			PostItem(self, account, from, type, actionItem, content, null);
		}
		
		public static void PostItem (this ActivityFeedService self, Account account, JID from, string type, string actionItem, string content, object data)
		{
			PostItem(self, account, from, type, actionItem, content, null, data);
		}

		public static void PostItem (this ActivityFeedService self, Account account, JID from, string type, string actionItem, string content, string contentUrl)
		{
			PostItem(self, account, from, type, actionItem, content, contentUrl, null);
		}
		
		public static void PostItem (this ActivityFeedService self, Account account, JID from, string type, string actionItem, string content, string contentUrl, object data)
		{
			self.PostItem(new XmppActivityFeedItem(account, from, type, actionItem, content, contentUrl, data));
		}

	}

}
