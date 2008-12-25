//
// Account.cs: A single XMPP account.
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using jabber;
using jabber.client;
using jabber.connection;
using jabber.protocol.client;
using jabber.protocol;
using jabber.protocol.iq;
using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Services;

namespace Synapse.Xmpp
{
	public delegate void AccountEventHandler (Account account);
	public delegate void MessageEventHandler (Account account, Packet packet);
	public delegate void AccountErrorEventHandler (Account account, Exception ex);
	
	public class Account
	{
		string m_User;
		string m_Domain;
		string m_Resource;
		string m_Password;
		string m_ConnectServer;

		bool m_NetworkDisconnected = false;

		DateTime m_ConnectedAt;
			
		AccountConnectionState m_State;
		ClientStatus           m_Status;
		ClientStatus           m_PendingStatus;
		
		JabberClient      m_Client;
		CapsManager       m_CapsManager;
		RosterManager     m_Roster;
		PubSubManager     m_PubSubManager;
		DiscoManager      m_DiscoManager;
		ConferenceManager m_ConferenceManager;
		BookmarkManager   m_BookmarkManager;
		PresenceManager   m_PresenceManager;

		AvatarManager     m_AvatarManager;
		
		Dictionary<Type, IDiscoverable> m_Features   = new Dictionary<Type, IDiscoverable>();
		PropertyCollection              m_Properties = new PropertyCollection();

		Dictionary<JID, Presence> m_UserPresenceCache;
		
		ActivityFeed m_ActivityFeed;
		
		public event AccountEventHandler Changed; // XXX: is this used?
		public event AccountEventHandler ConnectionStateChanged;
		public event AccountEventHandler StatusChanged;

		public event AccountErrorEventHandler Error;
		
		public Account (string user, string domain, string resource) : this (user, domain, resource, null)
		{
		}
		
		public Account (string user, string domain, string resource, string connectServer) : this ()
		{
			if (String.IsNullOrEmpty(user)) throw new ArgumentNullException("user");
			if (String.IsNullOrEmpty(domain)) throw new ArgumentNullException("domain");
			if (String.IsNullOrEmpty(resource)) throw new ArgumentNullException("resource");
			
			m_User     = user;
			m_Domain   = domain;
			m_Resource = resource;
			m_ConnectServer = connectServer;

			m_UserPresenceCache = new Dictionary<JID, Presence>();
		}

		public static Account FromAccountInfo(AccountInfo info)
		{
			Account account = new Account(info.User, info.Domain, info.Resource, info.ConnectServer);
			account.Password = info.Password;
			return account;
		}
		
		private Account ()
		{
			m_ActivityFeed = new ActivityFeed(this);
			
			m_Client = new JabberClient();
			m_Client.AutoPresence = false;
			m_Client.AutoRoster = true;
			m_Client.AutoStartTLS = true;
			m_Client.OnConnect      += HandleOnConnect;
			m_Client.OnAuthenticate += HandleOnAuthenticate;
			m_Client.OnDisconnect   += HandleOnDisconnect;
			m_Client.OnError        += HandleOnError;
			m_Client.OnPresence 	+= HandleOnPresence;
			m_Client.OnInvalidCertificate += delegate { return true; }; // XXX:

			m_Client.OnStreamInit += HandleOnStreamInit;
			
			m_Roster = new RosterManager();
			m_Roster.Stream = m_Client;
			
			m_CapsManager = new CapsManager();
			m_CapsManager.Stream = m_Client;
			m_CapsManager.Node = "http://www.synapse.im/";
			
			m_PubSubManager = new PubSubManager();
			m_PubSubManager.Stream = m_Client;
						
			m_DiscoManager = new DiscoManager();
			m_DiscoManager.Stream = m_Client;
			
			m_ConferenceManager = new ConferenceManager();
			m_ConferenceManager.Stream = m_Client;

			m_BookmarkManager = new BookmarkManager();
			m_BookmarkManager.Stream = m_Client;
			m_BookmarkManager.ConferenceManager = m_ConferenceManager;

			m_PresenceManager = new PresenceManager();
			m_PresenceManager.Stream = m_Client;

			m_AvatarManager = new AvatarManager(this);
			
			// XXX: Don't hard-code this.
			m_CapsManager.AddIdentity("Synapse 0.1", "client", "pc", "en_US");
			
			// Create builtin features
			// XXX: This should be an extension point as well.
			AddFeature(new PersonalEventing(this));
			AddFeature(new Microblogging(this));
			AddFeature(new UserMood(this));
			AddFeature(new UserTune(this));
			AddFeature(new UserAvatars(this));
			AddFeature(new ChatStates(this));

			ServiceManager.Get<NetworkService>().StateChange += HandleNetworkStateChanged;
		}

