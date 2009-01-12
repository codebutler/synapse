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
using Qyoto;
using Synapse.ServiceStack;
using Synapse.UI.Services;
using Synapse.UI.Controllers;
using Synapse.UI.Views;
using Mono.Rocks;

namespace Synapse.QtClient
{
	public class TabbedChatsWindow : QWidget, ITabbedChatsWindowView
	{
		QTabWidget m_Tabs;
		
		public TabbedChatsWindow(TabbedChatsWindowController controller)
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
		}
		
		public void AddChatWindow (AbstractChatWindowController window, bool focus)
		{
			var view = (QWidget)window.View;
			
			int oldIndex = m_Tabs.CurrentIndex;
			m_Tabs.InsertTab(oldIndex + 1, view, view.WindowIcon, view.WindowTitle);

			if (focus) {
				FocusChatWindow(window);
			} else {
				m_Tabs.SetCurrentIndex(oldIndex);
			}
			
			TabAdded();

			window.View.UrgencyHintChanged += HandleChatUrgencyHintChanged;
		}

		public void FocusChatWindow (AbstractChatWindowController window)
		{
			window.View.Show();
			int newIndex = m_Tabs.IndexOf((QWidget)window.View);
			m_Tabs.SetCurrentIndex(newIndex);
			if (this.Minimized)
				this.ShowNormal();
			this.SetFocus();			
		}

		public void RemoveChatWindow (AbstractChatWindowController window)
		{
			int index = m_Tabs.IndexOf((QWidget)window.View);
			m_Tabs.RemoveTab(index);
			TabClosed();

			window.View.UrgencyHintChanged -= HandleChatUrgencyHintChanged;
		}

		public IChatWindowView CurrentChat {
			get {
				return m_Tabs.CurrentWidget() as IChatWindowView;
			}
		}

		void HandleChatUrgencyHintChanged (object sender, EventArgs args)
		{
			bool urgencyHint = false;
			
			for (int i = 0; i < m_Tabs.Count; i++) {
				IChatWindowView chat = m_Tabs.Widget(i) as IChatWindowView;
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
	}
}
