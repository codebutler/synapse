//
// ActionHandler.cs
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

using Synapse.UI;
using Synapse.QtClient;
using Synapse.QtClient.Windows;

using Qyoto;

namespace Synapse.Addins.CodeSnippets
{
	public class InsertSnippetAction : QAction
	{
		ChatWindow m_ChatWindow;
		
		public InsertSnippetAction (QWidget parent) : base (parent)
		{
			m_ChatWindow = (ChatWindow)parent;
			
			QObject.Connect(this, Qt.SIGNAL("triggered(bool)"), this, Qt.SLOT("on_triggered(bool)"));
			
			base.Text = "Code Snippet...";
			base.icon = Gui.LoadIcon("stock_script", 16);
		}
				
		[Q_SLOT]
		void on_triggered (bool isChecked)
		{
			var dialog = new InsertSnippetDialog(m_ChatWindow);
			dialog.Show();
		}
	}
}
