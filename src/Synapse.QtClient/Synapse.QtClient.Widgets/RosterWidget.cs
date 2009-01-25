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
using System.Linq;
using Qyoto;
using Synapse.Core;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;
using Synapse.ServiceStack;
using Synapse.UI;
using Synapse.UI.Controllers;
using Synapse.UI.Services;
using Synapse.QtClient;
using Synapse.QtClient.UI.Views;
using Synapse.QtClient.Widgets;
using jabber;
using jabber.connection;
using jabber.protocol.client;
using jabber.protocol.iq;
using Mono.Rocks;

public partial class RosterWidget : QWidget
{
	BookmarkedMUCsModel   m_MucModel;
	RosterAvatarGridModel m_RosterModel;
	QMenu                 m_RosterMenu;
	QMenu                 m_RosterItemMenu;
	QAction               m_ShowOfflineAction;
	QMenu                 m_InviteMenu;
	List<QAction>         m_InviteActions;
	QAction 			  m_ViewProfileAction;
	QAction               m_IMAction;
	QAction               m_GridModeAction;
	QAction               m_ListModeAction;
	QAction               m_ShowTransportsAction;
	QAction               m_EditGroupsAction;
	QAction               m_RemoveAction;
	RosterItem            m_MenuDownItem;

	// Map the JS element ID to the ActivityFeedItem
	Dictionary<string, IActivityFeedItem> m_ActivityFeedItems;
	
