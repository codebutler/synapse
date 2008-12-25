//
// BansheeNowPlayingProvider.cs
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
using Synapse.ServiceStack;
using Synapse.Services;
using NDesk.DBus;
using org.freedesktop.DBus;
using Banshee.MediaEngine;

namespace Synapse.Addins.BansheeAddin
{
	public class BansheeNowPlayingProvider : INowPlayingProvider
	{
		IDictionary<string, object> m_CurrentTrack;
		IPlayerEngineService        m_Banshee;

		public event EventHandler TrackChanged;
		
		public BansheeNowPlayingProvider()
		{
			Bus sessionBus = Bus.Session;

			// FIXME: We check that the name exists because otherwise calling 
			// GetObject() will start banshee, but this won't work if Banshee 
			// is started after Synapse. What's the equivilent of the pydbus 
			// bus.add_signal_receiver() method?
			if (sessionBus.NameHasOwner("org.bansheeproject.Banshee")) {
				m_Banshee = sessionBus.GetObject<IPlayerEngineService>("org.bansheeproject.Banshee",
															           new ObjectPath("/org/bansheeproject/Banshee/PlayerEngine"));
				m_Banshee.EventChanged += HandleEventChanged;

				UpdateTrackInfo();
			}
		}

		public bool IsPlaying {
			get {
				return (m_CurrentTrack != null);
			}
		}

		public string Artist { 
			get {
				return GetStringOrNull("artist");
			} 
		}
		
		public string Length { 
			get {
				return GetStringOrNull("length");
			} 
		}
		
		public string Rating {
			get {
				return null;
			} 
		}
		
		public string Source {
			get {
				return GetStringOrNull("album");
			} 
		}
		
		public string Title {
			get {
				return GetStringOrNull("name"); 
			} 
		}
				
		public string Track {
			get {
				return GetStringOrNull("track-number");
			} 
		}
		
		public string Uri { 
			get {
				try {
					Uri uri = new Uri(GetStringOrNull("URI"));
					if (uri.Scheme == "http") {
						if (!String.IsNullOrEmpty(uri.UserInfo))
							return null; // Don't leak private data.
						else
							return uri.ToString();
					}
				} catch { }
				return null;
			} 
		}

		string GetStringOrNull (string key)
		{
			return (m_CurrentTrack.ContainsKey(key)) ? m_CurrentTrack[key].ToString() : null;
		}
		
		void HandleEventChanged(string evnt, string message, double bufferingPercent)
		{
			if (evnt == "trackinfoupdated" || evnt == "startofstream") {
				UpdateTrackInfo();
			}
		}

		void UpdateTrackInfo()
		{
			var currentTrack = m_Banshee.CurrentTrack;
			m_CurrentTrack = (currentTrack.Count > 1) ? currentTrack : null;
			if (TrackChanged != null)
				TrackChanged(this, EventArgs.Empty);

			//foreach (var k in m_CurrentTrack.Keys)
			//	Console.WriteLine(k + " == " + m_CurrentTrack[k]);
		}
	}
}
