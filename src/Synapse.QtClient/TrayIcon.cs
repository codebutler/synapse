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
	
namespace Synapse.QtClient
{
	public class TrayIcon : QObject, IDisposable
	{
		QSystemTrayIcon m_Icon;
		
		QMenu m_Menu;
		
		QAction m_ShowMainWindowAction;
		QAction m_ShowDebugWindowAction;
		
		public TrayIcon ()
		{
			m_ShowMainWindowAction = new QAction("Show Synapse", this);
			m_ShowMainWindowAction.Checkable = true;
			QObject.Connect(m_ShowMainWindowAction, Qt.SIGNAL("triggered()"), this, Qt.SLOT("HandleShowMainWindowActionTriggered()"));

			m_ShowDebugWindowAction = new QAction("Debug Window", this);
			m_ShowDebugWindowAction.Checkable = true;
			QObject.Connect(m_ShowDebugWindowAction, Qt.SIGNAL("triggered()"), this, Qt.SLOT("HandleShowDebugWindowActionTriggered()"));
			
			m_Menu = new QMenu();
			m_Menu.AddAction(m_ShowMainWindowAction);
			m_Menu.AddAction(m_ShowDebugWindowAction);
			m_Menu.AddSeparator();
			m_Menu.AddAction(Gui.GlobalActions.NewMessageAction);
			m_Menu.AddAction(Gui.GlobalActions.JoinConferenceAction);
			m_Menu.AddAction(Gui.GlobalActions.ShowBrowserAction);
			m_Menu.AddAction(Gui.GlobalActions.EditProfileAction);
			m_Menu.AddAction(Gui.GlobalActions.ChangeStatusAction);
			m_Menu.AddSeparator();
			m_Menu.AddAction(Gui.GlobalActions.ShowPreferencesAction);
			m_Menu.AddSeparator();
			m_Menu.AddAction(Gui.GlobalActions.AboutAction);
			m_Menu.AddAction(Gui.GlobalActions.SendFeedbackAction);
			m_Menu.AddSeparator();
			m_Menu.AddAction(Gui.GlobalActions.QuitAction);
			QObject.Connect(m_Menu, Qt.SIGNAL("aboutToShow()"), new NoArgDelegate(HandleMenuAboutToShow));

			QPixmap pixmap = new QPixmap("resource:/octy-22.png");
			QIcon icon = new QIcon(pixmap);
			m_Icon = new QSystemTrayIcon(icon);
			m_Icon.SetContextMenu(m_Menu);
			
			QObject.Connect(m_Icon, Qt.SIGNAL("activated(QSystemTrayIcon::ActivationReason)"), 
			                new OneArgDelegate<QSystemTrayIcon.ActivationReason>(HandleTrayActivated));
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

		void HandleMenuAboutToShow ()
		{
			m_ShowMainWindowAction.Checked = Gui.MainWindow.IsVisible();
			m_ShowDebugWindowAction.Checked = Gui.DebugWindow.IsVisible();
		}
		
		[Q_SLOT]
		void HandleShowMainWindowActionTriggered ()
		{
			if (m_ShowMainWindowAction.Checked)
				Gui.MainWindow.Show();
			else
				Gui.MainWindow.Hide();
		}

		[Q_SLOT]
		void HandleShowDebugWindowActionTriggered ()
		{
			if (m_ShowDebugWindowAction.Checked)
				Gui.DebugWindow.Show();
			else
				Gui.DebugWindow.Hide();	
		}
		
		[Q_SLOT]
		void HandleTrayActivated(QSystemTrayIcon.ActivationReason reason)
		{
			if (reason == QSystemTrayIcon.ActivationReason.Trigger) {
				if (Gui.MainWindow.IsVisible())
					Gui.MainWindow.Hide();
				else
					Gui.MainWindow.Show();
			}
		}
	}
}
