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

using Mono.Addins;

using Synapse.ServiceStack;
using Synapse.Core;
using Synapse.Xmpp;

using jabber;

namespace Synapse.Xmpp.Services
{
	public class AccountService : IRequiredService, IInitializeService, IDisposable
	{
		List<Account> m_Accounts = new List<Account>();
		string        m_FileName = Path.Combine(Paths.ApplicationData, "accounts.xml");
		
		public event AccountEventHandler AccountAdded;
		public event AccountEventHandler AccountRemoved;
		
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
		}

		public void Dispose ()
		{
			foreach (Account account in m_Accounts)
				account.Disconnect();
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
					AccountAdded(account);
				}

				if (account.AutoConnect)
					account.Connect();

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
					
					SaveAccounts();
				} else {
					throw new Exception("Account not found");
				}
			}
		}
		
		public IList<Account> Accounts {
			get {
				return m_Accounts.AsReadOnly();
			}
		}

		public IList<Account> ConnectedAccounts {
			get {
				return m_Accounts.FindAll(a => 
              		a.ConnectionState == AccountConnectionState.Connected
              	).AsReadOnly();
			}
		}

		public Account GetAccount(JID jid)
		{
			return m_Accounts.Find(a => a.Jid == jid);
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

		public bool AutoConnect {
			get; set;
		}
	}
}