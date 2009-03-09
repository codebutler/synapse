//
// EditGroupsWidget.cs
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
//
using System;
using System.Collections.Generic;
using System.Linq;
using Synapse.Xmpp;
using Qyoto;

namespace Synapse.QtClient.Widgets
{
	public partial class EditGroupsWidget : QWidget
	{
		Account m_Account;
		
		public EditGroupsWidget (QWidget parent) : base (parent)
		{
			SetupUi();

			addButton.icon = Gui.LoadIcon("add", 16);
		}
	
		public Account Account {
			get {
				return m_Account;
			}
			set {
				m_Account = value;

				var allGroupNames = m_Account.Roster.Select(jid => m_Account.Roster[jid])
					.SelectMany(item => item.GetGroups())
					.Select(group => group.GroupName)
					.Distinct()
					.OrderBy(name => name)
					.ToArray();
				
				listWidget.Clear();
				foreach (var groupName in allGroupNames) {
					AddItem(groupName);
				}
			}
		}
	
		public string[] SelectedGroups {
			get {
				List<string> result = new List<string>();
				for (int x = 0; x < listWidget.Count; x++) {
					QListWidgetItem item = listWidget.Item(x);
					if (item.CheckState() == Qt.CheckState.Checked)
						result.Add(item.Text());
				}
				return result.ToArray();
			}
			set {
				List<string> allGroupNames = new List<string>();
				for (int x = 0; x < listWidget.Count; x++) {
					QListWidgetItem item = listWidget.Item(x);
					string groupName = item.Text();
					allGroupNames.Add(groupName);
					bool inGroup = value.Contains(groupName);
					item.SetCheckState(inGroup ? Qt.CheckState.Checked : Qt.CheckState.Unchecked);
				}

				foreach (string groupName in value) {
					if (!allGroupNames.Contains(groupName)) {
						AddItem(groupName);
					}
				}
			}
		}

		QListWidgetItem AddItem (string groupName)
		{			
			QListWidgetItem item = new QListWidgetItem(groupName, listWidget);
			item.SetFlags((uint)Qt.ItemFlag.ItemIsEnabled | (uint)Qt.ItemFlag.ItemIsUserCheckable);
			item.SetCheckState(Qt.CheckState.Unchecked);
			listWidget.AddItem(item);
			return item;
		}

		[Q_SLOT]
		void on_addButton_clicked ()
		{
			// FIXME: Check if already exists before adding.
			var item = AddItem(lineEdit.Text);
			item.SetCheckState(Qt.CheckState.Checked);
		}
	}
}