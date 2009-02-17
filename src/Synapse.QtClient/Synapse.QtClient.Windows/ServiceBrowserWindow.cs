//
// ServiceBrowserWindow.cs
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
using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.QtClient;
using Synapse.QtClient.Widgets;
using Qyoto;
using jabber;
using jabber.connection;
using TemplateEngine;

namespace Synapse.QtClient.Windows
{
	public partial class ServiceBrowserWindow : QWidget
	{
		QToolBar m_Toolbar;
		QAction  m_BackAction;
		QAction  m_ForwardAction;
		QAction  m_ReloadAction;
		QAction  m_StopAction;
		QAction  m_HomeAction;
		QAction  m_GoAction;
		
		QComboBox m_AddresCombo;

		Account m_Account;
		Uri     m_HomeUri;
		
		public ServiceBrowserWindow (Account account)
		{
			SetupUi();

			m_Account = account;
			m_HomeUri = new Uri(String.Format("xmpp:{0}?disco", account.Jid.Server));
			
			this.WindowTitle = String.Format("XMPP Browser - {0}", account.Jid);
	
			m_BackAction    = new QAction(Gui.LoadIcon("back", 16), "Back", this);
			m_ForwardAction = new QAction(Gui.LoadIcon("forward", 16), "Forward", this);
			m_ReloadAction  = new QAction(Gui.LoadIcon("reload", 16), "Reload", this);
			m_StopAction    = new QAction(Gui.LoadIcon("stop", 16), "Stop", this);
			m_HomeAction    = new QAction(Gui.LoadIcon("go-home", 16), "Home", this);
			m_GoAction      = new QAction("Go", this);

			m_BackAction.Enabled = false;
			m_ForwardAction.Enabled = false;
			
			m_StopAction.Visible = false;
			
			m_Toolbar = new QToolBar(this);
			m_Toolbar.AddAction(m_BackAction);
			m_Toolbar.AddAction(m_ForwardAction);
			m_Toolbar.AddAction(m_ReloadAction);
			m_Toolbar.AddAction(m_StopAction);
			m_Toolbar.AddAction(m_HomeAction);

			m_AddresCombo = new QComboBox(m_Toolbar);
			m_AddresCombo.SetSizePolicy(QSizePolicy.Policy.Expanding, QSizePolicy.Policy.Fixed);
			m_AddresCombo.Editable = true;
			m_Toolbar.AddWidget(m_AddresCombo);
			
			m_Toolbar.AddAction(m_GoAction);

			QObject.Connect<QAction>(m_Toolbar, Qt.SIGNAL("actionTriggered(QAction*)"), HandleToolbarActionTriggered);
			
			((QBoxLayout)this.Layout()).InsertWidget(0, m_Toolbar);

			webView.Page().linkDelegationPolicy = QWebPage.LinkDelegationPolicy.DelegateAllLinks;

			RequestUrl(m_HomeUri);
			
			Gui.CenterWidgetOnScreen(this);
		}
		
		public Account Account {
			get {
				return m_Account;
			}
		}

		public void RequestUrl (Uri uri)
		{
			// FIXME: Actually we want to show this as a lightbox or something.
			LoadContent(uri, "Loading...");

			if (m_Account.ConnectionState != AccountConnectionState.Connected) {
				LoadContent(uri, "You are not connected.");
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
		
		void LoadContent (Uri uri, string html)
		{
			m_AddresCombo.LineEdit().Text = uri.ToString();
			webView.SetHtml(html, new QUrl(uri.ToString()));
		}

		void HandleToolbarActionTriggered(QAction action)
		{
			if (action == m_GoAction) {
				RequestUrl(new Uri(m_AddresCombo.LineEdit().Text));
			} else if (action == m_HomeAction) {
				RequestUrl(m_HomeUri);
			}
		}

		[Q_SLOT]
		void on_webView_linkClicked (QUrl url)
		{
			RequestUrl(new Uri(url.ToString()));
		}

		void ReceivedFeatures (DiscoManager manager, DiscoNode node, object state)
		{
			// Now query for items...
			m_Account.DiscoManager.BeginGetItems(node, new DiscoNodeHandler(ReceivedItems), null);
		}

		void ReceivedItems (DiscoManager manager, DiscoNode node, object state)
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
			
			QApplication.Invoke(delegate {
				Uri uri = new Uri(String.Format("xmpp:{0}?disco;node={1}", node.JID.ToString(), node.Node));
				LoadContent(uri, template.getContent());
			});
		}
	}
}