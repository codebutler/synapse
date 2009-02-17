//
// ChatWindow.cs
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
using System.Xml;

using Qyoto;

using Synapse.ServiceStack;
using Synapse.UI;
using Synapse.UI.Services;
using Synapse.UI.Chat;
using Synapse.Xmpp;
using Synapse.QtClient;
using Synapse.QtClient.ExtensionNodes;

using jabber;
using jabber.connection;
using jabber.protocol.client;
using jabber.protocol.iq;

using Mono.Addins;

namespace Synapse.QtClient.Windows
{
	public partial class ChatWindow : QWidget
	{
		public event EventHandler Closed;
		public event EventHandler UrgencyHintChanged;
		
		bool m_UrgencyHint = false;
	
		QAction m_BoldAction;
		QAction m_UnderlineAction;
		QAction m_ItalicAction;
		QAction m_StrikethroughAction;

		QComboBox m_ToComboBox;
	
		IChatHandler m_Handler;
	
		AbstractChatContent m_PreviousContent;
		
		internal ChatWindow (IChatHandler handler)
		{
			if (handler == null)
				throw new ArgumentNullException("handler");
			m_Handler = handler;
			
			SetupUi();
			
			if (handler is MucHandler) {
				var mucHandler = (MucHandler)handler;
				participantsGrid.Model = mucHandler.GridModel;
				participantsGrid.ContextMenuPolicy = Qt.ContextMenuPolicy.ActionsContextMenu;
				
				var group = new QActionGroup(this);
				
				var gridModeAction = new QAction("View as Grid", this);
				QObject.Connect(gridModeAction, Qt.SIGNAL("triggered()"), this, Qt.SLOT("HandleGridModeActionTriggered()")); 
				gridModeAction.SetActionGroup(group);
				gridModeAction.Checkable = true;
				gridModeAction.Checked = true;
				participantsGrid.AddAction(gridModeAction);
		
				var listModeAction = new QAction("View as List", this);
				QObject.Connect(listModeAction, Qt.SIGNAL("triggered()"), this, Qt.SLOT("HandleListModeActionTriggered()"));
				listModeAction.SetActionGroup(group);
				listModeAction.Checkable = true;
				participantsGrid.AddAction(listModeAction);
				
				var separatorAction = new QAction(participantsGrid);
				separatorAction.SetSeparator(true);
				participantsGrid.AddAction(separatorAction);
				
				var sliderAction = new AvatarGridZoomAction<jabber.connection.RoomParticipant>(participantsGrid);
				participantsGrid.AddAction(sliderAction);
			
				m_ConversationWidget.ChatName = mucHandler.Room.JID;
				this.WindowTitle = mucHandler.Room.JID.User; // FIXME: Show only "user" in tab, show full room jid in title?
				this.WindowIcon = Gui.LoadIcon("internet-group-chat");
			} else {
				var chatHandler = (ChatHandler)handler;
				rightContainer.Hide();
				this.WindowTitle = chatHandler.Account.GetDisplayName(chatHandler.Jid);
				this.WindowIcon = new QIcon((QPixmap)Synapse.Xmpp.AvatarManager.GetAvatar(chatHandler.Jid));
			}

			handler.NewContent += HandleNewContent;
			handler.ReadyChanged += HandleReadyChanged;
	
			splitter.SetStretchFactor(1, 0);
			splitter_2.SetStretchFactor(1, 0);
		
			KeyPressEater eater = new KeyPressEater(this);
			eater.KeyEvent += HandleKeyEvent;
			textEdit.InstallEventFilter(eater);
	
			QToolBar toolbar = new QToolBar(this);
			toolbar.IconSize = new QSize(16, 16);
						
			var formatMenu = new QMenu(this);			
			var formatMenuButton = new QToolButton(this);
			formatMenuButton.ToolButtonStyle = ToolButtonStyle.ToolButtonTextBesideIcon;
			formatMenuButton.Text = "Format";
			formatMenuButton.icon = Gui.LoadIcon("fonts", 16);
			formatMenuButton.PopupMode = QToolButton.ToolButtonPopupMode.InstantPopup;
			formatMenuButton.SetMenu(formatMenu);
			toolbar.AddWidget(formatMenuButton);
	
			m_BoldAction = new QAction(Gui.LoadIcon("format-text-bold", 16), "Bold", this);
			m_BoldAction.Shortcut = "Ctrl+B";
			m_BoldAction.Checkable = true;
			formatMenu.AddAction(m_BoldAction);
			
			m_ItalicAction = new QAction(Gui.LoadIcon("format-text-italic", 16), "Italic", this);
			m_ItalicAction.Shortcut = "Ctrl+I";
			m_ItalicAction.Checkable = true;
			formatMenu.AddAction(m_ItalicAction);
			
			m_UnderlineAction = new QAction(Gui.LoadIcon("format-text-underline", 16), "Underline", this);
			m_UnderlineAction.Shortcut = "Ctrl+U";
			m_UnderlineAction.Checkable = true;
			formatMenu.AddAction(m_UnderlineAction);
	
			m_StrikethroughAction = new QAction(Gui.LoadIcon("format-text-strikethrough", 16), "Strikethrough", this);
			m_StrikethroughAction.Shortcut = "Ctrl+S";
			m_StrikethroughAction.Checkable = true;
			formatMenu.AddAction(m_StrikethroughAction);
			
			formatMenu.AddSeparator();
			
			formatMenu.AddAction(Gui.LoadIcon("edit-clear", 16), "Clear Formatting");
			
			var insertMenu = new QMenu(this);			
			var insertMenuButton = new QToolButton(this);
			insertMenuButton.ToolButtonStyle = ToolButtonStyle.ToolButtonTextBesideIcon;
			insertMenuButton.Text = "Insert";
			insertMenuButton.icon = Gui.LoadIcon("image", 16);
			insertMenuButton.PopupMode = QToolButton.ToolButtonPopupMode.InstantPopup;
			insertMenuButton.SetMenu(insertMenu);
			toolbar.AddWidget(insertMenuButton);
			
			insertMenu.AddAction(Gui.LoadIcon("insert-image", 16), "Photo...");
			insertMenu.AddAction(Gui.LoadIcon("insert-link", 16), "Link...");
			
			foreach (IActionCodon node in AddinManager.GetExtensionNodes("/Synapse/QtClient/ChatWindow/InsertActions")) {
				insertMenu.AddAction((QAction)node.CreateInstance(this));
			}		
			
			toolbar.AddSeparator();
			
			var activitiesMenu = new QMenu(this);			
			var activitiesMenuButton = new QToolButton(this);
			activitiesMenuButton.ToolButtonStyle = ToolButtonStyle.ToolButtonTextBesideIcon;
			activitiesMenuButton.Text = "Activities";
			activitiesMenuButton.icon = Gui.LoadIcon("applications-games", 16); // FIXME: Not a good icon.
			activitiesMenuButton.PopupMode = QToolButton.ToolButtonPopupMode.InstantPopup;
			activitiesMenuButton.SetMenu(activitiesMenu);
			toolbar.AddWidget(activitiesMenuButton);
			
			if (m_Handler is ChatHandler) {
				activitiesMenu.AddAction(Gui.LoadIcon("internet-group-chat", 16), "Invite to Conference...");
				activitiesMenu.AddSeparator();
			}
			
			activitiesMenu.AddAction(Gui.LoadIcon("applications-graphics", 16), "Launch Whiteboard...");
			activitiesMenu.AddAction(Gui.LoadIcon("desktop", 16), "Share Desktop...");
			
			var spacerWidget = new QWidget(toolbar);
			spacerWidget.SetSizePolicy(QSizePolicy.Policy.Expanding, QSizePolicy.Policy.Fixed);
			toolbar.AddWidget(spacerWidget);

			var toContainer = new QWidget(toolbar);
			var layout = new QHBoxLayout(toContainer);
			layout.SetContentsMargins(0, 0, 4, 0);

			m_ToComboBox = new QComboBox(toContainer);
			
			layout.AddWidget(new QLabel("To:", toContainer));
			layout.AddWidget(m_ToComboBox);
			
			QAction toWidgetAction = (QWidgetAction)toolbar.AddWidget(toContainer);

			m_ToComboBox.AddItem("Automatic", "auto");
			m_ToComboBox.InsertSeparator(1);

			((QVBoxLayout)bottomContainer.Layout()).InsertWidget(0, toolbar);
			
			if (handler is ChatHandler) {
				var chatHandler = (ChatHandler)handler;
				handler.Account.Client.OnPresence += delegate(object sender, Presence pres) {
					if (pres.From.Bare != chatHandler.Jid.Bare || pres.Priority == "-1") {
						return;
					}
					QApplication.Invoke(delegate {
						if (!String.IsNullOrEmpty(pres.From.Resource)) {
							if (pres.Type == PresenceType.available) {
								string text = String.Format("{0} ({1})", Helper.GetResourceDisplay(pres), Helper.GetPresenceDisplay(pres));
								int i = m_ToComboBox.FindData(pres.From.Resource);
								if (i == -1) {
									m_ToComboBox.AddItem(text, pres.From.Resource);
								} else {
									m_ToComboBox.SetItemText(i, text);
								}
							} else if (pres.Type == PresenceType.unavailable) {
								int i = m_ToComboBox.FindData(pres.From.Resource);
								if (i > -1) {
									m_ToComboBox.RemoveItem(i);
									m_ToComboBox.CurrentIndex = 0;
								}
							}
						}

						string title = null;
						if (handler.Account.PresenceManager[pres.From.BareJID] == null) {
							title = String.Format("{0} (Offline)", chatHandler.Account.GetDisplayName(chatHandler.Jid));
						} else {
							title = chatHandler.Account.GetDisplayName(chatHandler.Jid);	
						}
						Gui.TabbedChatsWindow.SetTabTitle(this, title);
					});
				};
				
				foreach (var presence in chatHandler.Account.PresenceManager.GetAll(chatHandler.Jid)) {
					if (presence.Priority != "-1" && !String.IsNullOrEmpty(presence.From.Resource)) {
						string text = String.Format("{0} ({1})", Helper.GetResourceDisplay(presence), Helper.GetPresenceDisplay(presence));
						m_ToComboBox.AddItem(text, presence.From.Resource);
					}
				}			
				
				// FIXME: Make this a menu with "View Profile" and "View History".
				toolbar.AddAction(Gui.LoadIcon("info", 16), "View Profile");			
			} else {
				toWidgetAction.Visible = false;
			}
			
			m_ConversationWidget.LoadTheme("Mockie", "Orange - Icon Left");
			
			m_Handler.FireQueued();
		}

