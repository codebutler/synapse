//
// ChatWindowContainerWindow.cs
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
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using Qyoto;
using Synapse.ServiceStack;
using Synapse.UI.Chat;
using Synapse.UI.Services;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;
using Mono.Rocks;
using jabber;
using jabber.protocol.client;
using jabber.client;
using jabber.connection;
	
namespace Synapse.QtClient.Windows
{
	public class TabbedChatsWindow : QWidget
	{
		QTabWidget m_Tabs;
		
		Dictionary<Account, AccountChatWindowManager> m_AccountManagers = new Dictionary<Account, AccountChatWindowManager>();
		
		public TabbedChatsWindow()
		{
			// FIXME: This doesn't work very well in most themes...
			//this.SetStyleSheet("QTabWidget::pane { border: 0px; }");

			// The tab widget messes up this background color.
			this.SetStyleSheet("QTabWidget > QWidget { background: palette(window); }");
			
			m_Tabs = new QTabWidget();
			m_Tabs.tabPosition = QTabWidget.TabPosition.South;

			QToolButton newTabButton = new QToolButton(m_Tabs);
			newTabButton.AutoRaise = true;
			newTabButton.SetDefaultAction(new QAction(Gui.LoadIcon("stock_new-tab", 16), "New Tab", newTabButton));
			newTabButton.SetToolButtonStyle(Qt.ToolButtonStyle.ToolButtonIconOnly);
			QObject.Connect(newTabButton, Qt.SIGNAL("triggered(QAction*)"), this, Qt.SLOT("newTab(QAction*)"));
			m_Tabs.SetCornerWidget(newTabButton, Qt.Corner.BottomLeftCorner);

			QHBoxLayout rightButtonsLayout = new QHBoxLayout();			
			rightButtonsLayout.SetContentsMargins(0, 0, 0, 0);
			rightButtonsLayout.Spacing = 0;
			
			QToolButton closeTabButton = new QToolButton(m_Tabs);
			closeTabButton.SetToolButtonStyle(Qt.ToolButtonStyle.ToolButtonIconOnly);
			closeTabButton.AutoRaise = true;
			closeTabButton.SetDefaultAction(new QAction(Gui.LoadIcon("stock_close", 16), "Close Tab", closeTabButton));
			QObject.Connect(closeTabButton, Qt.SIGNAL("triggered(QAction*)"), this, Qt.SLOT("closeTab(QAction*)"));
			rightButtonsLayout.AddWidget(closeTabButton);

			QMenu menu = new QMenu(this);
			menu.AddAction(new QIcon(), "No Recently Closed Tabs");

			QToolButton trashButton = new QToolButton(m_Tabs);
			trashButton.SetToolButtonStyle(Qt.ToolButtonStyle.ToolButtonIconOnly);
			trashButton.AutoRaise = true;
			trashButton.PopupMode = QToolButton.ToolButtonPopupMode.InstantPopup;
			trashButton.SetMenu(menu);
			trashButton.SetDefaultAction(new QAction(Gui.LoadIcon("trashcan_empty", 16), "Recently Closed Tabs", trashButton));
			rightButtonsLayout.AddWidget(trashButton);

			// FIXME: This looks bad.
			//rightButtonsLayout.AddWidget(new QSizeGrip(this));

			QWidget rightButtonsContainer = new QWidget(m_Tabs);
			rightButtonsContainer.SetLayout(rightButtonsLayout);
			m_Tabs.SetCornerWidget(rightButtonsContainer, Qt.Corner.BottomRightCorner);

			QVBoxLayout layout = new QVBoxLayout(this);
			layout.SetContentsMargins(0, 0, 0, 0);
			layout.AddWidget(m_Tabs, 1, 0);
			this.SetLayout(layout);

			QObject.Connect(m_Tabs, Qt.SIGNAL("currentChanged(int)"), this, Qt.SLOT("currentChanged(int)"));
			
			this.SetGeometry(0, 0, 445, 370);
			Gui.CenterWidgetOnScreen(this);

			var closeShortcuts = new [] { "Ctrl+w", "Esc" };
			closeShortcuts.ForEach(shortcut => {
				QAction closeAction = new QAction(this);
				QObject.Connect(closeAction, Qt.SIGNAL("triggered(bool)"), this, Qt.SLOT("closeAction_triggered(bool)"));
				closeAction.Shortcut = new QKeySequence(shortcut);
				this.AddAction(closeAction);
			});

			0.UpTo(9).ForEach(num => {
				QAction action = new QAction(this);
				action.Shortcut = new QKeySequence("Alt+" + num.ToString());
				QObject.Connect(action, Qt.SIGNAL("triggered(bool)"), delegate {
					m_Tabs.CurrentIndex = num - 1;
				});
				this.AddAction(action);
			});

			QAction nextTabAction = new QAction(this);
			nextTabAction.Shortcut = new QKeySequence(QKeySequence.StandardKey.NextChild);
			QObject.Connect(nextTabAction, Qt.SIGNAL("triggered(bool)"), delegate {
				if (m_Tabs.CurrentIndex == m_Tabs.Count - 1)
					m_Tabs.CurrentIndex = 0;
				else
					m_Tabs.CurrentIndex += 1;
			});
			this.AddAction(nextTabAction);
			
			QAction prevTabAction = new QAction(this);
			prevTabAction.Shortcut = new QKeySequence(QKeySequence.StandardKey.PreviousChild);
			QObject.Connect(prevTabAction, Qt.SIGNAL("triggered(bool)"), delegate {
				if (m_Tabs.CurrentIndex == 0)
					m_Tabs.CurrentIndex = m_Tabs.Count - 1;
				else
					m_Tabs.CurrentIndex -= 1;
			});

			var accountService = ServiceManager.Get<AccountService>();
			accountService.AccountAdded += HandleAccountAdded;
			accountService.AccountRemoved += HandleAccountRemoved;
			foreach (Account account in accountService.Accounts)
				HandleAccountAdded(account);
		}

