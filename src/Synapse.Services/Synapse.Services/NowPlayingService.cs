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
using NDesk.DBus;
using Synapse.Core;
using org.freedesktop.DBus;
using Synapse.ServiceStack;

namespace Synapse.Services
{
	public class NowPlayingService : IService, IInitializeService
	{
		string m_Artist;
		ushort m_Length;
		int    m_Rating;
		string m_Source;
		string m_Title;
		string m_Track;
		string m_Uri;
		
		List<org.freedesktop.IMediaPlayer> m_Players
			= new List<org.freedesktop.IMediaPlayer>();
		
		public event EventHandler TrackChanged;
		
		#region Public Properties
		public string CurrentTrackArtist {
			get {
				return m_Artist;
			}
		}
		
		public ushort CurrentTrackLength {
			get {
				return m_Length;
			}
		}
		
		public int CurrentTrackRating {
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
			Bus sessionBus = Bus.Session;
			IBus bus = sessionBus.GetObject<IBus>("org.freedesktop.DBus", new ObjectPath("/org/freedesktop/DBus"));
			foreach (string name in bus.ListNames()) {
				if (name.StartsWith("org.mpris.")) {
					org.freedesktop.IMediaPlayer player = sessionBus.GetObject<org.freedesktop.IMediaPlayer>(name, new ObjectPath("/Player"));
					m_Players.Add(player);
					player.TrackChange += player_TrackChange;
				}
			}
		}
		
		private void player_TrackChange (IDictionary<string,object> stuffs)
		{
			m_Artist = null;
			m_Length = 0;
			m_Rating = -1;
			m_Source = null;
			m_Title  = null;
			m_Track  = null;
			m_Uri    = null;

			foreach (var pair in stuffs) {
				Console.WriteLine(pair.Key + " == " + pair.Value);
				switch (pair.Key) {
					case "artist":
						m_Artist = pair.Value.ToString();
						break;
					case "length":
						m_Length = (ushort)(UInt64.Parse(pair.Value.ToString()) / 60000);
						break;
					case "title":
						m_Title = pair.Value.ToString();
						break;
					case "album":
						m_Source = pair.Value.ToString();
						break;
				}
			}
			
			if (TrackChanged != null)
				TrackChanged(this, EventArgs.Empty);
		}

		string IService.ServiceName {
			get { return "NowPlayingService"; }
		}
	}
}

namespace org.freedesktop
{
	public delegate void TrackChangeEventHandler(IDictionary<string,object> stuffs);
	[Interface("org.freedesktop.MediaPlayer")]
	public interface IMediaPlayer
	{
		event TrackChangeEventHandler TrackChange;
	}	
}