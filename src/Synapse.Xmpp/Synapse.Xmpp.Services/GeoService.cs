//
// GeoService.cs: Keeps track of where in the world you and your friends are.
//
// Authors:
//   Eric Butler <eric@extremeboredom.net>
//
// Copyright (c) 2009 Eric Butler
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

using jabber;

namespace Synapse.Xmpp.Services
{
	public class GeoService : IRequiredService, IDelayedInitializeService
	{	
		Dictionary<JID, UserLocation> m_Locations;
		
		public void DelayedInitialize ()
		{
			m_Locations = new Dictionary<JID, UserLocation>();
		}
		
		public string ServiceName {
			get {
				return "GeoService";
			}
		}
	}
	
	public class UserLocation
	{
		JID m_Jid;
		string m_LocationName;
		string m_Latitude;
		string m_Longitude;
		string m_Source;
		
		public UserLocation (JID jid, string locationName, string latitude, string longitude, string source)
		{
			m_Jid          = jid;
			m_LocationName = locationName;
			m_Latitude     = latitude;
			m_Longitude    = longitude;
			m_Source       = source;
		}
		
		public JID Jid {
			get {
				return m_Jid;
			}
		}
		
		public string LocationName {
			get {
				return m_LocationName;
			}
		}
		
		public string Latitude {
			get {
				return m_Latitude;
			}
		}
		
		public string Longitude {
			get {
				return m_Longitude;
			}
		}
		
		public string Source {
			get {
				return m_Source;
			}
		}
	}
}
