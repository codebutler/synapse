	//
// ServiceDiscovery.cs: [Describe this file here].
//
// Copyright (c) 2008 Dronelabs LLC.
//
// Authors:
//   eric
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
using System.Linq;
using Loudmouth;

namespace Dragon.Core.Xmpp
{
	public class ServiceDiscovery : IDiscoverable
	{
		Account m_Account;
		
		public ServiceDiscovery(Account account)
		{
			m_Account = account;
			account.RegisterMessageHandler(MessageType.Iq, HandlerPriority.Normal, ReceivedDiscoRequest);
		}
		
		public void GetInfo (Jid entity, string node, MessageHandlerFunc callback)
		{
			Message m = new Message(entity.ToString(), MessageType.Iq, MessageSubType.Get);
			MessageNode queryNode = m.Node.AddChild("query", null);
			queryNode.SetAttribute("xmlns", Namespace.DiscoInfo);
			queryNode.SetAttribute("node", node);
			m_Account.Send(m, callback);
		}
		
		public string[] FeatureNames {
			get {
				return new string[] { Namespace.DiscoInfo };
			}
		}
		
		private HandlerResult ReceivedDiscoRequest (Account account, Message message)
		{
			if (message.SubType == MessageSubType.Get && message.Node.FirstChild != null &&
				message.Node.FirstChild.Name == "query" && 
				message.Node.FirstChild.GetAttribute("xmlns") == Namespace.DiscoInfo)
			{
				string from = message.Node.GetAttribute("from");
				string id   = message.Node.GetAttribute("id");
				
				Console.WriteLine("Received disco info request from " + from);
								
				
				Message resultMessage = new Message(from, MessageType.Iq, MessageSubType.Result);
				resultMessage.Node.SetAttribute("id", id);
				MessageNode queryNode = resultMessage.Node.AddChild("query", null);
				queryNode.SetAttribute("xmlns", Namespace.DiscoInfo);
				
				foreach (Identity identity in m_Account.Identities) {
					MessageNode identityNode = queryNode.AddChild("identity", null);
					identityNode.SetAttribute("category", identity.Category.ToString());
					identityNode.SetAttribute("type", identity.Type.ToString());
					identityNode.SetAttribute("name", identity.Name);
				}
				
				foreach (string featureName in m_Account.Features.SelectMany(x => x.FeatureNames)) {
					MessageNode featureNode = queryNode.AddChild("feature", null);
					featureNode.SetAttribute("var", featureName);
				}			 
				
				account.Send(resultMessage);
			}
			return HandlerResult.AllowMoreHandlers;
		}
	}
	
	public class ClientCapabilities
	{
		string[] m_Features;
		string[] m_Identities;
		
		public string[] Features {
			get {
				return m_Features;
			}
			internal set {
				m_Features = value;
			}
		}
				
		public string[] Identities {
			get {
				return m_Identities;
			}
			internal set {
				m_Identities = value;
			}
		}
	}
}
