//
// WebIdentityConfiguratorWidget.cs
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
using Synapse.QtClient;
using Synapse.QtClient.ExtensionNodes;

using Qyoto;

namespace Synapse.QtClient.Widgets
{	
	public partial class WebIdentityConfiguratorWidget : QWidget
	{
		WebIdentityConfiguratorCodon m_Node;
		IWebIdentityConfigurator     m_Configurator;
		
		public WebIdentityConfiguratorWidget (Account account, WebIdentityConfiguratorCodon node)
		{
			SetupUi();
			
			m_Node         = node;
			m_Configurator = node.CreateInstance(account);
			
			iconLabel.Pixmap = new QPixmap(node.IconUri);
			
			UpdateStatus();
		}
		
		void UpdateStatus ()
		{
			nameLabel.Text = String.Format("<b>{0}</b><br/>{1}", m_Node.Name, m_Node.Description);
			if (m_Configurator.IsConfigured)
				nameLabel.Text += String.Format("<br/><br/><span style=\"color: green;\">{0} is configured!</span>", m_Node.Name);
			else
				nameLabel.Text += String.Format("<br/><br/><span style=\"color: red;\">{0} is not configured.</span>", m_Node.Name);
		}
		
		[Q_SLOT]
		void on_configureButton_clicked ()
		{
			m_Configurator.ShowConfigurationDialog(base.TopLevelWidget());
			UpdateStatus();
		}
	}
}