	public RosterWidget (QWidget parent) : base (parent)
	{
		SetupUi();
		
		m_RosterModel = new RosterAvatarGridModel();
		rosterGrid.Model = m_RosterModel;
		rosterGrid.ItemActivated += HandleItemActivated;
		rosterGrid.ShowGroupCounts = true;
		rosterGrid.InstallEventFilter(new KeyPressEater(delegate (QKeyEvent evnt) {
			if (!String.IsNullOrEmpty(evnt.Text())) {
				rosterSearchButton.Checked = true;
				friendSearchLineEdit.Text += evnt.Text();
				friendSearchLineEdit.SetFocus();
				return true;
			}
			return false;
		}, this));

		var accountService = ServiceManager.Get<AccountService>();
		accountService.AccountAdded += HandleAccountAdded;
		accountService.AccountRemoved += HandleAccountRemoved;
		foreach (Account account in accountService.Accounts) {
			HandleAccountAdded(account);
		}
		
		m_ActivityFeedItems = new Dictionary<string, IActivityFeedItem>();

		rosterGrid.ContextMenuPolicy = Qt.ContextMenuPolicy.CustomContextMenu;
		
		m_RosterMenu = new QMenu(this);
		QObject.Connect(m_RosterMenu, Qt.SIGNAL("triggered(QAction*)"), this, Qt.SLOT("rosterMenu_triggered(QAction*)"));

		var rosterViewActionGroup = new QActionGroup(this);
		QObject.Connect(rosterViewActionGroup, Qt.SIGNAL("triggered(QAction *)"), this, Qt.SLOT("rosterViewActionGroup_triggered(QAction*)"));

		m_GridModeAction = new QAction("View as Grid", this);
		m_GridModeAction.SetActionGroup(rosterViewActionGroup);
		m_GridModeAction.Checkable = true;
		m_GridModeAction.Checked = true;
		m_RosterMenu.AddAction(m_GridModeAction);

		m_ListModeAction = new QAction("View as List", this);
		m_ListModeAction.SetActionGroup(rosterViewActionGroup);
		m_ListModeAction.Checkable = true;
		m_RosterMenu.AddAction(m_ListModeAction);

		m_RosterMenu.AddSeparator();
		
		m_ShowOfflineAction = new QAction("Show Offline Friends", this);
		m_ShowOfflineAction.Checkable = true;
		m_RosterMenu.AddAction(m_ShowOfflineAction);
		
		m_ShowTransportsAction = new QAction("Show Transports", this);
		m_ShowTransportsAction.Checkable = true;
		m_RosterMenu.AddAction(m_ShowTransportsAction);

		m_RosterMenu.AddSeparator();

		var sliderAction = new QWidgetAction(this);
		
		var sliderContainer = new QWidget(this);
		sliderContainer.SetLayout(new QHBoxLayout());
		sliderContainer.Layout().AddWidget(new QLabel("Zoom:", sliderContainer));
		var zoomSlider = new QSlider(Orientation.Horizontal, sliderContainer);
		zoomSlider.Minimum = 16;
		zoomSlider.Maximum = 60;
		zoomSlider.Value = rosterGrid.IconSize;
		QObject.Connect(zoomSlider, Qt.SIGNAL("valueChanged(int)"), this, Qt.SLOT("zoomSlider_valueChanged(int)"));
		sliderContainer.Layout().AddWidget(zoomSlider);
		sliderAction.SetDefaultWidget(sliderContainer);
		m_RosterMenu.AddAction(sliderAction);

		m_InviteActions = new List<QAction>();
		
		m_InviteMenu = new QMenu(this);		
		m_InviteMenu.MenuAction().Text = "Invite To";
		m_InviteMenu.AddAction("New Conference...");

		m_RosterItemMenu = new QMenu(this);
		QObject.Connect(m_RosterItemMenu, Qt.SIGNAL("triggered(QAction*)"), this, Qt.SLOT("rosterItemMenu_triggered(QAction*)"));
		QObject.Connect(m_RosterItemMenu, Qt.SIGNAL("aboutToShow()"), this, Qt.SLOT("rosterItemMenu_aboutToShow()"));
		QObject.Connect(m_RosterItemMenu, Qt.SIGNAL("aboutToHide()"), this, Qt.SLOT("rosterItemMenu_aboutToHide()"));

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

		m_RemoveAction = new QAction("Remove", m_RosterItemMenu);
		m_RosterItemMenu.AddAction(m_RemoveAction);
		
		friendSearchLineEdit.InstallEventFilter(new KeyPressEater(delegate (QKeyEvent evnt) {
			if (evnt.Key() == (int)Key.Key_Escape) {
				friendSearchLineEdit.Clear();
				rosterSearchButton.Checked = false;
				rosterGrid.SetFocus();
				return true;
			}
			return false;
		}, this));
		
		QSizeGrip grip = new QSizeGrip(tabWidget);
		tabWidget.SetCornerWidget(grip, Qt.Corner.BottomRightCorner);

		tabWidget.ElideMode = Qt.TextElideMode.ElideMiddle;
	
		0.UpTo(9).ForEach(num => {
			QAction action = new QAction(this);
			action.Shortcut = new QKeySequence("Alt+" + num.ToString());
			QObject.Connect(action, Qt.SIGNAL("triggered(bool)"), delegate {
				tabWidget.CurrentIndex = num - 1;
			});
			this.AddAction(action);
		});

		var jsWindowObject = new JSDebugObject(this);
		m_ActivityWebView.Page().linkDelegationPolicy = QWebPage.LinkDelegationPolicy.DelegateAllLinks;
		QObject.Connect(m_ActivityWebView, Qt.SIGNAL("linkClicked(QUrl)"), this, Qt.SLOT("HandleActivityLinkClicked(QUrl)"));
		QObject.Connect(m_ActivityWebView.Page(), Qt.SIGNAL("loadFinished(bool)"), this, Qt.SLOT("activityPage_loadFinished(bool)"));
		QObject.Connect(m_ActivityWebView.Page().MainFrame(), Qt.SIGNAL("javaScriptWindowObjectCleared()"), delegate {
			m_ActivityWebView.Page().MainFrame().AddToJavaScriptWindowObject("Synapse", jsWindowObject);
		});
		m_ActivityWebView.Page().MainFrame().Load("resource:/feed.html");
		
		friendMucListWebView.Page().MainFrame().Load("resource:/friend-muclist.html");

		quickJoinMucContainer.Hide();
		shoutContainer.Hide();

		QObject.Connect(shoutLineEdit, Qt.SIGNAL("textChanged(const QString &)"), delegate {
			shoutCharsLabel.Text = (140 - shoutLineEdit.Text.Length).ToString();
		});
		
		QObject.Connect(shoutLineEdit, Qt.SIGNAL("returnPressed()"), delegate {
			var service = ServiceManager.Get<ActivityFeedService>();
			service.Shout(shoutLineEdit.Text);
			shoutLineEdit.Clear();
		});

		QVBoxLayout layout = new QVBoxLayout(m_AccountsContainer);
		layout.Margin = 0;
		m_AccountsContainer.SetLayout(layout);

		m_MucModel = new BookmarkedMUCsModel();
		mucTree.SetModel(m_MucModel);

		friendSearchContainer.Hide();
		
		rosterViewButton.icon  = new QIcon(new QPixmap("resource:/view-grid.png"));
		rosterSearchButton.icon = new QIcon(new QPixmap("resource:/simple-search.png"));
		addFriendButton.icon = new QIcon(new QPixmap("resource:/simple-add.png"));

		UpdateOnlineCount();
	}

	public new void Show ()
	{
		base.Show();
		rosterGrid.SetFocus();
	}

