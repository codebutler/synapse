//
// PersonalEventing.cs: Implements XEP-0163: Personal Eventing via Pubsub
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
using System.Collections.Generic;
using System.Linq;
using jabber;
using jabber.protocol;
using jabber.protocol.iq;
using jabber.protocol.client;
using jabber.connection;
using System.Xml;

namespace Synapse.Xmpp
{
	public delegate void PubsubHandler (JID from, string node, PubSubItem item);
	
	public class PersonalEventing : IDiscoverable
	{
		Account m_Account;
		Dictionary<string, PubsubHandler> m_Handlers = new Dictionary<string, PubsubHandler>();

		Queue<IQ> m_Queue = new Queue<IQ>();
		
		public PersonalEventing(Account account)
		{
			m_Account = account;
			account.ConnectionStateChanged +=HandleConnectionStateChanged; 
		}

		void HandleConnectionStateChanged(Account account)
		{
			if (account.ConnectionState == AccountConnectionState.Connected) {
				lock (m_Queue) {
					if (m_Queue.Count > 0) {
						while (m_Queue.Count > 0) {
							account.Send(m_Queue.Dequeue());
						}
					}
				}
			} else if (account.ConnectionState == AccountConnectionState.Disconnected) {
				m_Queue.Clear();
			}
		}
		
		public void RegisterHandler (string node, PubsubHandler handler)
		{
			// FIXME: What's the purpose of this maxItems arg?
			m_Account.PubSubManager.AddNodeHandler(node, delegate (PubSubNode n, PubSubItem item) {
				handler(n.Jid, n.Node, item);
			}, null, 1000);
		}
		
		public void Publish (string node, XmlElement item)
		{
			IQ iq = new IQ(m_Account.Client.Document);
			iq.Type = IQType.set;
			PubSub pubsub = new PubSub(m_Account.Client.Document);
			pubsub.SetAttribute("xmlns", "http://jabber.org/protocol/pubsub");
			Publish publish = new Publish(m_Account.Client.Document);
			publish.SetAttribute("node", node);
			publish.AddChild(item);
			pubsub.AddChild(publish);
			iq.AddChild(pubsub);

			if (m_Account.ConnectionState == AccountConnectionState.Connected) {
				m_Account.Send(iq);
			} else {
				lock (m_Queue) {
					m_Queue.Enqueue(iq);
				}
			}
		}
		
		public string[] FeatureNames {
			get {
				return new string[0];
			}
		}
	}
}
