//
// RosterWidget.cs
// 
// Copyright (C) 2010 Eric Butler
//
// Authors:
//   Eric Butler <eric@codebutler.com>
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
using Gtk;
using Synapse.Xmpp;

namespace Synapse.GtkClient.Widgets
{
	public class RosterWidget : VBox
	{
		VBox m_AccountsBox;
		
		public RosterWidget ()
		{
			Visible = false;
			
			m_AccountsBox = new VBox();
			m_AccountsBox.BorderWidth = 6;
			base.PackStart(m_AccountsBox, false, false, 0);
			m_AccountsBox.Show();
		}
		
		public void AddAccount (Account account)
		{
			var widget = new AccountStatusWidget(account);
			m_AccountsBox.Add(widget);
			widget.Show();
		}
		
		public void RemoveAccount (Account account)
		{
			foreach (var widget in m_AccountsBox.Children) {
				if (widget is AccountStatusWidget) {
					var statusWidget = (AccountStatusWidget)widget;
					if (statusWidget.Account == account) {
						m_AccountsBox.Remove(statusWidget);
						break;
					}
				}
			}
		}
		
		public int AccountsCount {
			get { return m_AccountsBox.Children.Length; }
		}
	}
}

