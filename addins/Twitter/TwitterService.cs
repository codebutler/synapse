//
// TwitterService.cs
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
using System.Timers;
using System.Linq;
using System.Web;
using System.Net;
using System.Collections.Generic;
using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;
using Synapse.Services;
using Twitter;

namespace Synapse.Addins.Twitter
{	
	public class TwitterService  : IExtensionService, IDelayedInitializeService
	{
		Dictionary<string, TwitterAccountHandler> m_TwitterAccounts = new Dictionary<string, TwitterAccountHandler>();
		List<int> m_SeenIds = new List<int>();

		public void Initialize ()
		{
			
		}
		
		public void DelayedInitialize ()
		{
			#region Feed Templates
			var replyAction = new NotificationAction() {
				Name = "reply",
				Label = "Reply",
				Callback = delegate (IActivityFeedItem item, NotificationAction action) {
					var twitterItem = (TwitterActivityFeedItem)item;
					// FIXME:
					Console.WriteLine("Reply");
				}
			};
			
			var retweetAction = new NotificationAction() {
				Name = "retweet",
				Label = "Retweet",
				Callback = delegate (IActivityFeedItem item, NotificationAction action) {
					var twitterItem = (TwitterActivityFeedItem)item;
					// FIXME:
					Console.WriteLine("Retweet");
				}
			};

			var feedService = ServiceManager.Get<ActivityFeedService>();
			feedService.AddTemplate("tweet", "Twitter", "tweets", "tweet", new Dictionary<string, object> {
				{ "IconUrl", "resource:/twitter/twitm-16.png" }
			}, replyAction, retweetAction);

			feedService.AddTemplate("direct-tweet", "Twitter", "direct tweets", "direct tweet", new Dictionary<string, object> {
				{ "IconUrl", "resource:/twitter/twitm-16.png" }
			}, replyAction);

			#endregion

			var accountService = ServiceManager.Get<AccountService>();
			accountService.AccountAdded   += HandleAccountAdded;
			accountService.AccountRemoved += HandleAccountRemoved;
			foreach (Account account in accountService.Accounts) {
				HandleAccountAdded(account);
			}
		}

		public void AddStatus (AbstractTwitterItem status)
		{
			if (!m_SeenIds.Contains(status.ID)) {
				m_SeenIds.Add(status.ID);
				
				var item = new TwitterActivityFeedItem(status);
				
				var activityFeed = ServiceManager.Get<ActivityFeedService>();
				activityFeed.PostItem(item);
			}
		}
		
		public void AccountConfigUpdated (Account account, string oldUsername, string newUsername, string newPassword)
		{
			lock (m_TwitterAccounts) {
				if (oldUsername != newUsername) {
					RemoveTwitterAccount(account, oldUsername);
				}
				if (!String.IsNullOrEmpty(newUsername) && !String.IsNullOrEmpty(newPassword)) {
					AddTwitterAccount(account, newUsername, newPassword);
				}
			}
		}
				                                  
		public string ServiceName {
			get {
				return "TwitterService";
			}
		}

		public void Dispose ()
		{
			lock (m_TwitterAccounts) {
				foreach (var pair in m_TwitterAccounts) {
					var handler = pair.Value;
					handler.Dispose();
				}
				m_TwitterAccounts.Clear();
			}
		}
		
		void HandleAccountAdded (Account account)
		{
			if (!String.IsNullOrEmpty(account.GetProperty("Twitter.Username")) &&
			    !String.IsNullOrEmpty(account.GetProperty("Twitter.Password")))
			{
				string username = account.GetProperty("Twitter.Username");
				string password = account.GetProperty("Twitter.Password");
				account.GetFeature<UserWebIdentities>().SetIdentity("twitter", username);
				
				AddTwitterAccount(account, username, password);	
			}
		}
		
		void HandleAccountRemoved (Account account)
		{
			string username = account.GetProperty("Twitter.Username");
			if (!String.IsNullOrEmpty(username)) {
				RemoveTwitterAccount(account, username);
			}
		}
		
		void AddTwitterAccount (Account account, string username, string password)
		{
			lock (m_TwitterAccounts) {
				if (!m_TwitterAccounts.ContainsKey(username)) {
					var handler = new TwitterAccountHandler(username, password);
					m_TwitterAccounts.Add(username, handler);
					
					var shoutService = ServiceManager.Get<ShoutService>();
					shoutService.AddHandler(handler);
					
					handler.Accounts.Add(account);
					
					Console.WriteLine("Added twitter account: " + username);
				} else if (m_TwitterAccounts[username].Accounts.Contains(account)) {
					m_TwitterAccounts[username].Password = password;
					m_TwitterAccounts[username].Accounts.Add(account);
				}
			}
		}
		
		void RemoveTwitterAccount (Account account, string username)
		{
			lock (m_TwitterAccounts) {
				if (m_TwitterAccounts.ContainsKey(username)) {
					var handler = m_TwitterAccounts[username];
					if (handler.Accounts.Contains(account)) {
						handler.Accounts.Remove(account);
					}
					
					if (handler.Accounts.Count == 0) {
						handler.Dispose();
						m_TwitterAccounts.Remove(username);
						
						var shoutService = ServiceManager.Get<ShoutService>();
						shoutService.RemoveHandler(handler);
					}
				}
			}
		}
	}

	public class TwitterActivityFeedItem : AbstractActivityFeedItem
	{
		AbstractTwitterItem m_Item;
		
		public TwitterActivityFeedItem (AbstractTwitterItem item)
		{
			m_Item = item;
		}
		
		public override string FromName {
			get {
				return (m_Item is Status) ? ((Status)m_Item).User.ScreenName : ((DirectMessage)m_Item).SenderScreenName;
			}
		}
		
		public override string FromUrl {
			get {
				return String.Format("http://twitter.com/{0}", HttpUtility.UrlEncode(FromName));
			}
		}
		
		public override string AvatarUrl {
			get {
				return (m_Item is Status) ? ((Status)m_Item).User.ProfileImageUrl : ((DirectMessage)m_Item).Sender.ProfileImageUrl;
			}
		}
		
		public override string Type {
			get {
				return (m_Item is Status) ? "tweet" : "direct-tweet";
			}
		}
		
		public override string ActionItem {
			get {
				return null;
			}
		}
		
		public override string Content {
			get {
				return m_Item.Text;
			}
		}
		
		public override Uri ContentUrl {
			get {
				if (m_Item is Status)
					return new Uri(String.Format("http://twitter.com/{0}/status/{1}", FromName, m_Item.ID));
				else
					return new Uri("http://twitter.com/direct_messages");
			}
		}		
	}
}
