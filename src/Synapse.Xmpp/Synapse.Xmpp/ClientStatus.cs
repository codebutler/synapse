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

namespace Synapse.Xmpp
{
	public enum ClientStatusType
	{
		Available,
		FreeToChat,
		Away,
		ExtendedAway,
		DoNotDisturb,
		Invisible,
		Offline
	}
	
	public class ClientStatus
	{
		public ClientStatus(string typeDescription, string statusText)
		{
			switch (typeDescription) {
			case "Available":
			case null:
				Type = ClientStatusType.Available;
				break;
			case "Free to Chat":
			case "chat":
				Type = ClientStatusType.FreeToChat;
				break;
			case "Away":
			case "away":
				Type = ClientStatusType.Away;
				break;
			case "Extended Away":
			case "xa":
				Type = ClientStatusType.ExtendedAway;
				break;
			case "Do Not Disturb":
			case "dnd":
				Type = ClientStatusType.DoNotDisturb;
				break;
			case "Offline":
				Type = ClientStatusType.Offline;
				break;
			default:
				throw new ArgumentException("Unknown status type: " + typeDescription);
			}
			
			StatusText = statusText;
		}
		
		public ClientStatus(ClientStatusType type, string statusText)
		{
			Type       = type;
			StatusText = statusText;
		}

		public ClientStatusType Type {
			get;
			set;
		}

		public string StatusText {
			get;
			set;
		}
	}
}
