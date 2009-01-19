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
using Qyoto;
using Synapse.ServiceStack;
using Synapse.UI.Services;
using Synapse.UI.Controllers;
using Synapse.UI.Views;
using Synapse.QtClient.UI.Views;

public partial class EditGroupsWindow : QWidget, IEditGroupsWindowView
{	
	public EditGroupsWindow (EditGroupsWindowController controller)
	{
		SetupUi();

		buttonBox.StandardButtons = (uint)QDialogButtonBox.StandardButton.Ok | (uint)QDialogButtonBox.StandardButton.Cancel;

		groupsWidget.Account = controller.Account;
		groupsWidget.SelectedGroups = controller.GroupNames;
	}

	public new void Show ()
	{
		var gui = ServiceManager.Get<GuiService>();
		((MainWindow)gui.MainWindow.View).ShowLightbox(this);
	}

	[Q_SLOT]
	void on_buttonBox_clicked (QAbstractButton button)
	{
		var role = buttonBox.buttonRole(button);
		var gui = ServiceManager.Get<GuiService>();
		if (role == QDialogButtonBox.ButtonRole.RejectRole)
			((MainWindow)gui.MainWindow.View).HideLightbox();
		else
			Console.WriteLine("Other!");
	}
}
