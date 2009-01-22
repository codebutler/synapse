//
// AddFriend.cs
// 
// Copyright (C) 2009 Eric Butler
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
using Synapse.Xmpp;
using Synapse.ServiceStack;
using Synapse.UI.Services;
using Synapse.QtClient.UI.Views;
using Qyoto;
using jabber;
using jabber.protocol.client;
using jabber.protocol.iq;

public partial class AddFriendWindow : QWidget
{
	Account m_Account;
	
	public AddFriendWindow (Account account)
	{
		SetupUi();

		buttonBox.StandardButtons = (uint)QDialogButtonBox.StandardButton.Ok | (uint)QDialogButtonBox.StandardButton.Cancel;

		m_Account = account;
		
		groupsWidget.Account = account;
	}

	public new void Show ()
	{
		GuiService gui = ServiceManager.Get<GuiService>();
		((MainWindow)gui.MainWindow.View).ShowLightbox(this);
	}
	
	[Q_SLOT]
	void on_buttonBox_clicked (QAbstractButton button)
	{
		try {
			var role = buttonBox.buttonRole(button);
			if (role == QDialogButtonBox.ButtonRole.AcceptRole) {
				JID jid = new JID(lineEdit.Text);

				// FIXME: Start spinner
				
				m_Account.AddRosterItem(jid, groupsWidget.SelectedGroups, AddRosterItemComplete);
			} else {
				Application.Invoke(delegate {
					var gui = ServiceManager.Get<GuiService>();
					((MainWindow)gui.MainWindow.View).HideLightbox();
				});
			}			
		} catch (Exception ex) {
			Application.Invoke(delegate {
				QMessageBox.Critical(base.TopLevelWidget(), "Failed to add user", ex.Message);
			});
		}
	}

	void AddRosterItemComplete (object sender, IQ response, object data)
	{
		var gui = ServiceManager.Get<GuiService>();
		if (response.Type == IQType.set) {
			Application.Invoke(delegate {
				((MainWindow)gui.MainWindow.View).HideLightbox();
			});
		} else {
			Application.Invoke(delegate {
				QMessageBox.Critical(base.TopLevelWidget(), "Failed to add user", "Server returned an error.");
			});
		}
	}
}