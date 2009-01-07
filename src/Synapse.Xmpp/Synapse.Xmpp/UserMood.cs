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
using System.Collections.Generic;
using System.Xml;
using jabber;
using jabber.protocol.iq;
using jabber.protocol;
using Synapse.ServiceStack;

namespace Synapse.Xmpp
{	
	public class UserMood : IDiscoverable
	{
		Account m_Account;
		Dictionary<string, Mood> m_FriendMoods = new Dictionary<string, Mood>();
				
		public UserMood(Account account)
		{
			m_Account = account;
			account.GetFeature<PersonalEventing>().RegisterHandler(
				"http://jabber.org/protocol/mood",
				ReceivedMood
			);
		}

		public void PublishMood (string mood, string reason)
		{			
			XmlDocument doc = m_Account.Client.Document;

			PubSubItem itemElement = new PubSubItem(doc);
			itemElement.SetAttribute("id", "current");
			doc.AppendChild(itemElement);

			Mood moodElement = new Mood(doc, mood);
			moodElement.Text = reason;
			itemElement.AppendChild(moodElement);
			
			m_Account.GetFeature<PersonalEventing>().Publish("http://jabber.org/protocol/mood", itemElement);
		}
		
		void ReceivedMood (JID from, string node, PubSubItem item)
		{
			Mood mood = (Mood)item["mood"];

			m_FriendMoods[from.Bare] = mood;
			
			// Only show in feed if we know this is a recent event.
			if (mood["timestamp"] != null && DateTime.Now.Subtract(DateTime.Parse(mood["timestamp"].InnerText)).TotalSeconds <= 60) {
				Application.Invoke(delegate {
					m_Account.PostActivityFeedItem(from.ToString(), "mood", mood.MoodName, mood.Text);
				});
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

		public class Mood : Element
		{
			public Mood (XmlDocument doc, string moodName) : base ("mood", "http://jabber.org/protocol/mood", doc)
			{
				this.AppendChild(doc.CreateElement(moodName));
					
				// FIXME: Abstract this, I suspect I'll be using it many places.
				XmlElement timestampElement = new Element("timestamp", "http://synapse.im/protocol/timestamp", doc);
				timestampElement.InnerText = DateTime.Now.ToUniversalTime().ToString("o");
				this.AppendChild(timestampElement);
			}

			public Mood (string prefix, XmlQualifiedName qname, XmlDocument doc) :
					base (qname.Name, doc)
			{
			}

			public string MoodName {
				get {
					return base.FirstChild.Name;
				}
			}
				
			public string Text {
				get {
					return GetElem("text");
				}
				set {
					SetElem("text", value);
				}
			}
		}
	}
}
