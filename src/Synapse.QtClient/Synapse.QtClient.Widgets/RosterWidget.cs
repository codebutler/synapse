//
// RosterWidget.cs
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
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Qyoto;
using Synapse.Core;
using Synapse.Xmpp;
using Synapse.UI.Operations;
using Synapse.ServiceStack;
using Synapse.UI;
using Synapse.UI.Controllers;
using Synapse.QtClient;
using Synapse.QtClient.UI.Views;
using Synapse.QtClient.Widgets;
using jabber;
using jabber.connection;
using jabber.protocol.iq;

public partial class RosterWidget : QWidget
{
	MainWindow            m_ParentWindow;
	BookmarkedMUCsModel   m_MucModel;
	RosterAvatarGridModel m_RosterModel;
	QMenu                 m_RosterMenu;
	QMenu                 m_RosterItemMenu;
	QAction               m_ShowOfflineAction;
	QMenu                 m_InviteMenu;
	List<QAction>         m_InviteActions;
	QAction 			  m_ViewProfileAction;
	QAction               m_IMAction;
	QAction               m_ListModeAction;
	QAction               m_EditGroupsAction;

	public event EventHandler ActivityFeedReady;
	
	public RosterWidget (MainWindow parent) : base (parent)
	{
		SetupUi();

		rosterGrid.ContextMenuPolicy = Qt.ContextMenuPolicy.CustomContextMenu;
		m_RosterMenu = new QMenu(this);
		QObject.Connect(m_RosterMenu, Qt.SIGNAL("triggered(QAction*)"), this, Qt.SLOT("rosterMenu_triggered(QAction*)"));

		m_ShowOfflineAction = new QAction("Show Offline Friends", this);
		m_ShowOfflineAction.Checkable = true;
		m_RosterMenu.AddAction(m_ShowOfflineAction);

		m_ListModeAction = new QAction("List Mode", this);
		m_ListModeAction.Checkable = true;
		m_RosterMenu.AddAction(m_ListModeAction);

		m_InviteActions = new List<QAction>();
		
		m_InviteMenu = new QMenu(this);		
		m_InviteMenu.MenuAction().Text = "Invite To";
		m_InviteMenu.AddAction("New Conference...");

		m_RosterItemMenu = new QMenu(this);
		QObject.Connect(m_RosterItemMenu, Qt.SIGNAL("triggered(QAction*)"), this, Qt.SLOT("rosterItemMenu_triggered(QAction*)"));
		m_ViewProfileAction = new QAction("View Profile", m_RosterItemMenu);
		m_RosterItemMenu.AddAction(m_ViewProfileAction);
		
		m_IMAction = new QAction("IM", m_RosterItemMenu);
		m_RosterItemMenu.AddAction(m_IMAction);
		
		m_RosterItemMenu.AddAction("Send File...");
		m_RosterItemMenu.AddMenu(m_InviteMenu);
		m_RosterItemMenu.AddAction("View History");
		m_RosterItemMenu.AddSeparator();

		m_EditGroupsAction = new QAction("Edit Groups", m_RosterItemMenu);
		m_RosterItemMenu.AddAction(m_EditGroupsAction);
		
		m_RosterItemMenu.AddAction("Remove");

		m_RosterModel = new RosterAvatarGridModel();
		rosterGrid.Model = m_RosterModel;
		rosterGrid.ItemActivated += HandleItemActivated;
		
		QSizeGrip grip = new QSizeGrip(tabWidget);
		tabWidget.SetCornerWidget(grip, Qt.Corner.BottomRightCorner);

		tabWidget.ElideMode = Qt.TextElideMode.ElideMiddle;
		
		m_ActivityWebView.Page().linkDelegationPolicy = QWebPage.LinkDelegationPolicy.DelegateAllLinks;
		m_ActivityWebView.Page().MainFrame().Load("resource:/feed.html");
		QObject.Connect(m_ActivityWebView, Qt.SIGNAL("linkClicked(QUrl)"), this, Qt.SLOT("HandleActivityLinkClicked(QUrl)"));
		QObject.Connect(m_ActivityWebView.Page(), Qt.SIGNAL("loadFinished(bool)"), this, Qt.SLOT("activityPage_loadFinished(bool)"));
		
		m_ParentWindow = parent;

		friendMucListWebView.Page().MainFrame().Load("resource:/friend-muclist.html");

		quickJoinMucContainer.Hide();
		shoutContainer.Hide();

		QVBoxLayout layout = new QVBoxLayout(m_AccountsContainer);
		layout.Margin = 0;
		m_AccountsContainer.SetLayout(layout);

		m_MucModel = new BookmarkedMUCsModel();
		mucTree.SetModel(m_MucModel);

		rosterIconSizeSlider.Value = rosterGrid.IconSize;
	}

	public int AccountsCount {
		get {
			return m_AccountsContainer.Layout().Count();
		}
	}
	
	public void AddAccount(Account account)
	{
		AccountStatusWidget widget = new AccountStatusWidget(account, this, m_ParentWindow);
		m_AccountsContainer.Layout().AddWidget(widget);
		widget.Show();
	}

	public void RemoveAccount(Account account)
	{
		throw new NotImplementedException();
	}
	
	public void AddActivityFeedItem (Account account, IActivityFeedItem item)
	{
		string html = Util.EscapeJavascript(item.ToHtml());
		m_ActivityWebView.Page().MainFrame().EvaluateJavaScript(String.Format("{0}(\"{1}\")", "add_item", html));
	}
	
