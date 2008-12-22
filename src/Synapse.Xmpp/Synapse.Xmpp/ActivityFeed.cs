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
	public delegate void ActivityFeedItemEventHandler (Account account, IActivityFeedItem item);
	
	public class ActivityFeed
	{	
		Account                  m_Account;	
		Queue<IActivityFeedItem> m_Queue = new Queue<IActivityFeedItem>();
		
		public event ActivityFeedItemEventHandler NewItem;
		
		public ActivityFeed(Account account)
		{
			m_Account = account;
			
			PostItem(new ActivityFeedItem(account, null, null, "Welcome to Synapse!", null, null));
		}
		
		public void PostItem (IActivityFeedItem item)
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
	
	public interface IActivityFeedItem
	{
		string ToHtml();
		DateTime Timestamp {
			get;
		}
	}
	
	// XXX: Rename this to "GenericActivityFeedItem" or something.
	public class ActivityFeedItem : IActivityFeedItem
	{
		Account  m_Account;
		JID 	 m_From;
		DateTime m_Timestamp;
		string   m_Type;
		string   m_Action;
		string   m_ActionItem;
		string   m_Content;
		
		public ActivityFeedItem (Account account, JID from, string type, string action, string actionItem, string content)
		{
			if (action == null)
				throw new ArgumentNullException("action");
			
			m_Timestamp  = DateTime.Now;
			m_Account    = account;
			m_From       = from;
			m_Type       = type;
			m_Action     = Util.EscapeHtml(action);
			m_ActionItem = Util.EscapeHtml(actionItem); 
			m_Content    = Util.EscapeHtml(content);
		}
		
		public DateTime Timestamp {
			get {
				return m_Timestamp;
			}
		}
		
		public string ToHtml()
		{
			StringBuilder htmlBuilder = new StringBuilder();

			string avatarHash = null;
			if (m_From != null)
				avatarHash = AvatarManager.GetAvatarHash(m_From);
			else
				avatarHash = "octy";
			
			htmlBuilder.AppendFormat("<div class=\"item\" style=\"background-image: url('avatar:/{0}') !important\">", avatarHash);
			
			htmlBuilder.Append(String.Format("<div class='timestamp'>{0}</div>", m_Timestamp.ToShortTimeString()));
			
			if (m_From != null) {
				string name = null;
				Item item = m_Account.Roster[m_From.Bare];
				if (item != null && !String.IsNullOrEmpty(item.Nickname))
					name = item.Nickname;
				else
					name = m_From.User;

				name = Util.EscapeHtml(name);

				var uri = String.Format("xmpp:{0}?message", m_From.ToString());
				htmlBuilder.Append(String.Format("<a href='{0}' title='{1}' class='jid'>{2}</a> ", uri, m_From.ToString(), name));
			}

			if (m_ActionItem != null) {
				htmlBuilder.Append(String.Format(m_Action, String.Format("<strong>{0}</strong>", m_ActionItem)));
			} else {
				htmlBuilder.Append(String.Format("<strong>{0}</strong>", m_Action));
			}
			
			if (m_Content != null) {
				htmlBuilder.Append(":");
				htmlBuilder.Append(String.Format("<blockquote>{0}</blockquote>", m_Content));
			} else if (m_From != null) {
				htmlBuilder.Append(".");
			}
			
			htmlBuilder.Append("</div>");
						
			return htmlBuilder.ToString();
		}
	}
	
	public class FriendRequestActivityFeedItem : IActivityFeedItem
	{
		Account    m_Account;
		DateTime   m_Timestamp;
		Item m_Item;
		string     m_Message;
		
		public FriendRequestActivityFeedItem (Account account, Item item, string message)
		{
			m_Account   = account;
			m_Item      = item;
			m_Message   = message;
			m_Timestamp = DateTime.Now;
		}
		
		public DateTime Timestamp {
			get {
				return m_Timestamp;
			}
		}
		
		public string ToHtml()
		{
			StringBuilder htmlBuilder = new StringBuilder();
			htmlBuilder.Append("<div class=\"item\">");
			htmlBuilder.Append(String.Format("<div class='timestamp'>{0}</div>", m_Timestamp.ToShortTimeString()));
			htmlBuilder.Append(String.Format("Friend Request from <strong>{0}</strong> ({1})", m_Item.JID.ToString(), m_Item.Name));
			htmlBuilder.Append(String.Format("<blockquote>{0}</blockquote>", m_Message));
	  		htmlBuilder.Append(String.Format("<div id='actions'><a href='dragon://accept'>Accept</a> or <a href='dragon://deny'>Deny</a></div>"));
      		htmlBuilder.Append("</div>");
			return htmlBuilder.ToString();
		}
	}
}