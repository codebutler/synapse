//
// EditAccountDialog.cs
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

using Qyoto;
using jabber;

using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;

namespace Synapse.QtClient.Windows
{
	public partial class EditAccountDialog : QDialog
	{
		Account m_Account;
		
		public EditAccountDialog (Account account, QWidget parentWindow) : base (parentWindow)
		{
			SetupUi();
			
			QObject.Connect(this, Qt.SIGNAL("accepted()"), this, Qt.SLOT("HandleDialogAccepted()"));
			
			m_Account = account;
			
			jidLineEdit.Text = account.Jid.Bare;
			passwordLineEdit.Text = account.Password;
			resourceCombo.SetEditText(account.Resource);
			
			serverLineEdit.Text = account.ConnectServer;
			portSpinBox.Value = account.ConnectPort;
			
			autoConnectCheckBox.Checked = account.AutoConnect;
			
			this.buttonBox.StandardButtons = (uint)QDialogButtonBox.StandardButton.Ok |
			                                 (uint)QDialogButtonBox.StandardButton.Cancel;
			
			Gui.CenterWidgetOnScreen(this);
		}
		
		[Q_SLOT]
		void HandleDialogAccepted ()
		{
			JID jid = null;
			if (!JID.TryParse(jidLineEdit.Text, out jid))
				QMessageBox.Critical(this, "Problem saving account", "JID is invalid");
			else if (String.IsNullOrEmpty(jid.User))
				QMessageBox.Critical(this, "Problem saving account", "JID must have a username");
			else if (String.IsNullOrEmpty(jid.Server))
				QMessageBox.Critical(this, "Problem saving account", "JID must have a server");
			else if (resourceCombo.CurrentText.Trim() == String.Empty)
				QMessageBox.Critical(this, "Problem saving account", "Password may not be blank");
			else if (resourceCombo.CurrentText.Trim() == String.Empty)
				QMessageBox.Critical(this, "Problem saving account", "Resource may not be blank");
			
			m_Account.User = jid.User;
			m_Account.Domain = jid.Server;
			m_Account.Password = passwordLineEdit.Text;
			m_Account.Resource = resourceCombo.CurrentText;
			
			m_Account.ConnectServer = serverLineEdit.Text;
			m_Account.ConnectPort = portSpinBox.Value;
			
			m_Account.AutoConnect = autoConnectCheckBox.Checked;
			
			ServiceManager.Get<AccountService>().SaveAccounts();
		}
	}
}