	public int AccountsCount {
		get {
			return m_AccountsContainer.Layout().Count();
		}
	}
	
	public void AddAccount(Account account)
	{
		AccountStatusWidget widget = new AccountStatusWidget(account, this, (MainWindow)base.TopLevelWidget());
		m_AccountsContainer.Layout().AddWidget(widget);
		widget.Show();
	}

	public void RemoveAccount(Account account)
	{
		throw new NotImplementedException();
	}
	
	public void AddActivityFeedItem (IActivityFeedItem item)
	{
		Application.Invoke(delegate {
			string accountJid = (item is XmppActivityFeedItem && ((XmppActivityFeedItem)item).Account != null) ? ((XmppActivityFeedItem)item).Account.Jid.Bare : null;
			string fromJid = (item is XmppActivityFeedItem) ? ((XmppActivityFeedItem)item).FromJid : null;
			string content = Util.Linkify(item.Content);
			string js = Util.CreateJavascriptCall("ActivityFeed.addItem", accountJid, item.Type, item.AvatarUrl, 
		                                      	  fromJid, item.FromName, item.FromUrl, item.ActionItem, content, 
			                                      item.ContentUrl);
			var result = m_ActivityWebView.Page().MainFrame().EvaluateJavaScript(js);
			if (!result.IsNull()) {
				m_ActivityFeedItems.Add(result.ToString(), item);
			}
		});
	}
	
	void HandleItemActivated (AvatarGrid<RosterItem> grid, RosterItem item)
	{
		// FIXME: Move to controller.
		Synapse.ServiceStack.ServiceManager.Get<Synapse.UI.Services.GuiService>().OpenChatWindow(item.Account, item.Item.JID);
	}

	void HandleAccountAdded (Account account)
	{
		account.Client.OnPresence += HandleOnPresence;
		account.ConnectionStateChanged += HandleConnectionStateChanged;

		Application.Invoke(delegate {
			UpdateOnlineCount();
		});
	}

	void HandleAccountRemoved (Account account)
	{
		account.Client.OnPresence -= HandleOnPresence;
		account.ConnectionStateChanged -= HandleConnectionStateChanged;
		
		Application.Invoke(delegate {
			UpdateOnlineCount();
		});
	}

	void HandleOnPresence (object o, Presence pres)
	{
		Application.Invoke(delegate {
			UpdateOnlineCount();
		});
	}

	void HandleConnectionStateChanged (Account account)
	{
		Application.Invoke(delegate {
			UpdateOnlineCount();
		});
	}

