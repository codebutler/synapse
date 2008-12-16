//
// MainWindowView.cs
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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Qyoto;
using Synapse.Core;
using Synapse.UI;
using Synapse.UI.Views;
using Synapse.UI.Controllers;
using Synapse.ServiceStack;
using Synapse.Xmpp;

namespace Synapse.QtClient.UI.Views
{
	public partial class MainWindow : QWidget, IMainWindowView
	{
		NoAccountsWidget m_NoAccountsWidget;
		RosterWidget     m_RosterWidget;

		string m_StyleSheet;
		string m_NoAccountsStyleSheet;

		public event PresenceChangedEventHandler PresenceChanged;
		
		public event DialogValidateEventHandler AddNewAccount
		{
			add {
				m_NoAccountsWidget.AddNewAccount += value;
			}
			remove {
				m_NoAccountsWidget.AddNewAccount -= value;
			}
		}

		public MainWindow(MainWindowController controller)
		{
			SetupUi();
			base.WindowFlags = (uint)Qt.WindowType.FramelessWindowHint;

			QPixmap pixmap = new QPixmap("resource:/tray.png");
			base.WindowIcon = new QIcon(pixmap);
			
			QVBoxLayout layout = new QVBoxLayout();
			layout.SetContentsMargins(0, 0, 0, 0);
			contentWidget.SetLayout(layout);

			m_NoAccountsStyleSheet = Util.ReadResource("mainwindow-noaccounts.qss");
			m_StyleSheet = Util.ReadResource("mainwindow.qss");				

			this.SetStyleSheet(m_StyleSheet);
			
			m_RosterWidget = new RosterWidget(this);
			contentWidget.Layout().AddWidget(m_RosterWidget);
			
			m_NoAccountsWidget = new NoAccountsWidget();
			contentWidget.Layout().AddWidget(m_NoAccountsWidget);

			Helper.CenterWidgetOnScreen(this);

			headerLabel.InstallEventFilter(new WindowMover(this));
		}
		
		public new void Show ()
		{
			HideShowNoAccountsWidget();			
			base.Show();
		}

		public string Login {
			get {
				return m_NoAccountsWidget.Login;
			}
		}

		public string Password {
			get {
				return m_NoAccountsWidget.Password;
			}
		}
		
		public void AddAccount(Account account)
		{
			m_RosterWidget.AddAccount(account);
			account.ActivityFeed.NewItem += HandleNewItem;
			HideShowNoAccountsWidget();
		}

		public void RemoveAccount(Account account)
		{
			m_RosterWidget.RemoveAccount(account);
			HideShowNoAccountsWidget();
		}
		
		void HandleNewItem(Account account, IActivityFeedItem item)
		{
			Application.Invoke(delegate {
				m_RosterWidget.AddActivityFeedItem(account, item);
			});
		}

		void HideShowNoAccountsWidget ()
		{
			if (m_RosterWidget.AccountsCount > 0) {
				this.SetStyleSheet("max-width: -1px");
				containerWidget.SetStyleSheet(m_StyleSheet);
				m_NoAccountsWidget.Hide();
				m_RosterWidget.Show();
			} else {
				this.SetStyleSheet("max-width: 384px;");
				containerWidget.SetStyleSheet(m_NoAccountsStyleSheet);
				m_RosterWidget.Hide();
				m_NoAccountsWidget.Show();
			}
		}

		internal void RaisePresenceChanged (Account account, string presence, string statusText)
		{
			if (PresenceChanged != null)
				PresenceChanged(account, presence, statusText);
		}
	}
}
