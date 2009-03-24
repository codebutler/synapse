//
// ClientStatus.cs
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
using System.Text.RegularExpressions;

namespace Synapse.Xmpp
{
	public enum ClientAvailability
	{
		Available,
		FreeToChat,
		Away,
		ExtendedAway,
		DoNotDisturb
	}
	
	public class ClientStatus
	{
		public ClientStatus(string availabilityDescription, string statusText)
		{
			switch (availabilityDescription) {
			case "Available":
			case null:
				Availability = ClientAvailability.Available;
				break;
			case "Free To Chat":
			case "chat":
				Availability = ClientAvailability.FreeToChat;
				break;
			case "Away":
			case "away":
				Availability = ClientAvailability.Away;
				break;
			case "Extended Away":
			case "xa":
				Availability = ClientAvailability.ExtendedAway;
				break;
			case "Do Not Disturb":
			case "dnd":
				Availability = ClientAvailability.DoNotDisturb;
				break;
			default:
				throw new ArgumentException("Unknown availability: " + availabilityDescription);
			}
			
			StatusText = statusText;
		}
		
		public ClientStatus(ClientAvailability availability, string statusText)
		{
			Availability = availability;
			StatusText   = statusText;
		}

		public ClientAvailability Availability {
			get;
			set;
		}

		public string AvailabilityDisplayName {
			get {
				return Regex.Replace(this.Availability.ToString(), @"(\B[A-Z])", @" $1");
			}
		}

		public string StatusText {
			get;
			set;
		}
	}
}