	void UpdateOnlineCount ()
	{
		var accountService = ServiceManager.Get<AccountService>();
		int num = accountService.Accounts.Sum(account => account.NumOnlineFriends);
		statsLabel.Text = String.Format("{0} friends online", num);
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
					selectedAccount.JoinMuc(jid);
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
					account.JoinMuc(conf.JID);
				} catch (UserException e) {
					QMessageBox.Critical(this.TopLevelWidget(), "Synapse", e.Message);
				}
			}
		}
	}

	[Q_SLOT]
	void on_rosterGrid_customContextMenuRequested(QPoint point)
	{
		m_MenuDownItem = rosterGrid.HoverItem;
		if (m_MenuDownItem != null) {
			m_InviteActions.ForEach(a => m_InviteMenu.RemoveAction(a));
			if (m_MenuDownItem.Account.ConferenceManager.Count > 0) {
				m_InviteActions.Add(m_InviteMenu.AddSeparator());
				foreach (var conference in m_MenuDownItem.Account.ConferenceManager.Rooms) {
					QAction action = m_InviteMenu.AddAction(conference.JID);
					m_InviteActions.Add(action);
				}
			}
			m_RosterItemMenu.Popup(rosterGrid.MapToGlobal(point));
		}
	}

	[Q_SLOT]
	void rosterItemMenu_triggered (QAction action)
	{
		// FIXME: Actions should be handled in the controller.
		
		if (m_MenuDownItem == null)
			return;
		
		if (action == m_ViewProfileAction) {
			new UserProfileWindowController(m_MenuDownItem.Account, m_MenuDownItem.Item.JID);
		} else if (action == m_IMAction) {
			Synapse.ServiceStack.ServiceManager.Get<Synapse.UI.Services.GuiService>().OpenChatWindow(m_MenuDownItem.Account, m_MenuDownItem.Item.JID);	
		} else if (m_InviteActions.Contains(action)) {
			// FIXME
			Console.WriteLine("Send Invitation!!");
		} else if (action == m_EditGroupsAction) {
			var win = new EditGroupsWindow(m_MenuDownItem.Account, m_MenuDownItem.Item);
			win.Show();
		} else if (action == m_RemoveAction) {
			if (QMessageBox.Question(this.TopLevelWidget(), "Synapse", "Are you sure you want to remove this friend?", (uint)QMessageBox.StandardButton.Yes | (uint)QMessageBox.StandardButton.No) == QMessageBox.StandardButton.Yes) {
				m_MenuDownItem.Account.RemoveRosterItem(m_MenuDownItem.Item.JID);
			}
		}
	}
	
	[Q_SLOT]
	void rosterMenu_triggered (QAction action)
	{
		if (action == m_ShowOfflineAction) {
			m_RosterModel.ShowOffline = action.Checked;
		} else if (action == m_ShowTransportsAction) {
			m_RosterModel.ShowTransports = action.Checked;
		}
	}

	[Q_SLOT]
	void rosterViewActionGroup_triggered (QAction action)
	{
		if (action == m_ListModeAction) {
			rosterGrid.ListMode = action.Checked;
			rosterViewButton.icon  = new QIcon(new QPixmap("resource:/view-list.png"));
		} else if (action == m_GridModeAction) {
			rosterGrid.ListMode = !action.Checked;
			rosterViewButton.icon  = new QIcon(new QPixmap("resource:/view-grid.png"));
		}
	}
	
	[Q_SLOT]
	void zoomSlider_valueChanged (int value)
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
		if (m_ActivityWebView.Url.ToString() != "resource:/feed.html")
			return;
		
		if (!ok)
			throw new Exception("Failed to load activity feed html!");

		// FIXME: This is very strange.
		while (m_ActivityWebView.Page().MainFrame().EvaluateJavaScript("ActivityFeed.loaded").ToBool() != true) {
			Console.WriteLine("Failed to load activity feed, trying again!");
			m_ActivityWebView.Page().MainFrame().Load("resource:/feed.html");
			return;
		}

		var feedService = ServiceManager.Get<ActivityFeedService>();
		foreach (var template in feedService.Templates.Values) {
			string js = Util.CreateJavascriptCall("ActivityFeed.addTemplate", template.Name, template.SingularText,
			                                      template.PluralText, template.IconUrl, template.Actions);
			var ret = m_ActivityWebView.Page().MainFrame().EvaluateJavaScript(js);
			if (ret.IsNull() || !ret.ToBool()) {
				throw new Exception("Failed to add template!\n" + js);
			}
		}
		
		feedService.NewItem += AddActivityFeedItem;

		// FIXME: This can't stay here, too many other things use this service.
		// Will need to move this into ActivityFeedService HandleOnClientStarted, 
		// and implement a queue in here.
		feedService.FireQueued();
	}

	[Q_SLOT]
	void HandleActivityLinkClicked (QUrl url)
	{
		try {
			Uri uri = new Uri(url.ToString());
			if (uri.Scheme == "http" || uri.Scheme == "https") {
				Gui.Open(uri.ToString());
			} else {
				if (uri.Scheme == "xmpp") {
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
				} else if (uri.Scheme == "activity-item") {
					string itemId = uri.AbsolutePath;
					string action = uri.Query.Substring(1);
					m_ActivityFeedItems[itemId].TriggerAction(action);
				}
			}
		} catch (Exception ex) {
			Console.Error.WriteLine(ex);
			QMessageBox.Critical(null, "Synapse Error", ex.Message);
		}
	}

	[Q_SLOT]
	void on_addFriendButton_clicked ()
	{
		Account account = Gui.ShowAccountSelectMenu(addFriendButton);
		if (account != null) {
			AddFriendWindow window = new AddFriendWindow(account);
			window.Show();
		}
	}

	[Q_SLOT]
	void rosterItemMenu_aboutToShow ()
	{
		rosterGrid.SuppressTooltips = true;
	}

	[Q_SLOT]
	void rosterItemMenu_aboutToHide ()
	{
		rosterGrid.SuppressTooltips = false;
	}

	[Q_SLOT]
	void on_rosterSearchButton_toggled (bool active)
	{
		friendSearchContainer.SetVisible(active);
		m_RosterModel.TextFilter = active ? friendSearchLineEdit.Text : String.Empty;
	}

	[Q_SLOT]
	void on_rosterViewButton_clicked ()
	{
		m_ShowOfflineAction.Checked = m_RosterModel.ShowOffline;
		m_ShowTransportsAction.Checked = m_RosterModel.ShowTransports;

		var buttonPos = rosterViewButton.MapToGlobal(new QPoint(0, rosterViewButton.Height()));
		m_RosterMenu.Popup(buttonPos);
	}
	#endregion
}
