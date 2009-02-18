//
// NowPlayingService.cs: Keeps track of what music is being listened to using
//                       the MPRIS spec http://wiki.xmms2.xmms.se/wiki/MPRIS and
//                       http://www.xmpp.org/extensions/xep-0118.html
//
// Authors:
//   Eric Butler <eric@extremeboredom.net>
//
// Copyright (C) 2008 Eric Butler
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
using Synapse.Core;
using Synapse.ServiceStack;
using Mono.Addins;

namespace Synapse.Services
{
	public class NowPlayingService : IService, IInitializeService
	{
		bool   m_IsPlaying;
		string m_Artist;
		string m_Length;
		string m_Rating;
		string m_Source;
		string m_Title;
		string m_Track;
		string m_Uri;

		List<INowPlayingProvider> m_Providers = new List<INowPlayingProvider>();
		
		public event EventHandler TrackChanged;
		
		#region Public Properties
		public bool IsPlaying {
			get {
				return m_IsPlaying;
			}
		}
		
		public string CurrentTrackArtist {
			get {
				return m_Artist;
			}
		}
		
		public string CurrentTrackLength {
			get {
				return m_Length;
			}
		}
		
		public string CurrentTrackRating {
			get {
				return m_Rating;
			}
		}
		
		public string CurrentTrackSource {
			get {
				return m_Source;
			}
		}
		
		public string CurrentTrackTitle {
			get {
				return m_Title;
			}
		}
		
		public string CurrentTrackNumber {
			get {
				return m_Track;
			}
		}
		
		public string CurrentTrackUri {
			get {
				return m_Uri;
			}
		}
	#endregion
	
		public void Initialize ()
		{
			AddinManager.AddExtensionNodeHandler("/Synapse/Services/NowPlaying/Providers", OnExtensionChanged);
		}

		void OnExtensionChanged (object o, ExtensionNodeEventArgs args)
		{
			TypeExtensionNode node = (TypeExtensionNode)args.ExtensionNode;
			INowPlayingProvider provider = (INowPlayingProvider)node.CreateInstance();
			provider.TrackChanged += HandleTrackChanged;
			m_Providers.Add(provider);

			HandleTrackChanged(provider, EventArgs.Empty);
		}

		void HandleTrackChanged(object sender, EventArgs e)
		{
			INowPlayingProvider provider = (INowPlayingProvider)sender;
			m_IsPlaying = provider.IsPlaying;
			if (provider.IsPlaying) {
				if (m_Artist != provider.Artist || m_Title != provider.Title) {
					m_Artist = provider.Artist;
					m_Length = provider.Length;
					m_Rating = provider.Rating;
					m_Source = provider.Source;
					m_Title  = provider.Title;
					m_Track  = provider.Track;
					m_Uri    = provider.Uri;
				} else {
					return;
				}
			} else {
				m_Artist = null;
				m_Length = null;
				m_Rating = null;
				m_Source = null;
				m_Title  = null;
				m_Track  = null;
				m_Uri    = null;
			}

			if (TrackChanged != null)
				TrackChanged(this, EventArgs.Empty);
		}
		
		string IService.ServiceName {
			get { return "NowPlayingService"; }
		}
	}

	public interface INowPlayingProvider
	{
		event EventHandler TrackChanged;
		bool IsPlaying { get; }
		string Artist { get; }
		string Length { get; }
		string Rating { get; }
		string Source { get; }
		string Title { get; }
		string Track { get; }
		string Uri { get; }
	}
}