		void HandleOnStreamInit(object sender, ElementStream stream)
		{
			stream.AddType("tune", "http://jabber.org/protocol/tune", typeof(UserTune.Tune));
		}

		void HandleNetworkStateChanged (NetworkState state)
		{
			if ((state == NetworkState.Disconnected || state == NetworkState.Asleep) && ConnectionState != AccountConnectionState.Disconnected) {
				m_NetworkDisconnected = true;
				Disconnect();
			} else if (state == NetworkState.Connected && m_NetworkDisconnected) {
				Connect();
			}
		}

		void HandleOnDisconnect(object sender)
		{
			m_Status = null;
			ConnectionState = AccountConnectionState.Disconnected;
		}

		void HandleOnAuthenticate(object sender)
		{
			ConnectionState = AccountConnectionState.Connected;

			if (m_PendingStatus != null) {
				Status = m_PendingStatus;
			} else {
				Status = new ClientStatus(ClientStatusType.Available, null);
			}

			m_ConnectedAt = DateTime.Now;
		}

		void HandleOnError(object sender, Exception ex)
		{
			Console.Error.WriteLine("An error has occurred with: " + Jid.ToString() + ": " + ex);
			ConnectionState = AccountConnectionState.Disconnected;
			if (Error != null)
				Error(this, ex);
		}

		void HandleOnConnect(object sender, StanzaStream stream)
		{
			 ConnectionState = AccountConnectionState.Authenticating;
		}

		void HandleOnPresence(object sender, Presence pres)
		{
			Presence oldPresence =  m_UserPresenceCache.ContainsKey(pres.From) ? m_UserPresenceCache[pres.From] : null;
			m_UserPresenceCache[pres.From] = pres;
			
			if (pres.To == Jid && pres.From == Jid) {
				m_Status = new ClientStatus(pres.Show, pres.Status);
				if (StatusChanged != null)
					StatusChanged(this);
			}

			// This is my lame way to avoid showing the initial onslaught of presence messages.
			if ((DateTime.Now - m_ConnectedAt).TotalSeconds > 30) {
				if (oldPresence == null || (oldPresence.Type != pres.Type || oldPresence.Show != pres.Show || oldPresence.Status != pres.Status)) {
					if (pres.Type == PresenceType.available || pres.Type == PresenceType.unavailable) {
						m_ActivityFeed.PostItem(new ActivityFeedItem(this, pres.From, "presence", "is now {0}", Helper.GetPresenceDisplay(pres), pres.Status));
					}
				}
			}
		}
		
		// XXX: We probably won't need this method once we replace all this 
		// with an extension point as mentioned above.
		private void AddFeature (IDiscoverable feature)
		{
			m_Features.Add(feature.GetType(), feature);
			foreach (string featureName in feature.FeatureNames)
				m_CapsManager.AddFeature(featureName);
		}

		public DiscoManager DiscoManager {
			get {
				return m_DiscoManager;
			}
		}
		
		public ConferenceManager ConferenceManager {
			get {
				return m_ConferenceManager;
			}
		}

		public BookmarkManager BookmarkManager {
			get {
				return m_BookmarkManager;
			}
		}

		public PresenceManager PresenceManager {
			get {
				return m_PresenceManager;
			}
		}

		public PubSubManager PubSubManager {
			get {
				return m_PubSubManager;
			}
		}
		
		// XXX: Can we do away with this?
		public JabberClient Client {
			get {
				return m_Client;
			}
		}
		
		public string Password {
			get {
				return m_Password;
			}
			set {
				CheckIfReadOnly();
				this.m_Password = value;
				OnChanged();
			}
		}
		
		public string User {
			get {
				return m_User;
			}
			set {
				CheckIfReadOnly();
				m_User = value;
				OnChanged();
			}
		}
		
		public string Domain {
			get {
				return m_Domain;
			}
			set {
				CheckIfReadOnly();
				m_Domain = value;
				OnChanged();
			}
		}
		
		public string Resource {
			get {
				return m_Resource;
			}
			set {
				CheckIfReadOnly();
				m_Resource = value;
				OnChanged();
			}
		}
		
		public string ConnectServer {
			get {
				return m_ConnectServer;
			}
			set {
				CheckIfReadOnly();
				m_ConnectServer = value;
				OnChanged();
			}
		}
		
		public JID Jid {
			get {
				if (string.IsNullOrEmpty(m_User))
					throw new Exception("User is blank");
				
				if (string.IsNullOrEmpty(m_Domain))
					throw new Exception("Domain is blank");
				
				return new JID(m_User, m_Domain, m_Resource);
			}
		}

