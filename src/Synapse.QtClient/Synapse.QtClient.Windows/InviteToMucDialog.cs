//
// InviteToMucDialog.cs
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

using jabber;

using Qyoto;

using Synapse.Core;
using Synapse.UI.Chat;

namespace Synapse.QtClient.Windows
{
	public partial class InviteToMucDialog : QDialog
	{
		IChatHandler m_Handler;
		
		public InviteToMucDialog (IChatHandler handler, QWidget parentWindow) : base (parentWindow)
		{
			SetupUi();
		
			buttonBox.Button(QDialogButtonBox.StandardButton.Close).Hide();
			buttonBox.Button(QDialogButtonBox.StandardButton.Ok).Text = "&Invite";
			
			m_Handler = handler;
			
			if (handler is ChatHandler) {			
				foreach (var room in m_Handler.Account.ConferenceManager.Rooms) {
					mucsListWidget.AddItem(room.JID.ToString());					
				}
				stackedWidget.CurrentIndex = 0;
			} else {
				foreach (var jid in m_Handler.Account.Roster) {
					if (m_Handler.Account.PresenceManager.IsAvailable(jid)) {
						friendsListWidget.AddItem(jid.ToString());
					}
				}				
				stackedWidget.CurrentIndex = 1;
				urlLineEdit.Text = String.Format("http://chat.synapse.im/room/{0}/", Uri.EscapeUriString(((MucHandler)m_Handler).Room.JID.ToString()));
			}
			
			Gui.CenterWidgetOnScreen(this);
			
			on_mucsListWidget_itemSelectionChanged();
		}	
		
		public override void Accept ()
		{
			base.Accept ();
			
			if (m_Handler is ChatHandler) {
				foreach (var room in m_Handler.Account.ConferenceManager.Rooms) {
					if (room.JID.Bare == mucsListWidget.SelectedItems()[0].Text()) {
						room.Invite(((ChatHandler)m_Handler).Jid, String.Empty);
						return;
					}
				}
			} else {
				if (tabWidget.CurrentIndex == 0) {
					var jid = new JID(friendsListWidget.SelectedItems()[0].Text());
					((MucHandler)m_Handler).Room.Invite(jid, String.Empty);
				}
			}
		}
		
		[Q_SLOT]
		void on_mucsListWidget_itemSelectionChanged ()
		{
			buttonBox.Button(QDialogButtonBox.StandardButton.Ok).Enabled = (mucsListWidget.SelectedItems().Count > 0);
		}
		
		[Q_SLOT]
		void on_emailButton_clicked ()
		{
			// FIXME: Write up a better default email.
			string subject = Uri.EscapeUriString("Conference Invitation");
			string body    = Uri.EscapeUriString("Click to join: " + urlLineEdit.Text);
			Util.Open(String.Format("mailto:?subject={0}&body={1}", subject, body));
		}
		
		[Q_SLOT]
		void on_copyButton_clicked ()
		{
			var clipboard = QApplication.Clipboard();
			clipboard.SetText(urlLineEdit.Text);
		}
		
		[Q_SLOT]
		void on_tabWidget_currentChanged (int index)
		{
			if (index == 0) {
				buttonBox.Button(QDialogButtonBox.StandardButton.Ok).Show();
				buttonBox.Button(QDialogButtonBox.StandardButton.Cancel).Show();
				buttonBox.Button(QDialogButtonBox.StandardButton.Close).Hide();
			} else {
				buttonBox.Button(QDialogButtonBox.StandardButton.Ok).Hide();
				buttonBox.Button(QDialogButtonBox.StandardButton.Cancel).Hide();
				buttonBox.Button(QDialogButtonBox.StandardButton.Close).Show();
			}
		}
		
		[Q_SLOT]
		void on_friendsListWidget_itemSelectionChanged ()
		{
			buttonBox.Button(QDialogButtonBox.StandardButton.Ok).Enabled = (friendsListWidget.SelectedItems().Count > 0);
		}		
	}
}