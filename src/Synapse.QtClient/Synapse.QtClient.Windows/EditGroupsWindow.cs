//
// EditGroupsWindow.cs
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
using Qyoto;
using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.UI.Services;
using jabber.protocol.iq;
using Synapse.QtClient;

namespace Synapse.QtClient.Windows
{
	public partial class EditGroupsWindow : QWidget
	{
		Account m_Account;
		Item    m_Item;
		
		public EditGroupsWindow (Account account, Item item)
		{
			SetupUi();
	
			m_Account = account;
			m_Item    = item;
	
			buttonBox.StandardButtons = (uint)QDialogButtonBox.StandardButton.Ok | (uint)QDialogButtonBox.StandardButton.Cancel;
	
			jidLabel.Text = item.JID.ToString();
			
			groupsWidget.Account = account;
			groupsWidget.SelectedGroups = item.GetGroups().Select(g => g.GroupName).ToArray();
		}
	
		public new void Show ()
		{
			Gui.MainWindow.ShowLightbox(this);
		}
	
		[Q_SLOT]
		void on_buttonBox_clicked (QAbstractButton button)
		{
			var role = buttonBox.buttonRole(button);
			if (role == QDialogButtonBox.ButtonRole.AcceptRole) {
				var currentGroups = m_Item.GetGroups().Select(g => g.GroupName);
				var newGroups = groupsWidget.SelectedGroups;
	
				foreach (string groupName in currentGroups) {
					if (!newGroups.Contains(groupName)) {
						m_Item.RemoveGroup(groupName);
					}
				}
	
				foreach (String groupName in newGroups) {
					if (!currentGroups.Contains(groupName)) {
						m_Item.AddGroup(groupName);
					}
				}
					
				m_Account.Roster.Modify(m_Item);
			}
			Gui.MainWindow.HideLightbox();
		}
	}
}