		public string GetDisplayName(JID jid)
		{
			Item item = this.Roster[jid.Bare];
			if (item != null && !String.IsNullOrEmpty(item.Nickname))
				return item.Nickname;
			else if (!String.IsNullOrEmpty(jid.User))
				return jid.User;
			else
				return jid.ToString();
		}

		public RosterManager Roster {
			get {
				return this.m_Roster;
			}
		}
		
		public ActivityFeed ActivityFeed {
			get {
				return m_ActivityFeed;
			}
		}
		
		public T GetFeature<T>()
		{
			return (T)m_Features[typeof(T)];
		}
		
		public IEnumerable<IDiscoverable> Features {
			get {
				return m_Features.Values;
			}
		}
		
		public bool IsReadOnly {
			get {
				return (m_State != AccountConnectionState.Disconnected);
			}
		}
		
		public AccountConnectionState ConnectionState {
			get {
				return m_State;
			}
			private set {
				m_State = value;
				if (m_State == AccountConnectionState.Disconnected) {
					m_Status = null;
				}
				OnStateChanged();
			}
		}
		
		public ClientStatus Status {
			set {
				m_PendingStatus = null;
				
				if (value.Type != ClientStatusType.Offline && this.ConnectionState != AccountConnectionState.Connected) {
					m_PendingStatus = value;
					Connect();
					return;
				}
				
				switch (value.Type) {
				case ClientStatusType.Available:
					this.Client.Presence(PresenceType.available, value.StatusText, null, 0);
					break;
				case ClientStatusType.FreeToChat:
					this.Client.Presence(PresenceType.available, value.StatusText, "chat", 0);
					break;
				case ClientStatusType.Away:
					this.Client.Presence(PresenceType.available, value.StatusText, "away", 0);
					break;
				case ClientStatusType.ExtendedAway:
					this.Client.Presence(PresenceType.available, value.StatusText, "xa", 0);
					break;
				case ClientStatusType.DoNotDisturb:
					this.Client.Presence(PresenceType.available, value.StatusText, "dnd", 0);
					break;
				case ClientStatusType.Offline:
					this.Disconnect();
					break;
				}
			}
			get {
				return m_Status;
			}
		}
		
		public void Connect()
		{
			if (String.IsNullOrEmpty(m_Password)) {
				throw new Exception("No password");
			}

			if (ConnectionState != AccountConnectionState.Disconnected)
				throw new InvalidOperationException("Already connected");

			m_UserPresenceCache.Clear();
			
			m_NetworkDisconnected = false;
			
			m_Client.User        = m_User;
			m_Client.Server      = m_Domain;
			m_Client.Resource    = m_Resource;
			m_Client.NetworkHost = m_ConnectServer;
			m_Client.Password    = m_Password;
			
			ConnectionState = AccountConnectionState.Connecting;
			
			// FIXME: Calling this in a separate thread so DNS doesn't block the UI.
			// This should be fixed inside jabber-net.
			new Thread(delegate () {
				m_Client.Connect();
			}).Start();
		}
		
		public void Send (Packet message)
		{
			Send(message, null);
		}
		
		public void Send (Packet packet, MessageEventHandler handler)
		{
			if (handler != null) {
				IQ iq = packet as IQ;
				if (iq == null) throw new Exception("BLAH!");
				m_Client.Tracker.BeginIQ(iq, delegate (object sender, IQ reply, object data) {
					handler(this, reply);
				}, null);
			} else {                        
				m_Client.Write(packet);
			}
		}
		
		public void Disconnect()
		{
			m_Client.Close();
		}
		
		public PropertyCollection Properties {
			get {
				return m_Properties;
			}
		}

		public AccountInfo ToAccountInfo ()
		{
			AccountInfo info = new AccountInfo();
			info.User     = m_User;
			info.Domain   = m_Domain;
			info.Resource = m_Resource;
			info.Password = m_Password;
			info.ConnectServer = m_ConnectServer;
			return info;
		}
		
		protected virtual void OnStateChanged ()
		{
			AccountEventHandler handler = ConnectionStateChanged;
			if (handler != null) {
				handler(this);
			}
		}
		
		protected virtual void OnChanged ()
		{
			AccountEventHandler handler = Changed;
			if (handler != null) {
				handler(this);
			}
		}
			
		private void CheckIfReadOnly()
		{
			if (IsReadOnly) {
				throw new InvalidOperationException("Cannot modify connected account");
			}
		}
	}
	
	public enum AccountConnectionState
	{
		Disconnected,
		Connecting,
		Authenticating,
		Connected
	}
}