	void HandleItemActivated (AvatarGrid<AccountItemPair> grid, AccountItemPair pair)
	{
		// FIXME: Move to controller.
		Synapse.ServiceStack.ServiceManager.Get<Synapse.UI.Services.GuiService>().OpenChatWindow(pair.Account, pair.Item.JID);
	}
	
	#region Private Slots
	[Q_SLOT]
	void on_m_JoinChatButton_clicked()
	{
		Account selectedAccount = Gui.ShowAccountSelectMenu(m_JoinChatButton);
		if (selectedAccount != null) {
			JID jid = null;
			if (JID.TryParse(m_ChatNameEdit.Text, out jid)) {
				if (!String.IsNullOrEmpty(jid.User) && !String.IsNullOrEmpty(jid.Server)) {
					ServiceManager.Get<OperationService>().Start(new JoinMucOperation(selectedAccount, jid));
				} else {
					QMessageBox.Critical(null, "Synapse", "Invalid JID");
				}
				m_ChatNameEdit.Text = String.Empty;
			} else {
				QMessageBox.Critical(this.TopLevelWidget(), "Synapse Error", "Invalid conference room");
			}
		}
	}

	[Q_SLOT]
	void on_mucTree_activated(QModelIndex index)
	{
		if (index.IsValid()) {
			if (index.InternalPointer() is BookmarkConference) {
				Account account = (Account)index.Parent().InternalPointer();
				BookmarkConference conf = (BookmarkConference)index.InternalPointer();
				try {
					ServiceManager.Get<OperationService>().Start(new JoinMucOperation(account, conf.JID));
				} catch (UserException e) {
					QMessageBox.Critical(this.TopLevelWidget(), "Synapse", e.Message);
				}
			}
		}
	}

	[Q_SLOT]
	void on_rosterGrid_customContextMenuRequested(QPoint point)
	{
		if (rosterGrid.HoverItem != null) {
			m_InviteActions.ForEach(a => m_InviteMenu.RemoveAction(a));
			if (rosterGrid.HoverItem.Account.ConferenceManager.Count > 0) {
				m_InviteActions.Add(m_InviteMenu.AddSeparator());
				foreach (var conference in rosterGrid.HoverItem.Account.ConferenceManager.Rooms) {
					QAction action = m_InviteMenu.AddAction(conference.JID);
					m_InviteActions.Add(action);
				}
			}
			m_RosterItemMenu.Popup(rosterGrid.MapToGlobal(point));
		} else {
			m_ShowOfflineAction.Checked = m_RosterModel.ShowOffline;
			m_RosterMenu.Popup(rosterGrid.MapToGlobal(point));
		}
	}

	[Q_SLOT]
	void rosterItemMenu_triggered (QAction action)
	{
		// FIXME: Actions should be handled in the controller.
		
		// FIXME: Don't use HoverItem, store MousePressItem separately.
		if (rosterGrid.HoverItem == null)
			return;
		
		if (action == m_ViewProfileAction) {
			ServiceManager.Get<OperationService>().Start(new RequestVCardOperation(rosterGrid.HoverItem.Account, rosterGrid.HoverItem.Item.JID));
		} else if (action == m_IMAction) {
			Synapse.ServiceStack.ServiceManager.Get<Synapse.UI.Services.GuiService>().OpenChatWindow(rosterGrid.HoverItem.Account, rosterGrid.HoverItem.Item.JID);	
		} else if (m_InviteActions.Contains(action)) {
			// FIXME
			Console.WriteLine("Send Invitation!!");
		} else if (action == m_EditGroupsAction) {
			var c = new EditGroupsWindowController(rosterGrid.HoverItem.Account, rosterGrid.HoverItem.Item);
		}
	}
	
	[Q_SLOT]
	void rosterMenu_triggered (QAction action)
	{
		if (action == m_ShowOfflineAction) {
			m_RosterModel.ShowOffline = action.Checked;
		} else if (action == m_ListModeAction) {
			rosterGrid.ListMode = action.Checked;
		}
	}

	[Q_SLOT]
	void on_rosterIconSizeSlider_valueChanged (int value)
	{
		rosterGrid.IconSize = value;
	}

	[Q_SLOT]
	void on_friendSearchLineEdit_textChanged ()
	{
		m_RosterModel.TextFilter = friendSearchLineEdit.Text;
	}

	[Q_SLOT]
	void activityPage_loadFinished (bool ok)
	{
		if (ActivityFeedReady != null)
			ActivityFeedReady(this, EventArgs.Empty);
	}

	[Q_SLOT]
	void HandleActivityLinkClicked (QUrl url)
	{
		Uri uri = new Uri(url.ToString());
		JID jid = new JID(uri.AbsolutePath);
		var query = XmppUriQueryInfo.ParseQuery(uri.Query);
		switch (query.QueryType) {
		case "message":
			// FIXME: Should not ask which account to use, should use whichever account generated the event.
			var account = Gui.ShowAccountSelectMenu(this);
			if (account != null)
				Synapse.ServiceStack.ServiceManager.Get<Synapse.UI.Services.GuiService>().OpenChatWindow(account, jid);
			break;
		default:
			throw new NotSupportedException("Unsupported query type: " + query.QueryType);
		}
	}
	#endregion
}