		public void StartChat (Account account, JID jid)
		{
			m_AccountManagers[account].OpenChatWindow(jid, true);
		}
		
		public ChatWindow CurrentChat {
			get {
				return m_Tabs.CurrentWidget() as ChatWindow;
			}
		}

		public void SetTabTitle (QWidget tab, string title)
		{
			int index = m_Tabs.IndexOf(tab);
			m_Tabs.SetTabText(index, title);
			if (m_Tabs.CurrentIndex == index)
				this.WindowTitle = title;
		}

		void HandleAccountAdded (Account account)
		{
			m_AccountManagers.Add(account, new AccountChatWindowManager(account));
		}

		void HandleAccountRemoved (Account account)
		{
			m_AccountManagers[account].Dispose();
			m_AccountManagers.Remove(account);
		}
		
		void AddChatWindow (ChatWindow window, bool focus)
		{
			int oldIndex = m_Tabs.CurrentIndex;
			m_Tabs.InsertTab(oldIndex + 1, window, window.WindowIcon, window.WindowTitle);

			if (focus) {
				FocusChatWindow(window);
			} else {
				m_Tabs.SetCurrentIndex(oldIndex);
			}
			
			TabAdded();

			window.UrgencyHintChanged += HandleChatUrgencyHintChanged;
		}

		void FocusChatWindow (ChatWindow window)
		{
			window.Show();
			int newIndex = m_Tabs.IndexOf(window);
			m_Tabs.SetCurrentIndex(newIndex);
			if (this.Minimized)
				this.ShowNormal();
			this.ActivateWindow();
			this.Raise();
			window.SetFocus();
		}

		void RemoveChatWindow (ChatWindow window)
		{
			int index = m_Tabs.IndexOf(window);
			m_Tabs.RemoveTab(index);
			TabClosed();

			window.UrgencyHintChanged -= HandleChatUrgencyHintChanged;
		}
		
		void HandleChatUrgencyHintChanged (object sender, EventArgs args)
		{
			bool urgencyHint = false;
			
			for (int i = 0; i < m_Tabs.Count; i++) {
				ChatWindow chat = m_Tabs.Widget(i) as ChatWindow;
				if (chat != null) {
					if (chat.UrgencyHint) {
						m_Tabs.SetTabIcon(i, Gui.LoadIcon("dialog-warning", 16));
						urgencyHint = true;
					} else {
						m_Tabs.SetTabIcon(i, ((QWidget)chat).WindowIcon);
					}
				}					
			}

			if (urgencyHint)
				QApplication.Alert(this);
		}
		
		[Q_SLOT]
		void currentChanged(int index)
		{
			if (m_Tabs.Widget(index) != null) {
				m_Tabs.Widget(index).SetFocus();
				this.WindowTitle = m_Tabs.TabText(index);
				this.WindowIcon  = m_Tabs.TabIcon(index);
			}
		}
			
		[Q_SLOT]
		void newTab (QAction action)
		{
			var tab = new EmptyTab();
			int index = m_Tabs.CurrentIndex + 1;
			index = m_Tabs.InsertTab(index, tab, tab.WindowIcon, "New Tab");
			m_Tabs.SetCurrentIndex(index);
			
			TabAdded();
		}
		
		[Q_SLOT]
		void closeTab (QAction action)
		{
			int index = m_Tabs.CurrentIndex;
			var widget = m_Tabs.CurrentWidget();
			widget.Close();
			if (widget is EmptyTab) {
				m_Tabs.RemoveTab(index);
				TabClosed();
			}
		}

