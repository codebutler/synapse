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
using System.Collections.Generic;
using Synapse.ServiceStack;
using Synapse.Xmpp;
using Qyoto;
using Gtk;

namespace Synapse.QtClient
{
	public static class Gui
	{
		public static Account ShowAccountSelectMenu (QWidget attachWidget)
		{
			AccountService accountService = ServiceManager.Get<AccountService>();
	
			if (accountService.ConnectedAccounts.Count == 0) {
				QMessageBox.Critical(attachWidget.TopLevelWidget(), "Synapse", "You are not connected.");
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
				
				QAction selectedAction = menu.Exec(attachWidget.MapToGlobal(new QPoint(0, attachWidget.Height())));
				selectedAccount = accountService.GetAccount(new jabber.JID(selectedAction.Text));
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

		public static QIcon LoadIcon (string name)
		{
			if (IconTheme.Default == null)
				throw new InvalidOperationException("No Default IconTheme");

			QIcon icon = new QIcon();
			int[] sizes = IconTheme.Default.GetIconSizes(name);
			if (sizes.Length == 0) {
				Console.WriteLine(String.Format("Icon not found: {0}", name));
			} else {
				foreach (int size in sizes) {
					var iconInfo = IconTheme.Default.LookupIcon(name, size, 0);
					icon.AddFile(iconInfo.Filename, new QSize(size, size), QIcon.Mode.Normal, QIcon.State.On);
				}
			}
			return icon;
		}
		
		public static QIcon LoadIcon (string name, int size)
		{
			if (IconTheme.Default == null)
				throw new InvalidOperationException("No Default IconTheme");
			
			var iconInfo = IconTheme.Default.LookupIcon(name, size, 0);
			if (iconInfo != null) {
				return new QIcon(iconInfo.Filename);
			} else {
				Console.Error.WriteLine(String.Format("Icon not found: {0} ({1})", name, size));
				return new QIcon();
			}			
		}
	}
}
