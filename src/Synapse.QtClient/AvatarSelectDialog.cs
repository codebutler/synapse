//
// AvatarSelectDialog.cs
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
using Synapse.Xmpp;
using Synapse.UI;
using Mono.Addins;

public partial class AvatarSelectDialog : QDialog
{
	Account m_Account;
	
	public AvatarSelectDialog (Account account)
	{
		SetupUi();

		m_Account = account;

		if (account.VCard != null && (!String.IsNullOrEmpty(account.VCard.Nickname) || !String.IsNullOrEmpty(account.VCard.FullName)))
			lineEdit.Text = !String.IsNullOrEmpty(account.VCard.Nickname) ? account.VCard.Nickname : account.VCard.FullName;
		else
			lineEdit.Text = account.Jid.User;

		foreach (var node in AddinManager.GetExtensionNodes("/Synapse/UI/AvatarProviders")) {
			IAvatarProvider provider = (IAvatarProvider)((TypeExtensionNode)node).CreateInstance();
			var tab = new AvatarProviderTab(provider, this);
			tabWidget.AddTab(tab, provider.Name);
			tab.Show();
		}
	}

	class AvatarProviderTab : QWebView
	{
		IAvatarProvider m_Provider;
			
		public AvatarProviderTab (IAvatarProvider provider, QWidget parent) : base (parent)
		{
			base.SetHtml(String.Empty);
		}

		public IAvatarProvider Provider {
			get {
				return m_Provider;
			}
		}
	}
}
