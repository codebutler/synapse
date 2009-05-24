//
// Helper.cs
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
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using Qyoto;

using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;
using Synapse.QtClient.Windows;

namespace Synapse.QtClient
{
	public static class Gui
	{
		static DebugWindow s_DebugWindow;
		static GlobalActions s_GlobalActions;
		
		public static MainWindow MainWindow {
			get;
			set;
		}
		
		public static TrayIcon TrayIcon {
			get;
			set;
		}

		public static DebugWindow DebugWindow {
			get {
				if (s_DebugWindow == null)
					s_DebugWindow = new DebugWindow();
				return s_DebugWindow;
			}
		}

		public static TabbedChatsWindow TabbedChatsWindow {
			get;
			set;
		}
		
		public static GlobalActions GlobalActions {
			get {
				if (s_GlobalActions == null)
					s_GlobalActions = new GlobalActions();
				return s_GlobalActions;
			}
		}
		
		public static void ShowPreferencesWindow()
		{
			var preferencesWindow = new PreferencesWindow();
			preferencesWindow.Show();
			// FIXME: Make this a dialog...
			// preferencesWindow.Exec();
		}
		
		public static Account ShowAccountSelectMenu (QWidget attachWidget)
		{
			AccountService accountService = ServiceManager.Get<AccountService>();
	
			if (accountService.ConnectedAccounts.Count == 0) {
				var widget = (attachWidget != null) ? attachWidget.TopLevelWidget() : Gui.MainWindow;
				QMessageBox.Critical(widget, "Synapse", "You are not connected.");
				return null;
			}			

			Account selectedAccount = null;
			if (accountService.ConnectedAccounts.Count > 1) {
				QMenu menu = new QMenu();
				menu.AddAction("Select Account:").SetDisabled(true);
		
				foreach (Account account in accountService.ConnectedAccounts) {
					QAction action = menu.AddAction(account.Jid.ToString());
					if (menu.ActiveAction() == null)
						menu.SetActiveAction(action);
				}
				
				var pos = (attachWidget != null) ? attachWidget.MapToGlobal(new QPoint(0, attachWidget.Height())) : QCursor.Pos();
				QAction selectedAction = menu.Exec(pos);
				if (selectedAction != null) {
					selectedAccount = accountService.GetAccount(new jabber.JID(selectedAction.Text));
				}
			} else {
				selectedAccount = accountService.ConnectedAccounts[0];
			}
			return selectedAccount;
		}

		public static void CenterWidgetOnScreen (QWidget widget)
		{
			QRect rect = QApplication.Desktop().AvailableGeometry(widget);
			widget.Move((rect.Width() / 2) - (widget.Rect.Width() / 2), (rect.Height() / 2) - (widget.Rect.Height() / 2));
		}

		public static QApplication QApp {
			get {
				return ((Synapse.QtClient.Client)Synapse.ServiceStack.Application.Client).QApp;
			}
		}

		public static QIcon LoadIcon (string name)
		{
			QIcon icon = new QIcon();
			int[] sizes = null;
			
			// FIXME: Need to remove Gtk dependency.
			if (Gtk.IconTheme.Default != null) {
				sizes = Gtk.IconTheme.Default.GetIconSizes(name);
				if (sizes != null && sizes.Length > 0) {
					foreach (int size in sizes) {
						var iconInfo = Gtk.IconTheme.Default.LookupIcon(name, size, 0);
						if (iconInfo != null && System.IO.File.Exists(iconInfo.Filename))
							icon.AddFile(iconInfo.Filename, new QSize(size, size), QIcon.Mode.Normal, QIcon.State.On);
					}
				}
			}
			
			// If icon wasn't found in theme, try loading from resource instead...
			if (sizes == null || sizes.Length == 0 || icon.IsNull()) {
				var assembly = Assembly.GetExecutingAssembly();
				foreach (string resourceName in assembly.GetManifestResourceNames()) {
					string pattern =  "^" + Regex.Escape(name) + @"__(\d+)\.png$";
					var match = Regex.Match(resourceName, pattern);
					if (match.Success) {
						icon.AddPixmap(new QPixmap("resource:/" + resourceName), QIcon.Mode.Normal, QIcon.State.On);
					}
				}
				
				if (icon.IsNull()) {
					Console.WriteLine(String.Format("Icon not found: {0}", name));
				}
			}
			
			return icon;
		}
		
		public static QIcon LoadIcon (string name, int size)
		{
			if (Gtk.IconTheme.Default == null)
				throw new InvalidOperationException("No Default IconTheme");
			
			var iconInfo = Gtk.IconTheme.Default.LookupIcon(name, size, 0);
			if (iconInfo != null) {
				return new QIcon(iconInfo.Filename);
			} else {
				// If icon wasn't found in theme, try loading from resource instead...
				string resourceName = String.Format("{0}__{1}.png", name, size.ToString());
				var assembly = System.Reflection.Assembly.GetExecutingAssembly();
				if (assembly.GetManifestResourceNames().Contains(resourceName)) {
					return new QIcon(new QPixmap("resource:/" + resourceName));
				} else {
					Console.Error.WriteLine(String.Format("Icon not found: {0} ({1})", name, size));
					return new QIcon();
				}
			}			
		}
		
		public static void DrawAvatar (QPainter painter, int width, int height, QPixmap avatarPixmap)
		{
				QPainterPath path = new QPainterPath();
				
				// Draw a rect without corners.
				path.MoveTo(0, 1);
				path.LineTo(1, 1);
				path.LineTo(1, 0);
				path.LineTo(width - 1, 0);
				path.LineTo(width, 1);
				path.LineTo(width, height - 1);
				path.LineTo(width - 1, height - 1);
				path.LineTo(width - 1, height);
				path.LineTo(1, height);
				path.LineTo(1, height - 1);
				path.LineTo(0, height - 1);
				path.LineTo(0, 1);
														
				QLinearGradient g1 = new QLinearGradient(0, 0, 0, height);
				g1.SetColorAt(0, new QColor("#888781"));
				g1.SetColorAt(1, new QColor("#abaaa8"));
				QBrush b1 = new QBrush(g1);
				
				painter.FillPath(path, b1);
				painter.FillRect(1, 1, width - 2, height - 2, new QBrush(Qt.GlobalColor.black));
				
				// Darken the corners...
				var b2 = new QBrush(new QColor(61, 61, 61, 102));
				painter.FillRect(0, 0, 3, 3, b2);
				painter.FillRect(width - 3, 0, 3, 3, b2);
				painter.FillRect(0, width - 3, 3, 3, b2);
				painter.FillRect(width - 3, width - 3, 3, 3, b2);
				
				painter.DrawPixmap(2, 2, width - 4, height - 4, avatarPixmap);
		}

		public static void ShowErrorWindow (string errorTitle, string errorMessage, string errorDetail)
		{
			ErrorDialog dialog = new ErrorDialog(errorTitle, errorMessage, errorDetail, Gui.MainWindow);
			dialog.Show();
			dialog.Exec();
		}
	}
}
