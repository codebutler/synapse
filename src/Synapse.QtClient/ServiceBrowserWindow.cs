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
using Synapse.UI.Controllers;
using Synapse.UI.Views;
using Qyoto;

namespace Synapse.QtClient
{
	public partial class ServiceBrowserWindow : QWidget, IServiceBrowserWindowView
	{
		QToolBar m_Toolbar;
		QAction  m_BackAction;
		QAction  m_ForwardAction;
		QAction  m_ReloadAction;
		QAction  m_StopAction;
		QAction  m_HomeAction;
		QAction  m_GoAction;
		
		QComboBox m_AddresCombo;

		ServiceBrowserWindowController m_Controller;
		
		public event UrlEventHandler UrlRequested;
		
		public ServiceBrowserWindow (ServiceBrowserWindowController controller)
		{
			SetupUi();

			m_Controller = controller;
			
			this.WindowTitle = String.Format("XMPP Browser - {0}", controller.Account.Jid);
	
			m_BackAction    = new QAction(Helper.LoadIcon("back", 16), "Back", this);
			m_ForwardAction = new QAction(Helper.LoadIcon("forward", 16), "Forward", this);
			m_ReloadAction  = new QAction(Helper.LoadIcon("reload", 16), "Reload", this);
			m_StopAction    = new QAction(Helper.LoadIcon("stop", 16), "Stop", this);
			m_HomeAction    = new QAction(Helper.LoadIcon("go-home", 16), "Home", this);
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

			QObject.Connect(m_Toolbar, Qt.SIGNAL("actionTriggered(QAction*)"), this, Qt.SLOT("toolbar_actionTriggered(QAction*)"));
			
			((QBoxLayout)this.Layout()).InsertWidget(0, m_Toolbar);

			webView.Page().linkDelegationPolicy = QWebPage.LinkDelegationPolicy.DelegateAllLinks;
			//QObject.Connect(webView, Qt.SIGNAL("linkClicked(QUrl)"), this, we
		}

		public void RequestUrl (Uri uri)
		{
			// FIXME: Actually we want to show this as a lightbox or something.
			LoadContent(uri, "Loading...");

			try {
				var evnt = UrlRequested;
				if (evnt != null)
					evnt(uri);
			} catch (Exception ex) {
				Console.Error.WriteLine(ex);
				QMessageBox.Critical(this, "Unable to load URL", ex.Message);
			}
		}
		
		public void LoadContent (Uri uri, string html)
		{
			m_AddresCombo.LineEdit().Text = uri.ToString();
			webView.SetHtml(html, new QUrl(uri.ToString()));
		}

		[Q_SLOT]
		private void toolbar_actionTriggered(QAction action)
		{
			if (action == m_GoAction) {
				RequestUrl(new Uri(m_AddresCombo.LineEdit().Text));
			} else if (action == m_HomeAction) {
				RequestUrl(m_Controller.HomeUri);
			}
		}

		[Q_SLOT]
		void on_webView_linkClicked (QUrl url)
		{
			RequestUrl(new Uri(url.ToString()));
		}
	}
}