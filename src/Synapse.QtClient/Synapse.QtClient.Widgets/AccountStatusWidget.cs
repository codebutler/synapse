//
// AccountStatusWidget.cs
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

using Synapse.ServiceStack;
using System;
using System.Text;
using Qyoto;
using Synapse.Xmpp;
using Synapse.UI;
using Synapse.QtClient;
using jabber;
using Synapse.QtClient.Windows;

namespace Synapse.QtClient.Widgets
{
	public partial class AccountStatusWidget : QWidget
	{
		Account    m_Account;
		MainWindow m_ParentWindow;
	
		QMenu   m_PresenceMenu;
		QAction m_AvailableAction;
		QAction m_FreeToChatAction;
		QAction m_AwayAction;
		QAction m_ExtendedAwayAction;
		QAction m_DoNotDisturbAction;
		QAction m_OfflineAction;
	
		EditProfileDialog m_EditProfileDialog;
		
		public AccountStatusWidget(Account account, RosterWidget parent, MainWindow parentWindow) : base (parent)
		{
			SetupUi();
			
			m_ParentWindow = parentWindow;
	
			m_EditProfileDialog = new EditProfileDialog(account, this.TopLevelWidget());
			
			m_AvatarLabel.Cursor = new QCursor(CursorShape.PointingHandCursor);
			m_AvatarLabel.Clicked += delegate {
				if (m_Account.ConnectionState == AccountConnectionState.Connected) {
					m_EditProfileDialog.Show(2);
					m_EditProfileDialog.ActivateWindow();
				} else {
					// FIXME: It really wouldn't be so hard to make this work. 
					// On connect, check to see if it was changed and update server.
					QMessageBox.Warning(this.TopLevelWidget(), "Synapse", "Cannot edit avatar when you're not connected.");
				}
			};
				
			m_Account = account;
			m_Account.ConnectionStateChanged += OnAccountStateChanged;
			m_Account.StatusChanged += OnAccountStateChanged;
			m_Account.MyVCardUpdated += HandleMyVCardUpdated;
			m_Account.AvatarManager.AvatarUpdated += HandleAvatarUpdated;
			OnAccountStateChanged(account);
	
			HandleAvatarUpdated(m_Account.Jid.Bare, null);
			
			HandleMyVCardUpdated(null, EventArgs.Empty);
			m_NameLabel.TextFormat = TextFormat.RichText;
	
			m_PresenceMenu = new QMenu(this);
			QObject.Connect(m_PresenceMenu, Qt.SIGNAL("aboutToShow()"), HandlePresenceMenuAboutToShow);
			QObject.Connect<QAction>(m_PresenceMenu, Qt.SIGNAL("triggered(QAction*)"), HandlePresenceMenuTriggered);
	
			QActionGroup group = new QActionGroup(this);
			group.Exclusive = true;
			
			m_AvailableAction = m_PresenceMenu.AddAction("Available");
			group.AddAction(m_AvailableAction);
			m_AvailableAction.Checkable = true;
			
			m_FreeToChatAction = m_PresenceMenu.AddAction("Free To Chat");
			group.AddAction(m_FreeToChatAction);
			m_FreeToChatAction.Checkable = true;
			
			m_AwayAction = m_PresenceMenu.AddAction("Away");
			group.AddAction(m_AwayAction);
			m_AwayAction.Checkable = true;
			
			m_ExtendedAwayAction = m_PresenceMenu.AddAction("Extended Away");
			group.AddAction(m_ExtendedAwayAction);
			m_ExtendedAwayAction.Checkable = true;
			
			m_DoNotDisturbAction = m_PresenceMenu.AddAction("Do Not Disturb");
			group.AddAction(m_DoNotDisturbAction);
			m_DoNotDisturbAction.Checkable = true;
			
			m_PresenceMenu.AddSeparator();
			
			m_OfflineAction = m_PresenceMenu.AddAction("Offline");
			group.AddAction(m_OfflineAction);
			m_OfflineAction.Checkable = true;
		}
		
		void OnAccountStateChanged (Account account)
		{
			QApplication.Invoke(delegate {
				string text = null;
				string statusText = null;
				if (account.Status != null) {
					text = account.Status.TypeDisplayName;
					if (!String.IsNullOrEmpty(account.Status.StatusText)) {
						statusText = account.Status.StatusText;
					}
				} else {
					text = account.ConnectionState.ToString();
				}
		
				StringBuilder statusLabelBuilder = new StringBuilder();
	
				// FIXME: Read font size / color from theme:
				statusLabelBuilder.Append(@"<html><style>a { font-size: 9pt; color: white; }</style><body>");
				
				if (statusText == null)
					statusLabelBuilder.Append(String.Format("<a href=\"#show-presence-menu\">{0}</a>", text));
				else
					statusLabelBuilder.Append(String.Format("<a href=\"#show-presence-menu\">{0}</a> - {1}", text, statusText));
		
				statusLabelBuilder.Append("</body></html>");
	
				m_StatusLabel.Text = statusLabelBuilder.ToString();
			});
		}
	
		void HandleAvatarUpdated (string jid, string hash)
		{
			if (jid == m_Account.Jid.Bare) {				
				QApplication.Invoke(delegate {		
					QPixmap pixmap = new QPixmap(36, 36);
					pixmap.Fill(GlobalColor.transparent);
			
					QPainter painter = new QPainter(pixmap);
					Gui.DrawAvatar(painter, m_AvatarLabel.Width(), m_AvatarLabel.Height(), (QPixmap)AvatarManager.GetAvatar(hash));
					painter.Dispose();
	
					m_AvatarLabel.Pixmap = pixmap;
				});
			}
		}
		
		void HandleMyVCardUpdated (object sender, EventArgs args)
		{
			QApplication.Invoke(delegate {
				if (m_Account.VCard != null && !String.IsNullOrEmpty(m_Account.VCard.Nickname))
					m_NameLabel.Text = m_Account.VCard.Nickname;
				else
					m_NameLabel.Text = m_Account.Jid.Bare;
			});
		}
			
		[Q_SLOT]
		void on_m_StatusLabel_linkActivated(string link)
		{
			switch (link) {
			case "#show-presence-menu":
				m_PresenceMenu.Popup(m_StatusLabel.MapToGlobal(m_StatusLabel.Rect.BottomLeft()));
				break;
			}
		}
	
		void HandlePresenceMenuTriggered(QAction action)
		{
			m_Account.Status = new ClientStatus(action.Text, String.Empty);
		}
	
		void HandlePresenceMenuAboutToShow()
		{
			if (m_Account.Status != null) {
				var currentStatus = m_Account.Status.Type;
				switch (currentStatus) {
				case ClientStatusType.Available:
					m_AvailableAction.Checked = true;
					break;
				case ClientStatusType.FreeToChat:
					m_FreeToChatAction.Checked = true;
					break;
				case ClientStatusType.Away:
					m_AwayAction.Checkable = true;
					break;
				case ClientStatusType.ExtendedAway:
					m_ExtendedAwayAction.Checked = true;
					break;
				case ClientStatusType.DoNotDisturb:
					m_DoNotDisturbAction.Checked = true;
					break;
				case ClientStatusType.Offline:
					m_OfflineAction.Checked = true;
					break;
				}
			} else {
				m_OfflineAction.Checked = true;
			}
		}
	}
}
