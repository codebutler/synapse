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
using Synapse.Xmpp.Services;
using Synapse.Services;
using Twitter;

namespace Synapse.Addins.TwitterAddin
{	
	public class TwitterService  : IExtensionService, IInitializeService
	{
		TwitterClient m_Twitter;
		Timer         m_Timer;
		List<int>     m_SeenIds = new List<int>();
		
		public void Initialize ()
		{
			m_Twitter = new TwitterClient();

			// Only show messages within the last 15 minutes.
			m_Twitter.FriendsTimelineLastCheckedAt = DateTime.Now.ToUniversalTime() - new TimeSpan(0, 15, 0);
			m_Twitter.RepliesLastCheckedAt = DateTime.Now.ToUniversalTime() - new TimeSpan(0, 15, 0);
			m_Twitter.DirectMessagesLastChecked = DateTime.Now.ToUniversalTime() - new TimeSpan(0, 15, 0);

			var replyAction = new NotificationAction() {
				Name = "reply",
				Label = "Reply",
				Callback = delegate (object o, EventArgs args) {
					var feedItem = (TwitterActivityFeedItem)o;
					// FIXME:
					Console.WriteLine("Reply");
				}
			};
			
			var retweetAction = new NotificationAction() {
				Name = "retweet",
				Label = "Retweet",
				Callback = delegate (object o, EventArgs args) {
					var feedItem = (TwitterActivityFeedItem)o;
					// FIXME:
					Console.WriteLine("Retweet");
				}
			};

			var feedService = ServiceManager.Get<ActivityFeedService>();
			feedService.AddTemplate("tweet", "tweets", "tweet", new Dictionary<string, object> {
				{ "IconUrl", "resource:/twitter/twitm-16.png" }
			}, replyAction, retweetAction);

			feedService.AddTemplate("direct-tweet", "direct tweets", "direct tweet", new Dictionary<string, object> {
				{ "IconUrl", "resource:/twitter/twitm-16.png" }
			}, replyAction);

			m_Timer = new Timer(240000);
			m_Timer.Elapsed += HandleElapsed;
				
			var settingsService = ServiceManager.Get<SettingsService>();
			m_Twitter.Username = settingsService.Get<string>("Twitter.Username");
			m_Twitter.Password = settingsService.Get<string>("Twitter.Password");

			Application.Client.Started += delegate {
				StartStop();
			};
		}
		
		public string Username {
			get {
				return m_Twitter.Username;
			}
			set {
				var settingsService = ServiceManager.Get<SettingsService>();
				settingsService.Set("Twitter.Username", value);
				
				m_Twitter.Username = value;
				StartStop();
			}
		}

		public string Password {
			get {
				return m_Twitter.Password;
			}
			set {
				var settingsService = ServiceManager.Get<SettingsService>();
				settingsService.Set("Twitter.Password", value);
				
				m_Twitter.Password = value;
				StartStop();
			}
		}

		public void Update (string status)
		{
			if (!String.IsNullOrEmpty(m_Twitter.Username) && !String.IsNullOrEmpty(m_Twitter.Password)) {
				AddStatus(m_Twitter.Update(status));
			} else {
				throw new Exception("No username/password");
			}
		}

		void HandleElapsed(object sender, ElapsedEventArgs e)
		{
			try {
				var statuses = m_Twitter.FriendsAndRepliesAndMessages(true).OrderBy(s => s.CreatedAtDT);
				foreach (var status in statuses) {
					AddStatus(status);
				}
				
			} catch (Exception ex) {
				Console.Error.WriteLine("Twitter API Error: " + ex);
			}
		}

		void StartStop ()
		{
			if (!String.IsNullOrEmpty(Username) && !String.IsNullOrEmpty(Password)) {
				System.Threading.ThreadPool.QueueUserWorkItem(delegate {
					HandleElapsed(null, null);
					m_Timer.Start();
				});
			} else {
				m_Timer.Stop();
			}
		}

		void AddStatus (AbstractTwitterItem status)
		{
			if (!m_SeenIds.Contains(status.ID)) {
				m_SeenIds.Add(status.ID);
				
				var item = new TwitterActivityFeedItem(status);
				
				var activityFeed = ServiceManager.Get<ActivityFeedService>();
				activityFeed.PostItem(item);
			}
		}
		
		public string ServiceName {
			get {
				return "TwitterService";
			}
		}

		public void Dispose ()
		{
			m_Timer.Stop();
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
