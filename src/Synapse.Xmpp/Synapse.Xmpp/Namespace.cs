//
// Namespace.cs: Constants for XMPP protocol namespaces. 
//               For a complete list, see: 
//               http://www.xmpp.org/registrar/namespaces.html
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
	public static class Namespace
	{
		// RFC 3921: XMPP IM
		// http://www.ietf.org/rfc/rfc3921.txt
		public const string Roster  = "jabber:iq:roster";
		
		// XEP-0049: Private XML Storage
		// http://www.xmpp.org/extensions/xep-0049.html
		public const string PrivateXmlStorage = "jabber:iq:private";
		
		// XEP-0060: Publish-Subscribe
		// http://www.xmpp.org/extensions/xep-0060.html
		public const string PubSub       = "http://jabber.org/protocol/pubsub";
		public const string PubSubEvent  = "http://jabber.org/protocol/pubsub#event";
		public const string PubSubErrors = "http://jabber.org/protocol/pubsub#errors";
		public const string PubSubOwner  = "http://jabber.org/protocol/pubsub#owner";
		
		// XEP-0115: Entity Capabilities
		// http://www.xmpp.org/extensions/xep-0115.html
		public const string Caps = "http://jabber.org/protocol/caps";
		
		public const string DiscoInfo  = "http://jabber.org/protocol/disco#info";
		public const string DiscoItems = "http://jabber.org/protocol/disco#items";

		// XEP-0085: Chat State Notifications
		// http://xmpp.org/extensions/xep-0085.html
		public const string ChatStates = "http://jabber.org/protocol/chatstates";
	}
}
