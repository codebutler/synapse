//
// UserTune.cs
//
// Copyright (C) Eric Butler
//
// Authors:
//   Eric Butler
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
using Synapse.Core.ExtensionMethods;
using Synapse.Services;
using System.Text;
using jabber;
using jabber.protocol.iq;
using Synapse.Core;
using Synapse.ServiceStack;

namespace Synapse.Xmpp
{	
	public class UserTune : IDiscoverable
	{
		Account m_Account;
		
		public UserTune(Account account)
		{
			m_Account = account;
			account.GetFeature<PersonalEventing>().RegisterHandler(
				"http://jabber.org/protocol/tune",
				ReceivedTune
			);
			
			ServiceManager.Get<NowPlayingService>().TrackChanged += TrackChanged;
		}
		
		private void ReceivedTune (JID from, string node, PubSubItem item)
		{
			XmlNode tune = item["tune"];
			if (tune["artist"] != null && tune["title"] != null) {
				string artist = tune["artist"].InnerText;
				string title = tune["title"].InnerText;

				// Only show in feed if we know this is a recent event.
				if (tune["timestamp"] != null && DateTime.Now.Subtract(DateTime.Parse(tune["timestamp"].InnerText)).TotalSeconds <= 60) {
					Application.Invoke(delegate {
						m_Account.ActivityFeed.PostItem(new ActivityFeedItem(m_Account, from, "music", "is now {0}", "listening to", String.Format("{0} - {1}", artist, title)));
					});
				}
			}
		}
		
		private void TrackChanged (object sender, EventArgs args)
		{			
			NowPlayingService nowPlaying = ServiceManager.Get<NowPlayingService>();
			
			XmlDocument doc = m_Account.Client.Document;
			
			XmlElement itemElement = doc.CreateElement("item");
			doc.AppendChild(itemElement);
			
			XmlElement tuneElement = doc.CreateElement("tune");
			tuneElement.SetAttribute("xmlns", "http://jabber.org/protocol/tune");
			itemElement.AppendChild(tuneElement);

			XmlElement timestampElement = doc.CreateElement("timestamp");
			timestampElement.SetAttribute("xmlns", "http://synapse.im/protocol/timestamp");
			timestampElement.InnerText = DateTime.Now.ToUniversalTime().ToString("o");
			tuneElement.AppendChild(timestampElement);
			
			// FIXME:
			if (nowPlaying.IsPlaying) {
				XmlElement artistElement = doc.CreateElement("artist");
				artistElement.InnerText = nowPlaying.CurrentTrackArtist;
				tuneElement.AppendChild(artistElement);
				
				XmlElement lengthElement = doc.CreateElement("length");
				lengthElement.InnerText = nowPlaying.CurrentTrackLength;
				tuneElement.AppendChild(lengthElement);
				
				XmlElement ratingElement = doc.CreateElement("rating");
				ratingElement.InnerText = nowPlaying.CurrentTrackRating;
				tuneElement.AppendChild(ratingElement);
				
				XmlElement sourceElement = doc.CreateElement("source");
				sourceElement.InnerText = nowPlaying.CurrentTrackSource;
				tuneElement.AppendChild(sourceElement);
				
				XmlElement titleElement = doc.CreateElement("title");
				titleElement.InnerText = nowPlaying.CurrentTrackTitle;
				tuneElement.AppendChild(titleElement);
				
				XmlElement trackElement = doc.CreateElement("track");
				trackElement.InnerText = nowPlaying.CurrentTrackNumber;
				tuneElement.AppendChild(trackElement);
				
				XmlElement uriElement = doc.CreateElement("uri");
				uriElement.InnerText = nowPlaying.CurrentTrackUri;
				tuneElement.AppendChild(uriElement);
			}
		              
			m_Account.GetFeature<PersonalEventing>().Publish("http://jabber.org/protocol/tune", itemElement);
		}
		
		public string[] FeatureNames {
			get {
				return new string[] { 
					"http://jabber.org/protocol/tune",
					"http://jabber.org/protocol/tune+notify"
				};
			}
		}
	}
}