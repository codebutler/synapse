//
// UserMood.cs
//
// Copyright (C) Eric Butler
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
using System.Xml;
using jabber;
using jabber.protocol.iq;

namespace Synapse.Xmpp
{	
	public class UserMood : IDiscoverable
	{
		Account m_Account;
		
		public UserMood(Account account)
		{
			m_Account = account;
			account.GetFeature<PersonalEventing>().RegisterHandler(
				"http://jabber.org/protocol/mood",
				ReceivedMood
			);
		}
		
		// XXX: This method is crap
		private void ReceivedMood (JID from, string node, PubSubItem item)
		{
			XmlNode moodItem = item["mood"];

			string mood = null;
			foreach (XmlNode childNode in moodItem.ChildNodes) {
				if (childNode.NodeType == XmlNodeType.Element && childNode.Name != "text") {
					mood = childNode.Name;
					break;
				}
			}
			
			if (mood != null) {
				string reason = (moodItem["text"].Value == null) ? null : moodItem["text"].Value;
				m_Account.ActivityFeed.PostItem(from.ToString(), "mood", mood, reason);
			}
		}

		public string[] FeatureNames {
			get {
				return new string[] { 
					"http://jabber.org/protocol/mood",
					"http://jabber.org/protocol/mood+notify"
				};
			}
		}
	}
}