		[Q_SLOT]
		void closeAction_triggered (bool chkd)
		{
			closeTab(null);
		}
		 
		void TabAdded ()
		{
			if (!this.IsVisible())
				this.Show();
		}
		
		void TabClosed ()
		{
			if (m_Tabs.Count == 0)
				this.Hide();
		}

		protected override void CloseEvent(QCloseEvent evnt)
		{
			while (m_Tabs.Count > 0)
				closeTab(null);
			
			this.Hide();
			evnt.Accept();
		}

		protected override void FocusInEvent (Qyoto.QFocusEvent arg1)
		{
			base.FocusInEvent (arg1);
		}

		protected override void ChangeEvent (Qyoto.QEvent arg1)
		{
			if (arg1.type() == QEvent.TypeOf.ActivationChange) {
				if (this.IsActiveWindow) {
					m_Tabs.CurrentWidget().SetFocus();
				}
			}
		}
		
		class EmptyTab : QWebView
		{
			public EmptyTab ()
			{
				var asm = Assembly.GetEntryAssembly();
				using (StreamReader reader = new StreamReader(asm.GetManifestResourceStream("newtab.html"))) {
					this.SetHtml(reader.ReadToEnd());
				}

				// FIXME: Need something better here.
				this.WindowIcon = Gui.LoadIcon("text-x-generic");
			}
		}

		class AccountChatWindowManager : IDisposable
		{
			Account m_Account;
			Dictionary<Room, ChatWindow> m_MucWindows;
			Dictionary<string, ChatWindow> m_ChatWindows;
			
			public AccountChatWindowManager (Account account)
			{
				m_MucWindows  = new Dictionary<Room, ChatWindow>();
				m_ChatWindows = new Dictionary<string, ChatWindow>();
				
				m_Account = account;
				account.ConferenceManager.OnJoin += HandleOnJoin;
				account.Client.OnMessage += HandleOnMessage;
				account.Client.OnPresence += HandleOnPresence;
			}

			public void Dispose ()
			{
				m_Account.ConferenceManager.OnJoin -= HandleOnJoin;
				m_Account.Client.OnMessage -= HandleOnMessage;
				m_Account.Client.OnPresence -= HandleOnPresence;
			}
				
			public ChatWindow OpenChatWindow (JID jid, bool focus)
			{
				ChatWindow window = null;
				if (!m_ChatWindows.ContainsKey(jid.Bare)) {
					window = new ChatWindow(new ChatHandler(m_Account, jid));
					window.Closed += HandleChatWindowClosed;
					m_ChatWindows.Add(jid.Bare, window);
	
					Gui.TabbedChatsWindow.AddChatWindow(window, focus);
				} else {
					window = m_ChatWindows[jid.Bare];
					if (focus) {
						Gui.TabbedChatsWindow.FocusChatWindow(window);
					}
				}
				return window;
			}

			void HandleOnJoin(Room room)
			{
				Application.Invoke(delegate {
					if (!m_MucWindows.ContainsKey(room)) {
						var window = new ChatWindow(new MucHandler(m_Account, room));
						window.Closed += HandleMucWindowClosed;
						m_MucWindows[room] = window;
						Gui.TabbedChatsWindow.AddChatWindow(window, true);
					}
				});
			}
	
			void HandleOnMessage (object sender, Message message)
			{
				Application.Invoke(delegate {
					if (message.Type == MessageType.chat) {
						// Make sure we don't open a new window if all we've got is a chatstate.
						// Some people like a "psycic" mode though, so this should be configurable.
						if (m_ChatWindows.ContainsKey(message.From.Bare) || (message.Body != null || message.Html != null )) {
							ChatWindow window = OpenChatWindow(message.From, false);
							((ChatHandler)window.Handler).AppendMessage(message);
						}
					}
				});
			}
			
			void HandleOnPresence (object o, Presence presence)
			{
				Application.Invoke(delegate {
					if (m_ChatWindows.ContainsKey(presence.From.Bare)) {
						var window = m_ChatWindows[presence.From.Bare];
						((ChatHandler)window.Handler).SetPresence(presence);
					}
				});
			}

			void HandleChatWindowClosed(object sender, EventArgs e)
			{
				Application.Invoke(delegate {
					var window = (ChatWindow)sender;
					m_ChatWindows.Remove(((ChatHandler)window.Handler).Jid.Bare);	
					Gui.TabbedChatsWindow.RemoveChatWindow(window);
				});
			}
			
			void HandleMucWindowClosed(object sender, EventArgs e)
			{
				Application.Invoke(delegate {
					ChatWindow window = ((ChatWindow)sender);
					m_MucWindows.Remove(((MucHandler)window.Handler).Room);				
					Gui.TabbedChatsWindow.RemoveChatWindow(window);
				});
			}
		}
	}
}
