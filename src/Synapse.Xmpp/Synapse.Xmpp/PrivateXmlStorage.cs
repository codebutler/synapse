//
// PrivateXmlStorage.cs: Store/retreive arbitrary XML data according to 
//                       XEP-0049.
//
// Copyright (c) 2008 Dronelabs LLC.
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
using Loudmouth;

namespace Dragon.Core.Xmpp
{
	public class PrivateXmlStorage : IDiscoverable
	{
		Account m_Account;
		
		public PrivateXmlStorage(Account account)
		{
			m_Account = account;
		}
		
		public void GetData (string elementName, string elementNamespace, MessageHandlerFunc callback)
		{
			Message message = new Message(null, MessageType.Iq, MessageSubType.Get);
			MessageNode query = message.Node.AddChild("query", null);
			query.SetAttribute("xmlns", Namespace.PrivateXmlStorage);
			MessageNode child = query.AddChild(elementName, null);
			child.SetAttribute("xmlns", elementNamespace);
			m_Account.Send(message, callback);
		}
		
		public void SetData (string xml, MessageHandlerFunc callback)
		{
			Message message = new Message(null, MessageType.Iq, MessageSubType.Set);
			MessageNode query = message.Node.AddChild("query", xml);
			query.SetAttribute("xmlns", Namespace.PrivateXmlStorage);
			m_Account.Send(message, callback);
		}
		
		public string[] FeatureNames {
			get {
				return new string[] { Namespace.PrivateXmlStorage };
			}
		}
	}
}