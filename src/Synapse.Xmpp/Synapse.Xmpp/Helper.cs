//
// Helper.cs
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
using jabber.protocol.client;

namespace Synapse.Xmpp
{	
	public static class Helper
	{
		public static string GetPresenceDisplay (Presence presence)
		{
			if (presence.Type == PresenceType.available) {
				if (!String.IsNullOrEmpty(presence.Show)) {
					switch (presence.Show) {
						case "away":
							return "away";
						case "chat":
							return "free to chat";
						case "dnd":
							return "do not disturb";
						case "xa":
							return "extended away";
						default:
							return presence.Show;
					}
				} else {
					return "available";
				}
			} else if (presence.Type == PresenceType.unavailable) {
				return "offline";
			} else {
				throw new ArgumentException("presence type not supported: " + presence.Type);
			}
		}
	}
}
