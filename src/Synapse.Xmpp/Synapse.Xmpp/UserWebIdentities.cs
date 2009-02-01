//
// UserWebIdentities.cs
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
using System.Collections.Generic;
using System.Xml;
using jabber;
using jabber.protocol;
using jabber.protocol.iq;

namespace Synapse.Xmpp
{	
	public class UserWebIdentities : IDiscoverable
	{
		public event EventHandler MyIdentitiesReceived;
		
		Account m_Account;
		Dictionary<string, string> m_MyIdentities;
		Dictionary<JID, WebIdentities> m_IdentityCache;
		
		public UserWebIdentities(Account account)
		{
			m_Account = account;
			m_MyIdentities = new Dictionary<string, string>();
			m_IdentityCache = new Dictionary<JID, WebIdentities>();
			
			account.AddStreamType("web-identities", Namespace.WebIdentities, typeof(WebIdentities));
			account.GetFeature<PersonalEventing>().RegisterHandler(Namespace.WebIdentities, ReceivedWebIdentities);
		}

		public void SetIdentity (string name, string value)
		{
			m_MyIdentities[name] = value;
			UpdateServer();
		}

		public string GetIdentity (string name)
		{
			lock (m_MyIdentities) {
				if (m_MyIdentities.ContainsKey(name))
					return m_MyIdentities[name];
				else
					return null;
			}
		}

		public string GetIdentity (JID jid, string name)
		{
			if (m_IdentityCache.ContainsKey(jid))
				return m_IdentityCache[jid].GetIdentity(name);
			else
				return null;
		}
		
		public string[] FeatureNames {
			get {
				return new [] { Namespace.WebIdentities, Namespace.WebIdentities + "+notify" };
			}
		}

		void ReceivedWebIdentities (JID from, string node, PubSubItem item)
		{
			WebIdentities identities = (WebIdentities)item["web-identities"];
			
			if (from == m_Account.Jid.BareJID) {
				lock (m_MyIdentities) {
					m_MyIdentities.Clear();
					foreach (XmlElement child in identities.ChildNodes) {
						m_MyIdentities.Add(child.Name, child.InnerText);
					}
					if (MyIdentitiesReceived != null)
						MyIdentitiesReceived(this, EventArgs.Empty);
				}
			} else {
				m_IdentityCache[from] = identities;
			}
		}

		void UpdateServer ()
		{			
			WebIdentities identities = new WebIdentities(m_Account.Client.Document);
			lock (m_MyIdentities) {
				foreach (var kv in m_MyIdentities) {
					identities.SetIdentity(kv.Key, kv.Value);
				}
			}

			PubSubItem itemElement = new PubSubItem(m_Account.Client.Document);
			itemElement.SetAttribute("id", "current");
			itemElement.AppendChild(identities);
			
			m_Account.GetFeature<PersonalEventing>().Publish(Namespace.WebIdentities, itemElement);
		}
		
		class WebIdentities : Element
		{
			public WebIdentities(XmlDocument doc) : base ("web-identities", Namespace.WebIdentities, doc)
			{
				
			}
			
			public WebIdentities (string prefix, XmlQualifiedName qname, XmlDocument doc) :
					base (qname.Name, doc)
			{
			}

			public string GetIdentity (string name)
			{
				return base.GetElem(name);
			}

			public void SetIdentity (string name, string value)
			{
				base.SetElem(name, value);
			}
		}
	}
}
