//
// FireEagleService.cs
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

using jabber.connection;
using jabber.protocol;
using jabber.protocol.client;
using jabber.protocol.iq;

using OAuth;
using OAuth.RequestProxies;

namespace Synapse.Addins.FireEagle
{
	public delegate void AuthorizationNeededEventHandler (string authorizationUrl);
	
	public class FireEagleService : IExtensionService, IInitializeService
	{		
		// If you're re-using this code, don't use these keys.
		// Request new ones from https://fireeagle.yahoo.net/
		static readonly string CONSUMER_KEY    = "9SyK6gfZvvlM";
		static readonly string CONSUMER_SECRET = "c6t4UAaiQaclDpYzDP9okbFdObODoMYT";	

		static readonly string USER_LOCATION_URL = "https://fireeagle.yahooapis.com/api/0.1/user";
		
		Consumer     m_Consumer;
		RequestToken m_RequestToken;
		AccessToken  m_AccessToken;
		
		public event AuthorizationNeededEventHandler AuthorizationNeeded;
		public event EventHandler ReceivedAccessToken;
		public event EventHandler AccessTokenCleared;
		public event EventHandler LocationUpdated;
		
		public void Initialize ()
		{
			m_Consumer = new Consumer(CONSUMER_KEY, CONSUMER_SECRET) {
				Site         = "https://fireeagle.yahooapis.com",
				AuthorizeUrl = "https://fireeagle.yahoo.net/oauth/authorize"
			};
			
			var settingsService = ServiceManager.Get<SettingsService>();
			
			string accessToken  = settingsService.Get<string>("FireEagle.AccessToken");
			string accessSecret = settingsService.Get<string>("FireEagle.AccessTokenSecret");
			
			if (!String.IsNullOrEmpty(accessToken) && !String.IsNullOrEmpty(accessSecret)) {
				m_AccessToken = new AccessToken(m_Consumer, accessToken, accessSecret);
			}
			
			var accountService = ServiceManager.Get<AccountService>();
			foreach (Account account in accountService.Accounts) {
				HandleAccountAdded(account);
			}			
			accountService.AccountAdded += HandleAccountAdded;
			accountService.AccountRemoved += HandleAccountRemoved;
		}
		
		public bool IsReady {
			get {
				return m_AccessToken != null;
			}
		}
		
		public void GetRequestToken ()
		{
			m_RequestToken = null;
			
			ClearAccessToken();
			
			// FIXME: Make this async.
			m_RequestToken = m_Consumer.GetRequestToken();
			
			AuthorizationNeeded(m_RequestToken.AuthorizeUrl);
		}
		
		public void RequestAccessToken ()
		{
			if (m_RequestToken == null)
				throw new InvalidOperationException("Call GetRequestToken() first!");
			
			// FIXME: Make this async too.
			m_AccessToken = m_RequestToken.ConvertToAccessToken();
			
			m_RequestToken = null;
			
			var settingsService = ServiceManager.Get<SettingsService>();
			settingsService.Set("FireEagle.AccessToken", m_AccessToken.Token);
			settingsService.Set("FireEagle.AccessTokenSecret", m_AccessToken.Secret);
			
			// Add handler for new old node.
			var accountService = ServiceManager.Get<AccountService>();
			foreach (Account account in accountService.Accounts) {
				string nodeName = String.Format("/api/0.1/user/{0}", m_AccessToken.Token);
				account.PubSubManager.AddNodeHandler(nodeName, ReceivedItem, null, 0);
			}
			
			ReceivedAccessToken(this, EventArgs.Empty);
		}
		
		public void ClearAccessToken ()
		{
			// Remove handler for old node.
			var accountService = ServiceManager.Get<AccountService>();
			foreach (Account account in accountService.Accounts) {
				string nodeName = String.Format("/api/0.1/user/{0}", m_AccessToken.Token);
				account.PubSubManager.RemoveNodeHandler(nodeName, ReceivedItem);
			}
			
			m_AccessToken = null;
			
			AccessTokenCleared(this, EventArgs.Empty);
		}
		                              
		public string GetLocation ()
		{
			return m_Consumer.Request("GET", new Uri(USER_LOCATION_URL), m_AccessToken);
		}
		
		public void Subscribe ()
		{
			string node = String.Format("/api/0.1/user/{0}", m_AccessToken.Token);
			foreach (Account account in ServiceManager.Get<AccountService>().Accounts) {
				// FIXME: Queue if not connected...
				if (account.ConnectionState == AccountConnectionState.Connected) {
					
					var iq = new PubSubIQ(account.Client.Document, PubSubCommandType.subscribe, node);
					iq.Type = jabber.protocol.client.IQType.set;					
					((Subscribe)iq.Command).JID = account.Jid.BareJID;
					iq.To = new jabber.JID("fireeagle.com");
					iq.From = account.Jid;
				
					var oauth = new Synapse.Xmpp.OAuth(m_Consumer, m_AccessToken, account.Client.Document);
					iq["pubsub"].AppendChild(oauth);
					
					var proxy = RequestProxyFactory.CreateProxy(iq);
					proxy.Sign(m_Consumer.Secret, m_AccessToken.Secret);
					
					account.IQTracker.BeginIQ(iq, delegate (object sender, IQ result, object state) {
						Console.WriteLine("OAUTH SUBSCRIBE RESPONSE: " + result.OuterXml);
						// FIXME: Need to check if subscribe was successfull.
					}, null);
				}
			}
		}
		
		public void Unsubscribe ()
		{
			throw new NotImplementedException();
		}
		
		public void Dispose ()
		{
			
		}
	
		public string ServiceName {
			get {
				return "FireEagleService";
			}
		}
		
		void HandleAccountAdded (Account account)
		{					
			account.ConnectionStateChanged += HandleAccountConnectionStateChanged;
			
			if (IsReady) {				
				string nodeName = String.Format("/api/0.1/user/{0}", m_AccessToken.Token);
				account.PubSubManager.AddNodeHandler(nodeName, ReceivedItem, null, 0);
			}
		}
		
		void HandleAccountRemoved (Account account)
		{
			account.ConnectionStateChanged -= HandleAccountConnectionStateChanged;
			
			if (IsReady) {
				string nodeName = String.Format("/api/0.1/user/{0}", m_AccessToken.Token);
				account.PubSubManager.RemoveNodeHandler(nodeName, ReceivedItem);
			}
		}
		
		void HandleAccountConnectionStateChanged (Account account)
		{
			if (account.ConnectionState == AccountConnectionState.Connected) {
				if (IsReady) {
					// FIXME: Check subscriptions first.
					Subscribe();
				}
			}
		}
		
		void ReceivedItem (PubSubNode node, PubSubItem item)
		{
			Console.WriteLine("************");
			Console.WriteLine("RECEIVED FIREEAGLE PUBSUB !!!!");
			Console.WriteLine(node.Node);
			Console.WriteLine(item.OuterXml);
		}
	}
}