		public IChatHandler Handler {
			get {
				return m_Handler;
			}
		}
		
		public bool UrgencyHint {
			get {
				return m_UrgencyHint;
			}
			internal set {
				m_UrgencyHint = value;
				
				if (UrgencyHintChanged != null) {
					UrgencyHintChanged(this, EventArgs.Empty);
				}
			}
		}
	
		protected override void CloseEvent(QCloseEvent evnt)
		{			
			if (Closed != null)
				Closed(this, EventArgs.Empty);
			
			m_Handler.NewContent -= HandleNewContent;
			m_Handler.Dispose();
			m_Handler = null;
			
			evnt.Accept();
		}
		
		protected override void FocusInEvent (Qyoto.QFocusEvent arg1)
		{
			base.FocusInEvent (arg1);
			
			textEdit.SetFocus();
			UrgencyHint = false;
		}

		void HandleReadyChanged (object o, EventArgs args)
		{
			textEdit.Enabled = m_Handler.Ready;
		}
		
		void HandleNewContent (IChatHandler handler, AbstractChatContent content)
		{			
			if (content is ChatContentTyping) {
				var typingContent = (ChatContentTyping)content;
				if (m_Handler is ChatHandler) {
					var chatHandler = (ChatHandler)m_Handler;
					string title = null;
					if (typingContent.TypingState != TypingState.None && typingContent.TypingState != TypingState.Active) {
						title = String.Format("{0} ({1})", chatHandler.Account.GetDisplayName(chatHandler.Jid), typingContent.TypingState.ToString());
					} else {
						title = chatHandler.Account.GetDisplayName(chatHandler.Jid);
					}
					QApplication.Invoke(delegate {
						Gui.TabbedChatsWindow.SetTabTitle(this, title);
					});
				}
			} else {
				bool isSimilar   = m_PreviousContent != null && content.IsSimilarToContent(m_PreviousContent);
				//bool replaceLast = m_PreviousContent is ChatContentStatus && 
				//	               content is ChatContentStatus && 
				//	               ((ChatContentStatus)m_PreviousContent).CoalescingKey == ((ChatContentStatus)content).CoalescingKey;
				bool replaceLast = m_PreviousContent is ChatContentTyping;
				
				m_PreviousContent = content;
			
				QApplication.Invoke(delegate {
					m_ConversationWidget.AppendContent(content, isSimilar, false, replaceLast);
					
					if (content is ChatContentMessage && !IsActive) {
						UrgencyHint = true;
					}
						
					if (m_Handler is ChatHandler) {
						if (content is ChatContentMessage && (content.Source.Bare == ((ChatHandler)m_Handler).Jid.Bare)) {
							// Select this resource so our replies go to it.
							int i = m_ToComboBox.FindData(((ChatContentMessage)content).Source.Resource);
							m_ToComboBox.CurrentIndex = (i > -1) ? i : 0;
						}
					}
				});
			}
		}
		
		bool HandleKeyEvent(QKeyEvent kevent)
		{
			if ((kevent.Modifiers() & (uint)Qt.KeyboardModifier.ControlModifier) == 0 && kevent.Key() == (int)Qt.Key.Key_Return || kevent.Key() == (int)Qt.Key.Key_Enter) {
				// FIXME: Need to clean this HTML up...
				// string html = textEdit.Html;
				string html = textEdit.PlainText;
				
				
				if (m_Handler is ChatHandler) {
					string resource = m_ToComboBox.ItemData(m_ToComboBox.CurrentIndex);
					((ChatHandler)m_Handler).Resource = (resource == "auto") ? null : resource;
				}
				
				m_Handler.Send(html);
				textEdit.Clear();
				return true;
			} else {
				return false;
			}
		}
		
		bool IsActive {
			get {
				return (Gui.TabbedChatsWindow.IsActiveWindow && Gui.TabbedChatsWindow.CurrentChat == this);
			}
		}
		
		[Q_SLOT]
		void HandleGridModeActionTriggered ()
		{
			participantsGrid.ListMode = false;
		}
		
		[Q_SLOT]
		void HandleListModeActionTriggered ()
		{
			participantsGrid.ListMode = true;
		}
	}
}
