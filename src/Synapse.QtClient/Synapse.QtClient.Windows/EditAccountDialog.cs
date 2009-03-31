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
		AccountInfo m_AccountInfo;
		
		public EditAccountDialog (AccountInfo accountInfo, QWidget parentWindow) : base (parentWindow)
		{
			SetupUi();
			
			m_AccountInfo = accountInfo;
			
			jidLineEdit.Text = String.Format("{0}@{1}", accountInfo.User, accountInfo.Domain);
			passwordLineEdit.Text = accountInfo.Password;
			resourceCombo.SetEditText(accountInfo.Resource);
			
			serverLineEdit.Text = accountInfo.ConnectServer;
			portSpinBox.Value = accountInfo.ConnectPort;
			
			autoConnectCheckBox.Checked = accountInfo.AutoConnect;
			
			switch (m_AccountInfo.ProxyType) {
			case ProxyType.System:
				comboBox.CurrentIndex = 0;
				ShowProxyWidgets(false);
				break;
			case ProxyType.None:
				comboBox.CurrentIndex = 1;
				ShowProxyWidgets(false);
				break;
			case ProxyType.HTTP:
				comboBox.CurrentIndex = 2;
				ShowProxyWidgets(true);
				break;
			case ProxyType.SOCKS4:
				comboBox.CurrentIndex = 3;
				ShowProxyWidgets(true);
				break;
			case ProxyType.SOCKS5:
				comboBox.CurrentIndex = 4;
				ShowProxyWidgets(true);
				break;
			}
			proxyHostLineEdit.Text = m_AccountInfo.ProxyHost;
			proxyPortSpinBox.Value = m_AccountInfo.ProxyPort;
			proxyUserLineEdit.Text = m_AccountInfo.ProxyUsername;
			proxyPassLineEdit.Text = m_AccountInfo.ProxyPassword;
			
			this.buttonBox.StandardButtons = (uint)QDialogButtonBox.StandardButton.Ok |
			                                 (uint)QDialogButtonBox.StandardButton.Cancel;
			
			Gui.CenterWidgetOnScreen(this);
		}
		
		public override void Accept ()
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
			} else if (resourceCombo.CurrentText.Trim() == String.Empty) {
				QMessageBox.Critical(this, "Problem saving account", "Resource may not be blank");
				error = true;
			}
			
			if (error) {
				return;
			}
			
			m_AccountInfo.User = jid.User;
			m_AccountInfo.Domain = jid.Server;
			m_AccountInfo.Password = passwordLineEdit.Text;
			m_AccountInfo.Resource = resourceCombo.CurrentText;
			
			m_AccountInfo.ConnectServer = serverLineEdit.Text;
			m_AccountInfo.ConnectPort = portSpinBox.Value;
			
			m_AccountInfo.AutoConnect = autoConnectCheckBox.Checked;
			
			switch (comboBox.CurrentIndex) {
			case 0:
				m_AccountInfo.ProxyType = ProxyType.System;
				break;
			case 1:
				m_AccountInfo.ProxyType = ProxyType.None;
				break;
			case 2:
				m_AccountInfo.ProxyType = ProxyType.HTTP;
				break;
			case 3:
				m_AccountInfo.ProxyType = ProxyType.SOCKS4;
				break;
			case 4:
				m_AccountInfo.ProxyType = ProxyType.SOCKS5;
				break;
			}
			m_AccountInfo.ProxyHost = proxyHostLineEdit.Text;
			m_AccountInfo.ProxyPort = proxyPortSpinBox.Value;
			m_AccountInfo.ProxyUsername = proxyUserLineEdit.Text;
			m_AccountInfo.ProxyPassword = proxyPassLineEdit.Text;
			
			ServiceManager.Get<AccountService>().SaveAccounts();
		
			base.Accept ();
		}
		
		[Q_SLOT]
		void on_comboBox_activated (int index)
		{
			/* 
			   0: Use System Settings
	           1: No Proxy
	           2: HTTP
	           3: SOCKS4
	           4: SOCKS5
	        */
			
			switch (index) {
			case 0:
			case 1:
				ShowProxyWidgets(false);
				break;
			default:
				ShowProxyWidgets(true);
				break;
			}
			
			switch (index) {
			case 2:
				proxyPortSpinBox.Value = 8080;
				break;
			case 3:
			case 4:
				proxyPortSpinBox.Value = 1080;
				break;
			}
		}
		
		void ShowProxyWidgets (bool show)
		{	
			line.SetVisible(show);
			proxyHostLabel.SetVisible(show);
			proxyHostLineEdit.SetVisible(show);
			proxyPortLabel.SetVisible(show);
			proxyPortSpinBox.SetVisible(show);
			proxyUserLabel.SetVisible(show);
			proxyUserLineEdit.SetVisible(show);
			proxyPassLabel.SetVisible(show);
			proxyPassLineEdit.SetVisible(show);
		}
	}
}