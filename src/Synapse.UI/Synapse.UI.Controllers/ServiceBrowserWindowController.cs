//
// ServiceBrowserWindowController.cs
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
using System.Reflection;
using Synapse.ServiceStack;
using Synapse.Core;
using Synapse.Xmpp;
using Synapse.UI.Views;
using jabber;
using jabber.connection;
using TemplateEngine;

namespace Synapse.UI.Controllers
{
	public class ServiceBrowserWindowController : AbstractController<IServiceBrowserWindowView>
	{
		Account m_Account;
		
		public ServiceBrowserWindowController (Account account)
		{
			m_Account = account;
			
			Application.InvokeAndBlock(delegate {
				InitializeView();
				View.UrlRequested += HandleViewUrlRequested;
				View.Show();
			});

			OpenUri(HomeUri);
		}

		public Uri HomeUri {
			get {
				return new Uri(String.Format("xmpp:{0}?disco", m_Account.Jid.Server));
			}
		}

		public Account Account {
			get {
				return m_Account;
			}
		}

		public void OpenUri (Uri uri)
		{
			Application.Invoke(delegate {
				View.RequestUrl(uri);
			});
		}

		private void HandleViewUrlRequested (Uri uri)
		{
			if (m_Account.ConnectionState != AccountConnectionState.Connected) {
				Application.Invoke(delegate {
					View.LoadContent(uri, "You are not connected.");
				});
				return;
			}
			
			var info = XmppUriQueryInfo.ParseQuery(uri.Query);

			string queryType = info.QueryType;
			// Default to disco.
			if (String.IsNullOrEmpty(queryType))
				queryType = "disco";

			switch (queryType) {
			case "disco":			
				var jid = new JID(uri.AbsolutePath);
				string node = null;
				if (info.Parameters.ContainsKey("node"))
					node = info.Parameters["node"];				
				var discoNode = new jabber.connection.DiscoNode(jid, node);
				m_Account.DiscoManager.BeginGetFeatures(discoNode, new DiscoNodeHandler(ReceivedFeatures), null);
				break;
			default:
				throw new Exception("Unsupported query type: " + queryType);
			}
		}

		private void ReceivedFeatures (DiscoManager manager, DiscoNode node, object state)
		{
			// Now query for items...
			m_Account.DiscoManager.BeginGetItems(node, new DiscoNodeHandler(ReceivedItems), null);
		}

		private void ReceivedItems (DiscoManager manager, DiscoNode node, object state)
		{
			string jid = node.JID.ToString();
			string nodeName = String.Empty;
			if (node.Node != null)
				nodeName = node.Node;
			
			string templateContent = Util.ReadResource("ServiceDiscovery.html");
			Template template = new Template(templateContent);
			template.SetField("NAME", jid);
			template.SetField("HREF", String.Format("xmpp:{0}?disco", jid));
			template.SetField("NODE", nodeName);
			
			template.SelectSection("FEATURES");
			foreach (var feature in node.FeatureNames) {
				template.SetField("FEATURE_NAME", feature);
				template.AppendSection();
			}			
			template.DeselectSection();

			template.SelectSection("ITEMS");
			foreach (DiscoNode item in node.Children) {
				template.SetField("ITEM_NAME", item.Name);
				template.SetField("ITEM_URL", String.Format("xmpp:{0}?disco;node={1}", item.JID.ToString(), item.Node));
				template.AppendSection();
			}
			template.DeselectSection();
			
			template.SelectSection("IDENTITIES");
			foreach (var identity in node.GetIdentities()) {
				template.SetField("IDENTITY_NAME", identity.Name);
				template.SetField("IDENTITY_CATEGORY", identity.Category);
				template.SetField("IDENTITY_TYPE", identity.Type);
				template.AppendSection();
			}
			template.DeselectSection();
			
			Application.Invoke(delegate {
				Uri uri = new Uri(String.Format("xmpp:{0}?disco;node={1}", node.JID.ToString(), node.Node));
				View.LoadContent(uri, template.getContent());
			});
		}
	}
}
