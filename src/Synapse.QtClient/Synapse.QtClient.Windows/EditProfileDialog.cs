//
// EditProfileDialog.cs
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

using Qyoto;

namespace Synapse.QtClient.Windows
{
	public partial class EditProfileDialog : QDialog
	{	
		Account m_Account;
		
		public EditProfileDialog (Account account, QWidget parent) : base (parent)
		{
			if (account == null)
				throw new ArgumentNullException("account");
			
			m_Account = account;
						
			// FIXME: This should not be needed here.
			// For some reason the group title isnt bold when this window has its parent set.
			this.SetStyleSheet(@"
				QGroupBox {
					font-weight: bold;
				}
			");
			
			SetupUi();
			buttonBox.StandardButtons = (uint)QDialogButtonBox.StandardButton.Ok |
			                            (uint)QDialogButtonBox.StandardButton.Cancel;
			
			SetupAvatarTab();
			SetupWebIdentitiesTab();
		}
		
		public void Show (int tab)
		{
			mainTabWidget.CurrentIndex = tab;
			Show();
		}
		
		public new void Show ()
		{
			base.Show();
			if (avatarTabWidget.Count > 0)
				((AvatarProviderTab)avatarTabWidget.CurrentWidget()).Update(avatarSearchLineEdit.Text);
		}
	}
}