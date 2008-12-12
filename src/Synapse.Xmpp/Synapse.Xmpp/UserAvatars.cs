//
// UserAvatars.cs
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
using System.Xml;
using jabber;

namespace Synapse.Xmpp
{
	public class UserAvatars : IDiscoverable
	{
		Account m_Account;
		
		public UserAvatars(Account account)
		{
			m_Account = account;
			account.GetFeature<PersonalEventing>().RegisterHandler("urn:xmpp:avatar:metadata", ReceivedAvatarMetadata);
		}
		
		public string[] FeatureNames {
			get {
				return new string[] { "urn:xmpp:avatar:metadata+notify" };
			}
		}

		private void ReceivedAvatarMetadata (JID from, string node, XmlNode items)
		{
			if (items.ChildNodes.Count == 0)
				return;

			Console.WriteLine("Received Avatar Data");
			Console.WriteLine(items.ToString());
		}
		
	}
}
