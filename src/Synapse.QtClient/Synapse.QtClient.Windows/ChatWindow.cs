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
using Synapse.Services;
using Synapse.UI;
using Synapse.UI.Services;
using Synapse.UI.Chat;
using Synapse.Xmpp;
using Synapse.QtClient;
using Synapse.QtClient.ExtensionNodes;
using Synapse.QtClient.Widgets;

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
		QAction m_ClearFormattingAction;

		QAction m_InsertPhotoAction;
		QAction m_InsertLinkAction;
		
		QAction m_InviteToMucAction;

		QComboBox m_ToComboBox;
	
		IChatHandler m_Handler;
	
		AbstractChatContent m_PreviousContent;
		
		QMenu m_ParticipantsMenu;
		QMenu m_ParticipantItemMenu;
		
		QMenu m_ModeratorActionsMenu;
		QAction m_ChangeAffiliationAction;
		QAction m_AddAsFriendAction;
		
		QAction m_ModeratorAction;
		QAction m_ParticipantAction;
		QAction m_VisitorAction;
		
		RoomParticipant m_SelectedParticipant;
		
		internal ChatWindow (IChatHandler handler)
		{
			if (handler == null)
				throw new ArgumentNullException("handler");
			m_Handler = handler;
			
			SetupUi();
			
			if (handler is MucHandler) {				
				m_ParticipantsMenu = new QMenu(this);
				QObject.Connect(m_ParticipantsMenu, Qt.SIGNAL("aboutToShow()"), HandleMenuAboutToShow);
				QObject.Connect(m_ParticipantsMenu, Qt.SIGNAL("aboutToHide()"), HandleMenuAboutToHide);
								
				var mucHandler = (MucHandler)handler;
				participantsGrid.Model = mucHandler.GridModel;
				participantsGrid.ContextMenuPolicy = Qt.ContextMenuPolicy.CustomContextMenu;
				participantsGrid.ItemActivated += HandleItemActivated;
				
				var group = new QActionGroup(this);
				
				var gridModeAction = new QAction("View as Grid", this);
				QObject.Connect(gridModeAction, Qt.SIGNAL("triggered()"), HandleGridModeActionTriggered); 
				gridModeAction.SetActionGroup(group);
				gridModeAction.Checkable = true;
				gridModeAction.Checked = true;
				m_ParticipantsMenu.AddAction(gridModeAction);
		
				var listModeAction = new QAction("View as List", this);
				QObject.Connect(listModeAction, Qt.SIGNAL("triggered()"), HandleListModeActionTriggered);
				listModeAction.SetActionGroup(group);
				listModeAction.Checkable = true;
				m_ParticipantsMenu.AddAction(listModeAction);
				
				var separatorAction = new QAction(participantsGrid);
				separatorAction.SetSeparator(true);
				m_ParticipantsMenu.AddAction(separatorAction);
				
				var sliderAction = new AvatarGridZoomAction<jabber.connection.RoomParticipant>(participantsGrid);
				m_ParticipantsMenu.AddAction(sliderAction);
				
				m_ParticipantItemMenu = new QMenu(this);
				QObject.Connect(m_ParticipantItemMenu, Qt.SIGNAL("aboutToShow()"), HandleMenuAboutToShow);
				QObject.Connect(m_ParticipantItemMenu, Qt.SIGNAL("aboutToHide()"), HandleMenuAboutToHide);
				
				var mucViewProfileAction = new QAction("View Profile", this);
				QObject.Connect(mucViewProfileAction, Qt.SIGNAL("triggered()"), HandleMucViewProfileActionTriggered);
				m_ParticipantItemMenu.AddAction(mucViewProfileAction);
				
				var mucPrivateMessageAction = new QAction("IM", this);
				QObject.Connect(mucPrivateMessageAction, Qt.SIGNAL("triggered()"), HandleMucPrivateMessageTriggered);
				m_ParticipantItemMenu.AddAction(mucPrivateMessageAction);
				
				var mucSendFileAction = new QAction("Send File...", this);
				QObject.Connect(mucSendFileAction, Qt.SIGNAL("triggered()"), HandleMucSendFileActionTriggered);
				m_ParticipantItemMenu.AddAction(mucSendFileAction);
				
				var mucViewHistoryAction = new QAction("View History", this);
				QObject.Connect(mucViewHistoryAction, Qt.SIGNAL("triggered()"), HandleMucViewHistoryActionTriggered);
				m_ParticipantItemMenu.AddAction(mucViewHistoryAction);
				
				m_ModeratorActionsMenu = new QMenu("Moderator Actions", this);
				
				var roomRoleActionGroup = new QActionGroup(this);
				QObject.Connect(roomRoleActionGroup, Qt.SIGNAL("triggered(QAction*)"), this, Qt.SLOT("HandleRoomRoleActionGroupTriggered(QAction*)"));
				
				m_ModeratorAction = new QAction("Moderator", this);
				roomRoleActionGroup.AddAction(m_ModeratorAction);
				m_ModeratorAction.Checkable = true;
				m_ModeratorActionsMenu.AddAction(m_ModeratorAction);
				
				m_ParticipantAction = new QAction("Participant", this);
				roomRoleActionGroup.AddAction(m_ParticipantAction);
				m_ParticipantAction.Checkable = true;				
				m_ModeratorActionsMenu.AddAction(m_ParticipantAction);
				
				m_VisitorAction = new QAction("Visitor", this);
				roomRoleActionGroup.AddAction(m_VisitorAction);
				m_VisitorAction.Checkable = true;				
				m_ModeratorActionsMenu.AddAction(m_VisitorAction);
				
				m_ModeratorActionsMenu.AddSeparator();
				
				var mucKickAction = new QAction("Kick...", this);
				QObject.Connect(mucKickAction, Qt.SIGNAL("triggered()"), HandleMucKickActionTriggered);
				m_ModeratorActionsMenu.AddAction(mucKickAction);
				
				var mucBanAction = new QAction("Ban...", this);
				QObject.Connect(mucBanAction, Qt.SIGNAL("triggered()"), HandleMucBanActionTriggered);
				m_ModeratorActionsMenu.AddAction(mucBanAction);
				
				m_ModeratorActionsMenu.AddSeparator();	
				
				m_ChangeAffiliationAction = new QAction("Change Affiliation...", this);
				QObject.Connect(m_ChangeAffiliationAction, Qt.SIGNAL("triggered()"), HandleChangeAffiliationTriggered);
				m_ModeratorActionsMenu.AddAction(m_ChangeAffiliationAction);
				
				m_ParticipantItemMenu.AddSeparator();
				m_ParticipantItemMenu.AddMenu(m_ModeratorActionsMenu);
				
				m_ParticipantItemMenu.AddSeparator();
				
				m_AddAsFriendAction = new QAction("Add as Friend", this);
				m_ParticipantItemMenu.AddAction(m_AddAsFriendAction);
			
				this.WindowTitle = mucHandler.Room.JID.User; // FIXME: Show only "user" in tab, show full room jid in title?
				this.WindowIcon = Gui.LoadIcon("internet-group-chat");
			} else {
				var chatHandler = (ChatHandler)handler;
				rightContainer.Hide();
				
				if (((ChatHandler)handler).IsMucMessage) {
					this.WindowTitle = chatHandler.Jid.Resource;
				} else {
					this.WindowTitle = chatHandler.Account.GetDisplayName(chatHandler.Jid);
				}
				this.WindowIcon = new QIcon((QPixmap)Synapse.Xmpp.AvatarManager.GetAvatar(chatHandler.Jid));
			}
			
			m_ConversationWidget.ChatHandler = handler;

			handler.ReadyChanged += HandleReadyChanged;
	
			splitter.SetStretchFactor(1, 0);
			splitter_2.SetStretchFactor(1, 0);
		
			KeyPressEater eater = new KeyPressEater(this);
			eater.KeyEvent += HandleKeyEvent;
			textEdit.InstallEventFilter(eater);
	
			QToolBar toolbar = new QToolBar(this);
			toolbar.IconSize = new QSize(16, 16);
						
			var formatMenuButton = new QToolButton(this);

			var formatMenu = new QMenu(this);			
			QObject.Connect<QAction>(formatMenu, Qt.SIGNAL("triggered(QAction*)"), HandleFormatMenuActionTriggered);
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
			
			m_ClearFormattingAction = new QAction(Gui.LoadIcon("edit-clear", 16), "Clear Formatting", this);
			formatMenu.AddAction(m_ClearFormattingAction);

			var insertMenu = new QMenu(this);			
			var insertMenuButton = new QToolButton(this);
			insertMenuButton.ToolButtonStyle = ToolButtonStyle.ToolButtonTextBesideIcon;
			insertMenuButton.Text = "Insert";
			insertMenuButton.icon = Gui.LoadIcon("image", 16);
			insertMenuButton.PopupMode = QToolButton.ToolButtonPopupMode.InstantPopup;
			insertMenuButton.SetMenu(insertMenu);
			toolbar.AddWidget(insertMenuButton);
			
			m_InsertPhotoAction = new QAction(Gui.LoadIcon("insert-image", 16), "Photo...", this);
			QObject.Connect(m_InsertPhotoAction, Qt.SIGNAL("triggered()"), HandleInsertImageActionTriggered);
			insertMenu.AddAction(m_InsertPhotoAction);

			m_InsertLinkAction = new QAction(Gui.LoadIcon("insert-link", 16), "Link...", this);
			QObject.Connect(m_InsertLinkAction, Qt.SIGNAL("triggered()"), HandleInsertLinkActionTriggered);
			insertMenu.AddAction(m_InsertLinkAction);
			
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
			
			m_InviteToMucAction = new QAction(Gui.LoadIcon("internet-group-chat", 16), "Invite to Conference...", this);
			QObject.Connect(m_InviteToMucAction, Qt.SIGNAL("triggered()"), HandleInviteToMucActionTriggered);		
			activitiesMenu.AddAction(m_InviteToMucAction);
			activitiesMenu.AddSeparator();
			
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

						if (chatHandler.IsMucMessage) {
							toWidgetAction.Visible = false;
						} else {
							string title = null;
							if (handler.Account.PresenceManager[pres.From.BareJID] == null) {
								title = String.Format("{0} (Offline)", chatHandler.Account.GetDisplayName(chatHandler.Jid));
							} else {
								title = chatHandler.Account.GetDisplayName(chatHandler.Jid);	
							}
							Gui.TabbedChatsWindow.SetTabTitle(this, title);
						}
					});
				};
				
				foreach (var presence in chatHandler.Account.PresenceManager.GetAll(chatHandler.Jid)) {
					if (presence.Priority != "-1" && !String.IsNullOrEmpty(presence.From.Resource)) {
						string text = String.Format("{0} ({1})", Helper.GetResourceDisplay(presence), Helper.GetPresenceDisplay(presence));
						m_ToComboBox.AddItem(text, presence.From.Resource);
					}
				}			
				
				// FIXME: Make this a menu with "View Profile" and "View History".
				var viewProfileAction = new QAction(Gui.LoadIcon("info", 16), "View Profile", this);
				QObject.Connect(viewProfileAction, Qt.SIGNAL("triggered()"), HandleViewProfileActionTriggered);
				toolbar.AddAction(viewProfileAction);
			} else {
				toWidgetAction.Visible = false;
			}
			
			QObject.Connect<bool>(m_ConversationWidget.Page(), Qt.SIGNAL("loadFinished(bool)"), delegate (bool ok) {
				if (!ok) {
					throw new Exception("Failed to load chat html.");
				}
				handler.NewContent += HandleNewContent;
				m_Handler.FireQueued();
			});
			
			var settings = ServiceManager.Get<SettingsService>();
			m_ConversationWidget.ShowHeader = settings.Get<bool>("MessageShowHeader");
			m_ConversationWidget.ShowUserIcons = settings.Get<bool>("MessageShowAvatars");
			m_ConversationWidget.LoadTheme(settings.Get<string>("MessageTheme"), settings.Get<string>("MessageThemeVariant"));
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

		void HandleItemActivated (AvatarGrid<RoomParticipant> grid, RoomParticipant participant)
		{
			if (!String.IsNullOrEmpty(participant.RealJID))
				Gui.TabbedChatsWindow.StartChat(m_Handler.Account, participant.RealJID, false);	
			else
				Gui.TabbedChatsWindow.StartChat(m_Handler.Account, participant.NickJID, true);	
		}
		
		void HandleReadyChanged (object o, EventArgs args)
		{
			textEdit.Enabled = (m_Handler != null && m_Handler.Ready);
		}
		
		void HandleNewContent (IChatHandler handler, AbstractChatContent content)
		{
			if (content is ChatContentTyping) {
				var typingContent = (ChatContentTyping)content;
				if (m_Handler is ChatHandler) {
					var chatHandler = (ChatHandler)m_Handler;
					if (!chatHandler.IsMucMessage) {
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
				}
			} else {
				bool isSimilar   = (!(content is ChatContentStatus)) && m_PreviousContent != null && content.IsSimilarToContent(m_PreviousContent);
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
				string html = textEdit.ToHtml();			
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
		
		void HandleGridModeActionTriggered ()
		{
			participantsGrid.ListMode = false;
		}
		
		void HandleListModeActionTriggered ()
		{
			participantsGrid.ListMode = true;
		}
		
		void HandleViewProfileActionTriggered ()
		{
			var window = new ProfileWindow(m_Handler.Account, ((ChatHandler)m_Handler).Jid);
			window.Show();
		}

		void HandleFormatMenuActionTriggered (QAction action)
		{
			var cursor = textEdit.TextCursor();

			QTextCharFormat fmt = new QTextCharFormat();

			if (action == m_ClearFormattingAction) {
				cursor.SetCharFormat(fmt);
				textEdit.SetCurrentCharFormat(fmt);
				return;
			}

			if (action == m_BoldAction)
				fmt.SetFontWeight(action.Checked ? (int)QFont.Weight.Bold : (int)QFont.Weight.Normal);
			else if (action == m_ItalicAction)
				fmt.SetFontItalic(action.Checked);
			else if (action == m_UnderlineAction)
				fmt.SetFontUnderline(action.Checked);
			else if (action == m_StrikethroughAction)
				fmt.SetFontStrikeOut(action.Checked);

			cursor.MergeCharFormat(fmt);
			textEdit.MergeCurrentCharFormat(fmt);
		}

		void HandleInsertImageActionTriggered ()
		{
			var dialog = new QFileDialog(this.TopLevelWidget(), "Select Photo");
			dialog.fileMode = QFileDialog.FileMode.ExistingFile;
			if (dialog.Exec() == (int)QFileDialog.DialogCode.Accepted && dialog.SelectedFiles().Count > 0) {
				string fileName = dialog.SelectedFiles()[0];
				textEdit.InsertImage(fileName);
			}
		}
		
		void HandleInsertLinkActionTriggered ()
		{
			var dialog = new InsertLinkDialog(this.TopLevelWidget());
			if (dialog.Exec() == (int)QDialog.DialogCode.Accepted) {
				textEdit.InsertLink(dialog.Text, dialog.Url);
			}
		}
		
		void HandleInviteToMucActionTriggered ()
		{
			var dialog = new InviteToMucDialog(m_Handler, this.TopLevelWidget());
			dialog.Exec();
		}

		void HandleMucViewProfileActionTriggered ()
		{
			var jid = !String.IsNullOrEmpty(m_SelectedParticipant.RealJID) ? m_SelectedParticipant.RealJID : m_SelectedParticipant.NickJID;
			var window = new ProfileWindow(m_Handler.Account, jid);
			window.Show();
		}
		
		void HandleMucPrivateMessageTriggered ()
		{
			if (!String.IsNullOrEmpty(m_SelectedParticipant.RealJID))
				Gui.TabbedChatsWindow.StartChat(m_Handler.Account, m_SelectedParticipant.RealJID, false);	
			else
				Gui.TabbedChatsWindow.StartChat(m_Handler.Account, m_SelectedParticipant.NickJID, true);
		}
		
		void HandleMucSendFileActionTriggered ()
		{
			// FIXME: Not Implemented...
		}		
		
		void HandleMucViewHistoryActionTriggered ()
		{
			// FIXME: Not Implemented...
		}
		
		[Q_SLOT]
		void HandleRoomRoleActionGroupTriggered (QAction action)
		{
			var room = ((MucHandler)m_Handler).Room;
			if (action.Text == "Moderator" && m_SelectedParticipant.Role != RoomRole.moderator) {
				room.ChangeRole(m_SelectedParticipant.Nick, RoomRole.moderator, null);
			} else if (action.Text == "Participant" && m_SelectedParticipant.Role != RoomRole.participant) {
				room.ChangeRole(m_SelectedParticipant.Nick, RoomRole.participant, null);
			} else if (action.Text == "Visitor" && m_SelectedParticipant.Role != RoomRole.visitor) {
				room.ChangeRole(m_SelectedParticipant.Nick, RoomRole.visitor, null);
			}				
		}
		
		void HandleMucKickActionTriggered ()
		{
			// FIXME: Show dialog asking for reason
			((MucHandler)m_Handler).Room.Kick(m_SelectedParticipant.Nick, null);
		}
		
		void HandleMucBanActionTriggered ()
		{
			// FIXME: Show dialog asking for reason
			((MucHandler)m_Handler).Room.Ban(m_SelectedParticipant.RealJID, null);
		}
		
		void HandleChangeAffiliationTriggered ()
		{
			var mucHandler = (MucHandler)m_Handler;
			var dialog = new MucAffiliationDialog(mucHandler.Room, m_SelectedParticipant, base.TopLevelWidget());
			dialog.Exec();
		}
		
		void HandleMenuAboutToShow ()
		{
			participantsGrid.SuppressTooltips = true;
		}
		
		void HandleMenuAboutToHide ()
		{
			participantsGrid.SuppressTooltips = false;
		}
		
		[Q_SLOT]
		void on_textEdit_currentCharFormatChanged (QTextCharFormat format)
		{
			var font = format.Font();
			m_BoldAction.Checked = font.Bold();
			m_ItalicAction.Checked = font.Italic();
			m_UnderlineAction.Checked = font.Underline();
			m_StrikethroughAction.Checked = font.StrikeOut();
		}
		
		[Q_SLOT]
		void on_participantsGrid_customContextMenuRequested(QPoint point)
		{
			if (participantsGrid.HoverItem != null) {
				
				var mucHandler = (MucHandler)m_Handler;
				
				m_SelectedParticipant = participantsGrid.HoverItem;
				
				var me = mucHandler.Room.Participants[mucHandler.Room.RoomAndNick];
				
				bool isModerator = me.Role == RoomRole.moderator;
				bool isAdminOrOwner = me.Affiliation == RoomAffiliation.admin || me.Affiliation == RoomAffiliation.owner;	
								
				m_ModeratorActionsMenu.Enabled = isModerator || isAdminOrOwner;
				m_ChangeAffiliationAction.Enabled = isAdminOrOwner;
				
				m_ModeratorAction.Checked   = (m_SelectedParticipant.Role == RoomRole.moderator);
				m_VisitorAction.Checked     = (m_SelectedParticipant.Role == RoomRole.visitor);
				m_ParticipantAction.Checked = (m_SelectedParticipant.Role == RoomRole.participant);
				
				m_AddAsFriendAction.Enabled = !String.IsNullOrEmpty(participantsGrid.HoverItem.RealJID);
				
				m_ParticipantItemMenu.Popup(participantsGrid.MapToGlobal(point));
			} else {
				m_ParticipantsMenu.Popup(participantsGrid.MapToGlobal(point));
			}
		}
	}
}
