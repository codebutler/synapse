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

using Synapse.Core;
using Synapse.UI;
using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;
using Synapse.QtClient.Widgets;

using Qyoto;

namespace Synapse.QtClient.Windows
{
	public partial class MainWindow : QWidget
	{
		NoAccountsWidget m_NoAccountsWidget;
		RosterWidget     m_RosterWidget;
		
		QMenu m_MainMenu;

		string m_StyleSheet;
		string m_NoAccountsStyleSheet;
		
		public MainWindow()
		{
			SetupUi();
			base.WindowFlags = (uint)Qt.WindowType.FramelessWindowHint;
			
			closeButton.icon = new QIcon(new QPixmap("resource:/stock-close_12.png"));
			menuButton.icon = new QIcon(new QPixmap("resource:/menu-icon.png"));

			// FIXME: Add a global "Application Icon" somewhere that contains multiple sizes.
			QPixmap pixmap = new QPixmap("resource:/octy-22.png");
			base.WindowIcon = new QIcon(pixmap);
			
			m_NoAccountsStyleSheet = Util.ReadResource("mainwindow-noaccounts.qss");
			m_StyleSheet = Util.ReadResource("mainwindow.qss");				

			containerWidget.SetStyleSheet(m_StyleSheet);

			QVBoxLayout layout = new QVBoxLayout();
			layout.SetContentsMargins(0, 0, 0, 0);
			contentWidget.SetLayout(layout);

			m_RosterWidget = new RosterWidget(contentWidget);
			contentWidget.Layout().AddWidget(m_RosterWidget);
			
			m_NoAccountsWidget = new NoAccountsWidget(contentWidget);
			contentWidget.Layout().AddWidget(m_NoAccountsWidget);
			
			m_MainMenu = new QMenu(this);
			m_MainMenu.AddAction(Gui.GlobalActions.NewMessageAction);
			m_MainMenu.AddAction(Gui.GlobalActions.JoinConferenceAction);
			m_MainMenu.AddAction(Gui.GlobalActions.ShowBrowserAction);
			m_MainMenu.AddAction(Gui.GlobalActions.EditProfileAction);
			m_MainMenu.AddAction(Gui.GlobalActions.ChangeStatusAction);
			m_MainMenu.AddSeparator();
			m_MainMenu.AddAction(Gui.GlobalActions.ShowPreferencesAction);
			m_MainMenu.AddSeparator();
			m_MainMenu.AddAction(Gui.GlobalActions.AboutAction);
			m_MainMenu.AddAction(Gui.GlobalActions.SendFeedbackAction);
			m_MainMenu.AddSeparator();
			m_MainMenu.AddAction(Gui.GlobalActions.QuitAction);

			Gui.CenterWidgetOnScreen(this);

			headerLabel.InstallEventFilter(new WindowMover(this));

			AccountService accountService = ServiceManager.Get<AccountService>();
			accountService.AccountAdded   += AddAccount;
			accountService.AccountRemoved += RemoveAccount;

			foreach (Account account in accountService.Accounts) {
				AddAccount(account);
			}
		}
		
		public new void Show ()
		{
			HideShowNoAccountsWidget();			
			base.Show();
		}
		
		public void AddAccount(Account account)
		{
			QApplication.Invoke(delegate {
				m_RosterWidget.AddAccount(account);
				HideShowNoAccountsWidget();
			});
		}

		public void RemoveAccount(Account account)
		{
			QApplication.Invoke(delegate {
				m_RosterWidget.RemoveAccount(account);
				HideShowNoAccountsWidget();
			});
		}

		public void ShowLightbox (QWidget widget)
		{
			stackedWidget.ShowLightbox(widget);
		}

		public void HideLightbox ()
		{
			stackedWidget.HideLightbox();
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
		
		[Q_SLOT]
		void on_closeButton_clicked ()
		{
			base.Hide();
		}
		
		[Q_SLOT]
		void on_menuButton_clicked ()
		{
			var buttonPos = menuButton.MapToGlobal(new QPoint(0, menuButton.Height()));
			m_MainMenu.Popup(buttonPos);
		}
	}
}
