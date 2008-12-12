//
// TrayIconView.cs
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
using Qyoto;
using Synapse.ServiceStack;
using Synapse.UI.Services;
using Synapse.UI.Views;
using Synapse.UI.Controllers;
	
namespace Synapse.QtClient.UI.Views
{
	public class TrayIconView : QObject, ITrayIconView
	{
		QSystemTrayIcon m_Icon;
		
		QMenu m_Menu;
		QMenu m_StatusMenu;
		
		QAction m_ShowMainWindowAction;
		QAction m_ShowPreferencesAction;
		QAction m_NewMessageAction;
		QAction m_JoinMucAction;
		QAction m_QuitAction;
		
		public TrayIconView(TrayIconController controller)
		{
			m_ShowMainWindowAction = new QAction("Show Synapse", this);
			m_ShowMainWindowAction.Checkable = true;
			QObject.Connect(m_ShowMainWindowAction, Qt.SIGNAL("triggered()"), this, Qt.SLOT("showMainWindowAction_triggered()"));

			m_StatusMenu = new QMenu("Change Status");
			
			m_ShowPreferencesAction = new QAction("Preferences", this);
			m_NewMessageAction      = new QAction("New Message...", this);
			m_JoinMucAction         = new QAction("Create/Join Conference...", this);
			
			m_QuitAction = new QAction("Quit", this);
			QObject.Connect(m_QuitAction, Qt.SIGNAL("triggered()"), this, Qt.SLOT("quitAction_triggered()"));
			
			m_Menu = new QMenu();
			m_Menu.AddAction(m_ShowMainWindowAction);
			m_Menu.AddSeparator();
			m_Menu.AddAction(m_NewMessageAction);
			m_Menu.AddAction(m_JoinMucAction);
			m_Menu.AddMenu(m_StatusMenu);
			m_Menu.AddSeparator();
			m_Menu.AddAction(m_ShowPreferencesAction);
			m_Menu.AddSeparator();
			m_Menu.AddAction(m_QuitAction);
			QObject.Connect(m_Menu, Qt.SIGNAL("aboutToShow()"), new NoArgDelegate(menu_aboutToShow));
			
			QIcon icon = new QIcon(":resource/tray.png");
			m_Icon = new QSystemTrayIcon(icon);
			m_Icon.SetContextMenu(m_Menu);
			
			QObject.Connect(m_Icon, Qt.SIGNAL("activated(QSystemTrayIcon::ActivationReason)"), 
			                new OneArgDelegate<QSystemTrayIcon.ActivationReason>(tray_activated));

		}

		public bool IsVisible ()
		{
			return m_Icon.Visible;
		}

		public void Show ()
		{
			m_Icon.Show();
		}

		public void Hide ()
		{
			m_Icon.Hide();
		}

		void IDisposable.Dispose ()
		{
			Hide();
			m_Icon.Dispose();
			m_Icon = null;
		}

		void menu_aboutToShow ()
		{
			var mainWindow = ServiceManager.Get<GuiService>().MainWindow;
			m_ShowMainWindowAction.Checked = mainWindow.View.IsVisible();
		}

		[Q_SLOT]
		void quitAction_triggered ()
		{
			Application.Shutdown();
		}

		[Q_SLOT]
		void showMainWindowAction_triggered ()
		{
			var mainWindow = ServiceManager.Get<GuiService>().MainWindow;				
			if (m_ShowMainWindowAction.Checked)
				mainWindow.View.Show();
			else
				mainWindow.View.Hide();	
		}
		
		[Q_SLOT]
		void tray_activated(QSystemTrayIcon.ActivationReason reason)
		{
			if (reason == QSystemTrayIcon.ActivationReason.Trigger) {
				var mainWindow = ServiceManager.Get<GuiService>().MainWindow;				
				if (mainWindow.View.IsVisible())
					mainWindow.View.Hide();
				else
					mainWindow.View.Show();
			}
		}
	}
}
