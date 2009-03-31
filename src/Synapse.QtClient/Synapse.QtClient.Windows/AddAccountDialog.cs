//
// AddAccountDialog.cs
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

using Synapse.ServiceStack;
using Synapse.Xmpp.Services;

using Qyoto;
using jabber;

namespace Synapse.QtClient.Windows
{
	public partial class AddAccountDialog : QDialog
	{
		public AddAccountDialog ()
		{
			SetupUi();

			var forwardButton = new QPushButton(Gui.LoadIcon("forward"), "&Forward");
			QObject.Connect(forwardButton, Qt.SIGNAL("clicked()"), HandleForwardButtonClicked);
			buttonBox.AddButton(forwardButton, QDialogButtonBox.ButtonRole.AcceptRole);
				
			buttonBox_2.StandardButtons = (uint)QDialogButtonBox.StandardButton.Ok | (uint)QDialogButtonBox.StandardButton.Cancel;
			var addAccountButton = buttonBox_2.Button(QDialogButtonBox.StandardButton.Ok);
			QObject.Connect(addAccountButton, Qt.SIGNAL("clicked()"), HandleAddAccountButtonClicked);
			addAccountButton.Text = "&Add Account";
			
			Gui.CenterWidgetOnScreen(this);
		}
		
		void HandleForwardButtonClicked ()
		{
			stackedWidget.CurrentIndex ++;
		}
		
		void HandleAddAccountButtonClicked ()
		{
			bool error = false;
			JID jid = null;
			if (!JID.TryParse(jidLineEdit.Text, out jid)) {
				QMessageBox.Critical(this, "Problem saving account", "JID is invalid");
				error = true;
			} else if (String.IsNullOrEmpty(jid.User)) {
				QMessageBox.Critical(this, "Problem saving account", "JID must have a username");
				error = true;
			} else if (String.IsNullOrEmpty(jid.Server)) {
				QMessageBox.Critical(this, "Problem saving account", "JID must have a server");
				error = true;
			} else if (passwordLineEdit.Text.Trim() == String.Empty) {
				QMessageBox.Critical(this, "Problem saving account", "Password may not be blank");
				error = true;
			} else if (resourceComboBox.CurrentText.Trim() == String.Empty) {
				QMessageBox.Critical(this, "Problem saving account", "Resource may not be blank");
				error = true;
			}
			
			if (error) {
				return;
			}

			var accountInfo = new AccountInfo(jid.User, jid.Server, passwordLineEdit.Text, resourceComboBox.CurrentText);
			accountInfo.AutoConnect = autoConnectCheckBox.Checked;
			ServiceManager.Get<AccountService>().AddAccount(accountInfo);
			
			base.Accept();
		}
	}
}
