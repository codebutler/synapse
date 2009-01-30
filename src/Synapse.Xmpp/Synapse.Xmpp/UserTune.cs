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
using System.Collections.Generic;
using Synapse.Core.ExtensionMethods;
using Synapse.Services;
using System.Text;
using jabber;
using jabber.protocol;
using jabber.protocol.iq;
using Synapse.Core;
using Synapse.ServiceStack;

namespace Synapse.Xmpp
{	
	public class UserTune : IDiscoverable
	{
		Account m_Account;

		// FIXME: I'd much rather extend the roster Item class and store this there.
		Dictionary<string, Tune> m_FriendTunes = new Dictionary<string, Tune>();
		
		public UserTune(Account account)
		{
			m_Account = account;
			account.GetFeature<PersonalEventing>().RegisterHandler(Namespace.Tune, ReceivedTune);
			
			ServiceManager.Get<NowPlayingService>().TrackChanged += TrackChanged;

			account.AddStreamType("tune", Namespace.Tune, typeof(UserTune.Tune));
		}

		public Tune this [string bareJid] {
			get {
				if (m_FriendTunes.ContainsKey(bareJid))
					return m_FriendTunes[bareJid];
				else
					return null;
			}
		}
		
		private void ReceivedTune (JID from, string node, PubSubItem item)
		{
			Tune tune = (Tune)item["tune"];
			m_FriendTunes[from.Bare] = tune;
			if (!String.IsNullOrEmpty(tune.Artist) && !String.IsNullOrEmpty(tune.Title)) {
				// Only show in feed if we know this is a recent event.
				if (tune["timestamp"] != null && DateTime.Now.Subtract(DateTime.Parse(tune["timestamp"].InnerText)).TotalSeconds <= 60) {
					m_Account.PostActivityFeedItem(from, "music", null, String.Format("{0} - {1}", tune.Artist, tune.Title), tune.Uri);
				}
			}
		}
		
		private void TrackChanged (object sender, EventArgs args)
		{			
			NowPlayingService nowPlaying = ServiceManager.Get<NowPlayingService>();
			
			PubSubItem itemElement = new PubSubItem(m_Account.Client.Document);
			itemElement.SetAttribute("id", "current");

			Tune tune = new Tune(m_Account.Client.Document);
			itemElement.AppendChild(tune);

			if (nowPlaying.IsPlaying) {
				tune.Artist = nowPlaying.CurrentTrackArtist;
				tune.Length = nowPlaying.CurrentTrackLength;
				tune.Rating = nowPlaying.CurrentTrackRating;
				tune.Source = nowPlaying.CurrentTrackSource;
				tune.Title  = nowPlaying.CurrentTrackTitle;
				tune.Track  = nowPlaying.CurrentTrackNumber;
				tune.Uri    = nowPlaying.CurrentTrackUri;				
			}
		    
			m_Account.GetFeature<PersonalEventing>().Publish(Namespace.Tune, itemElement);
		}
		
		public string[] FeatureNames {
			get {
				return new string[] {
					Namespace.Tune,
					Namespace.Tune + "+notify"
				};
			}
		}

		public class Tune : Element
		{
			public Tune (XmlDocument doc) : base ("tune", Namespace.Tune, doc)
			{
				// FIXME: Abstract this, I suspect I'll be using it many places.
				XmlElement timestampElement = new Element("timestamp", "http://synapse.im/protocol/timestamp", doc);
				timestampElement.InnerText = DateTime.Now.ToUniversalTime().ToString("o");
				this.AppendChild(timestampElement);
			}

			public Tune (string prefix, XmlQualifiedName qname, XmlDocument doc) :
					base (qname.Name, doc)
			{
			}
				
			public string Artist {
				get {
					return GetElem("artist");
				}
				set {
					SetElem("artist", value);
				}
			}

			public string Length {
				get {
					return GetElem("length");
				}
				set {
					SetElem("length", value);
				}
			}

			public string Rating {
				get {
					return GetElem("rating");
				}
				set {
					SetElem("rating", value);
				}
			}

			public string Source {
				get {
					return GetElem("source");
				}
				set {
					SetElem("source", value);
				}
			}

			public string Title {
				get {
					return GetElem("title");
				}
				set {
					SetElem("title", value);
				}
			}

			public string Track {
				get {
					return GetElem("track");
				}
				set {
					SetElem("track", value);
				}
			}

			public string Uri {
				get {
					return GetElem("uri");
				}
				set {
					SetElem("uri", value);
				}
			}
		}
	}
}