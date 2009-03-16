//
// TwitterAccountHandler.cs
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
using System.Collections.Generic;
using System.Linq;

using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;

using Twitter;

namespace Synapse.Addins.Twitter
{
	public class TwitterAccountHandler : IShoutHandler, IDisposable
	{
		TwitterClient m_Twitter;
		Timer         m_Timer;
		
		bool m_IsDisposed = false;
		
		List<Account> m_Accounts = new List<Account>();
			
		public TwitterAccountHandler (string username, string password)
		{
			m_Twitter = new TwitterClient(username, password);
			m_Twitter.Source = "synapse";

			// Only show messages within the last 15 minutes.
			m_Twitter.FriendsTimelineLastCheckedAt = DateTime.Now.ToUniversalTime() - new TimeSpan(0, 15, 0);
			m_Twitter.RepliesLastCheckedAt         = DateTime.Now.ToUniversalTime() - new TimeSpan(0, 15, 0);
			m_Twitter.DirectMessagesLastChecked    = DateTime.Now.ToUniversalTime() - new TimeSpan(0, 15, 0);

			m_Timer = new Timer(240000);
			m_Timer.Elapsed += HandleElapsed;
			
			System.Threading.ThreadPool.QueueUserWorkItem(delegate {
				HandleElapsed(null, null);
				m_Timer.Start();
			});
		}
		
		public string Name {
			get {
				return String.Format("Twitter ({0})", m_Twitter.Username);
			}
		}
		
		public string Username {
			get {
				return m_Twitter.Username;
			}
		}
		
		public string Password {
			get {
				return m_Twitter.Password;
			}
			set {
				m_Twitter.Password = value;
			}
		}
		
		public void Shout (string message)
		{
			System.Threading.ThreadPool.QueueUserWorkItem(delegate {
				try {
					var service = ServiceManager.Get<TwitterService>();
					var status = m_Twitter.Update(message);
					service.AddStatus(status);
				} catch (Exception ex) {
					// FIXME: Show this in the feed.
					Console.WriteLine("FAILED TO UPDATE TWITTER STATUS: " + ex);
				}
			});
		}
			
		public List<Account> Accounts {
			get {
				return m_Accounts;
			}
		}
			
		public void Dispose ()
		{
			lock (m_Timer) {
				m_Timer.Stop();
				m_IsDisposed = true;
			}
		}
		
		void HandleElapsed(object sender, ElapsedEventArgs e)
		{
			try {
				lock (m_Timer) {
					if (m_IsDisposed)
						return;
				}
				
				var service = ServiceManager.Get<TwitterService>();
				var statuses = m_Twitter.FriendsAndRepliesAndMessages(true).OrderBy(s => s.CreatedAtDT);
				foreach (var status in statuses) {
					service.AddStatus(status);
				}
				
			} catch (Exception ex) {
				// FIXME: Show in feed.
				Console.Error.WriteLine("Twitter API Error: " + ex);
			}
		}
	}
}
