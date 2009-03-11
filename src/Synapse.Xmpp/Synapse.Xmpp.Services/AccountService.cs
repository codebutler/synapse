//
// AccountService.cs:
//
// Authors:
//   Eric Butler <eric@extremeboredom.net>
//
// Copyright (c) 2008 Eric Butler
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
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Mono.Addins;

using Synapse.ServiceStack;
using Synapse.Core;
using Synapse.Core.ExtensionMethods;
using Synapse.Services;
using Synapse.Xmpp;

using jabber;
using jabber.protocol.client;

namespace Synapse.Xmpp.Services
{
	public class AccountService : IRequiredService, IInitializeService, IDelayedInitializeService, IDisposable
	{
		List<Account> m_Accounts = new List<Account>();
		string        m_FileName = Path.Combine(Paths.ApplicationData, "accounts.xml");
		
		public event AccountEventHandler AccountAdded;
		public event AccountEventHandler AccountRemoved;
		public event AccountEventHandler AccountReceivedRoster;
		
		public event AccountEventHandler AccountConnectionStateChanged;
		
		public void Initialize ()
		{
			if (File.Exists(m_FileName)) {
				AccountsConfig config = null;
				try {
					XmlSerializer serializer = new XmlSerializer(typeof(AccountsConfig));
					using (StreamReader reader = new StreamReader(m_FileName)) {
						config = (AccountsConfig)serializer.Deserialize(reader);
					}
				} catch (Exception e) {
					Console.Error.WriteLine(e);
					File.Delete(m_FileName);
				}
				
				if (config != null) {
					foreach (AccountInfo info in config.Accounts) {
						AddAccount(Account.FromAccountInfo(info), false);
					}
				}
			}

			/*
			if (ServiceManager.Contains<ScreensaverService>()) {
				ScreensaverService screensaver = ServiceManager.Get<ScreensaverService>();
				screensaver.ActiveChanged +=HandleActiveChanged;
			}
			*/
		}

		public void DelayedInitialize ()
		{
			lock (m_Accounts) {	
				foreach (Account account in m_Accounts) {
					if (account.AutoConnect)
						account.Connect();
				}
			}
		}
		
		void HandleActiveChanged(bool isActive)
		{
			// FIXME:
			Console.WriteLine("Set Idle: " + !isActive);
			/*
			foreach (Account account in Accounts) {
				if (!isActive) {
					account.SetIdle();
				} else {
					account.SetNotIdle();
				}
			}
			*/
		}

		public void Dispose ()
		{
			lock (m_Accounts) {	
				foreach (Account account in m_Accounts)
					if (account.ConnectionState != AccountConnectionState.Disconnected)
						account.Disconnect();
			}
		}
		
		public void AddAccount (Account account)
		{
			AddAccount(account, true);
		}
		
		public void AddAccount (Account account, bool save)
		{
			lock (m_Accounts) {
				m_Accounts.Add(account);
			
				if (AccountAdded != null) {
					Console.WriteLine("Fire!");
					AccountAdded(account);
				}
				
				account.ConnectionStateChanged += HandleAccountConnectionStateChanged;
				account.ReceivedRoster += HandleAccountReceivedRoster;

				if (save)
					SaveAccounts();
			}
		}
		
		public void RemoveAccount (Account account)
		{
			lock (m_Accounts) {
				if (m_Accounts.Contains(account)) {
					m_Accounts.Remove(account);
					
					if (AccountRemoved != null) {
						AccountRemoved(account);
					}
					
					account.ConnectionStateChanged -= HandleAccountConnectionStateChanged;
					account.ReceivedRoster -= HandleAccountReceivedRoster;

					SaveAccounts();
				} else {
					throw new Exception("Account not found");
				}
			}
		}
		
		public IList<Account> Accounts {
			get {
				lock (m_Accounts) {	
					return m_Accounts.AsReadOnly();
				}
			}
		}

		public IList<Account> ConnectedAccounts {
			get {
				lock (m_Accounts) {	
					return m_Accounts.FindAll(a => 
	              		a.ConnectionState == AccountConnectionState.Connected
              		).AsReadOnly();
				}
			}
		}

		public Account GetAccount(JID jid)
		{
			lock (m_Accounts) {	
				return m_Accounts.Find(a => a.Jid.Equals(jid));
			}
		}
		
		public void SaveAccounts ()
		{
			List<AccountInfo> infos = new List<AccountInfo>();
			foreach (Account account in m_Accounts) {
				infos.Add(account.ToAccountInfo());
			}
			
			AccountsConfig config = new AccountsConfig();
			config.Accounts = infos;
			
			XmlSerializer serializer = new XmlSerializer(typeof(AccountsConfig));
			using (StreamWriter writer = new StreamWriter(m_FileName)) {
				serializer.Serialize(writer, config);
			}
		}
		
		string IService.ServiceName {
			get { return "AccountService"; }
		}
		
		void HandleAccountConnectionStateChanged (Account account)
		{
			var evnt = AccountConnectionStateChanged;
			if (evnt != null)
				evnt(account);
		}
		
		void HandleAccountReceivedRoster (Account account)
		{
			var evnt = AccountReceivedRoster;
			if (evnt != null)
				evnt(account);
		}
	}

	public class AccountsConfig
	{
		public List<AccountInfo> Accounts {
			get;
			set;
		}
	}
	
	public class AccountInfo
	{
		public AccountInfo ()
		{
			ConnectPort = 5222;
			AutoConnect = true;
		}
		
		public string User {
			get; set;
		}

		public string Domain {
			get; set;
		}
		
		public string Password {
			get; set;
		}

		public string Resource {
			get; set;
		}

		public string ConnectServer {
			get; set;
		}

		public int ConnectPort {
			get; set;
		}
		
		public bool AutoConnect {
			get; set;
		}
		
		public SerializableDictionary<string, string> Properties {
			get; set;
		}
	}
}
