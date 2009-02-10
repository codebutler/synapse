//
// EditProfileDialog.WebIdentitiesTab.cs
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

using Synapse.QtClient;
using Synapse.QtClient.Widgets;
using Synapse.QtClient.ExtensionNodes;

using Qyoto;
using Mono.Addins;

namespace Synapse.QtClient.Windows
{
	public partial class EditProfileDialog : QDialog
	{		
		void SetupWebIdentitiesTab ()
		{
			foreach (WebIdentityConfiguratorCodon node in AddinManager.GetExtensionNodes("/Synapse/QtClient/WebIdentityConfigurators")) {
				var widget = new WebIdentityConfiguratorWidget(m_Account, node);			
				
				QVBoxLayout layout = (QVBoxLayout)webIdentitiesContainer.Layout();
				layout.InsertWidget(0, widget, 0);
				
				widget.SetParent(webIdentitiesContainer);
			}
		}
	}
}
