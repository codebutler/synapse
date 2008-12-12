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
using Synapse.UI.Views;
using Synapse.QtClient.UI.Views;

public partial class AccountStatusWidget : QWidget
{
	QMenu      m_PresenceMenu;
	Account    m_Account;
	MainWindow m_ParentWindow;

	QAction m_AvailableAction;
	QAction m_FreeToChatAction;
	QAction m_AwayAction;
	QAction m_ExtendedAwayAction;
	QAction m_DoNotDisturbAction;
	QAction m_OfflineAction;
	
	public AccountStatusWidget(Account account, RosterWidget parent, MainWindow parentWindow) : base (parent)
	{
		SetupUi();
		
		m_ParentWindow = parentWindow;

		QPixmap pixmap = new QPixmap("resource:/default-avatar.png");
		m_AvatarLabel.Pixmap = pixmap;
			
		m_Account = account;
		m_Account.ConnectionStateChanged += OnAccountStateChanged;
		m_Account.StatusChanged += OnAccountStateChanged;
		OnAccountStateChanged(account);
		
		m_NameLabel.Text = account.Jid.Bare;

		m_PresenceMenu = new QMenu(this);
		QObject.Connect(m_PresenceMenu, Qt.SIGNAL("aboutToShow()"), this, Qt.SLOT("presenceMenu_aboutToShow()"));
		QObject.Connect(m_PresenceMenu, Qt.SIGNAL("triggered(QAction*)"), this, Qt.SLOT("OnPresenceSelected(QAction*)"));

		QActionGroup group = new QActionGroup(this);
		group.Exclusive = true;
		
		m_AvailableAction = m_PresenceMenu.AddAction("Available");
		group.AddAction(m_AvailableAction);
		m_AvailableAction.Checkable = true;
		
		m_FreeToChatAction = m_PresenceMenu.AddAction("Free to Chat");
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
		Application.Invoke(delegate {
			string text = null;
			string statusText = null;
			if (account.Status != null) {
				text = account.Status.Type.ToString();
				if (!String.IsNullOrEmpty(account.Status.StatusText))
					statusText = account.Status.StatusText;
			} else {
				text = account.ConnectionState.ToString();
			}
	
			StringBuilder statusLabelBuilder = new StringBuilder();
			
			statusLabelBuilder.Append(@"<html><style>a { color: white; }</style><body>");
			
			if (statusText == null)
				statusLabelBuilder.Append(String.Format("<a href=\"#show-presence-menu\">{0}</a>", text));
			else
				statusLabelBuilder.Append(String.Format("<a href=\"#show-presence-menu\">{0}</a> - {1}", text, statusText));
	
			statusLabelBuilder.Append("</body></html>");
	
			m_StatusLabel.Text = statusLabelBuilder.ToString();
		});
	}

	[Q_SLOT]
	private void on_m_StatusLabel_linkActivated(string link)
	{
		switch (link) {
		case "#show-presence-menu":
			m_PresenceMenu.Popup(m_StatusLabel.MapToGlobal(m_StatusLabel.Rect.BottomLeft()));
			break;
		}
	}

	[Q_SLOT]
	private void OnPresenceSelected(QAction action)
	{
		m_ParentWindow.RaisePresenceChanged(m_Account, action.Text, String.Empty);
	}

	[Q_SLOT]
	private void presenceMenu_aboutToShow()
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
