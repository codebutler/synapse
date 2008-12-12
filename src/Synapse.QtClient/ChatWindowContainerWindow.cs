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

namespace Synapse.QtClient
{
	public class ChatWindowContainerWindow : QWidget
	{
		QTabWidget m_Tabs;
		
		public ChatWindowContainerWindow()
		{
			this.SetStyleSheet("QTabWidget::pane { border: 0px; }");
			
			m_Tabs = new QTabWidget();
			m_Tabs.tabPosition = QTabWidget.TabPosition.South;

			QToolButton newTabButton = new QToolButton(m_Tabs);
			newTabButton.AutoRaise = true;
			newTabButton.SetDefaultAction(new QAction(Helper.LoadIcon("stock_new-tab", 16), "New Tab", newTabButton));
			newTabButton.SetToolButtonStyle(Qt.ToolButtonStyle.ToolButtonIconOnly);
			QObject.Connect(newTabButton, Qt.SIGNAL("triggered(QAction*)"), this, Qt.SLOT("newTab(QAction*)"));
			m_Tabs.SetCornerWidget(newTabButton, Qt.Corner.BottomLeftCorner);

			QHBoxLayout rightButtonsLayout = new QHBoxLayout();			
			rightButtonsLayout.SetContentsMargins(0, 0, 0, 0);
			rightButtonsLayout.Spacing = 0;
			
			QToolButton closeTabButton = new QToolButton(m_Tabs);
			closeTabButton.SetToolButtonStyle(Qt.ToolButtonStyle.ToolButtonIconOnly);
			closeTabButton.AutoRaise = true;
			closeTabButton.SetDefaultAction(new QAction(Helper.LoadIcon("stock_close", 16), "Close Tab", closeTabButton));
			QObject.Connect(closeTabButton, Qt.SIGNAL("triggered(QAction*)"), this, Qt.SLOT("closeTab(QAction*)"));
			rightButtonsLayout.AddWidget(closeTabButton);

			QMenu menu = new QMenu(this);
			menu.AddAction(new QIcon(), "No Recently Closed Tabs");

			QToolButton trashButton = new QToolButton(m_Tabs);
			trashButton.SetToolButtonStyle(Qt.ToolButtonStyle.ToolButtonIconOnly);
			trashButton.AutoRaise = true;
			trashButton.PopupMode = QToolButton.ToolButtonPopupMode.InstantPopup;
			trashButton.SetMenu(menu);
			trashButton.SetDefaultAction(new QAction(Helper.LoadIcon("trashcan_empty", 16), "Recently Closed Tabs", trashButton));
			rightButtonsLayout.AddWidget(trashButton);

			rightButtonsLayout.AddWidget(new QSizeGrip(this));

			QWidget rightButtonsContainer = new QWidget(m_Tabs);
			rightButtonsContainer.SetLayout(rightButtonsLayout);
			m_Tabs.SetCornerWidget(rightButtonsContainer, Qt.Corner.BottomRightCorner);

			QVBoxLayout layout = new QVBoxLayout(this);
			layout.SetContentsMargins(0, 0, 0, 0);
			layout.AddWidget(m_Tabs, 1, 0);
			this.SetLayout(layout);

			var gui = ServiceManager.Get<GuiService>();
			gui.ChatWindowOpened += HandleChatWindowOpened;
			gui.ChatWindowClosed += HandleChatWindowClosed;

			QObject.Connect(m_Tabs, Qt.SIGNAL("currentChanged(int)"), this, Qt.SLOT("currentChanged(int)"));
			
			this.SetGeometry(0, 0, 445, 370);
			Helper.CenterWidgetOnScreen(this);

			/*
			newTab(null);
			this.Show();
			*/
		}

		void HandleChatWindowOpened (AbstractChatWindowController window, bool focus)
		{
			Application.Invoke(delegate {
				var view = (QWidget)window.View;
				
				int index = m_Tabs.CurrentIndex + 1;
				index = m_Tabs.InsertTab(index, view, view.WindowTitle);
	
				if (focus)
					m_Tabs.SetCurrentIndex(index);
	
				TabAdded();
			});
		}

		void HandleChatWindowClosed (AbstractChatWindowController window)
		{
			Application.Invoke(delegate {
				int index = m_Tabs.IndexOf((QWidget)window.View);
				m_Tabs.RemoveTab(index);
				TabClosed();
			});
		}

		[Q_SLOT]
		void currentChanged(int index)
		{
			this.WindowTitle = m_Tabs.TabText(index);
		}
			
		[Q_SLOT]
		void newTab (QAction action)
		{
			int index = m_Tabs.CurrentIndex + 1;
			index = m_Tabs.InsertTab(index, new EmptyTab(), "New Tab");
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

		class EmptyTab : QWebView
		{
			public EmptyTab ()
			{
				var asm = Assembly.GetEntryAssembly();
				using (StreamReader reader = new StreamReader(asm.GetManifestResourceStream("newtab.html"))) {
					this.SetHtml(reader.ReadToEnd());
				}
			}
		}
	}
}
