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
using Synapse.UI.Services;
using Synapse.UI;
using Synapse.UI.Actions.ExtensionNodes;
using Synapse.UI.Chat;
using Synapse.Xmpp;
using Synapse.QtClient;
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
				m_ConversationWidget.ChatName = mucHandler.Room.JID;
				this.WindowTitle = mucHandler.Room.JID;
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
	
			m_BoldAction = new QAction(Gui.LoadIcon("format-text-bold", 16), "Bold", this);
			m_BoldAction.Shortcut = "Ctrl+B";
			m_BoldAction.Checkable = true;
			toolbar.AddAction(m_BoldAction);
			
			m_ItalicAction = new QAction(Gui.LoadIcon("format-text-italic", 16), "Italic", this);
			m_ItalicAction.Shortcut = "Ctrl+I";
			m_ItalicAction.Checkable = true;
			toolbar.AddAction(m_ItalicAction);
			
			m_UnderlineAction = new QAction(Gui.LoadIcon("format-text-underline", 16), "Underline", this);
			m_UnderlineAction.Shortcut = "Ctrl+U";
			m_UnderlineAction.Checkable = true;
			toolbar.AddAction(m_UnderlineAction);
	
			m_StrikethroughAction = new QAction(Gui.LoadIcon("format-text-strikethrough", 16), "Strikethrough", this);
			m_StrikethroughAction.Shortcut = "Ctrl+S";
			m_StrikethroughAction.Checkable = true;
			toolbar.AddAction(m_StrikethroughAction);
			
			foreach (IActionItemCodon node in AddinManager.GetExtensionNodes("/Synapse/UI/ChatWindow/FormattingToolbar")) {
				toolbar.AddAction((QAction)node.CreateInstance(this));
			}		
			
			var spacerWidget = new QWidget(toolbar);
			spacerWidget.SetSizePolicy(QSizePolicy.Policy.Expanding, QSizePolicy.Policy.Fixed);
			toolbar.AddWidget(spacerWidget);

			var toContainer = new QWidget(toolbar);
			var layout = new QHBoxLayout(toContainer);
			layout.SetContentsMargins(0, 0, 4, 0);

			layout.AddWidget(new QLabel("To:", toContainer));
			layout.AddWidget(new QComboBox(toContainer));
			
			toolbar.AddWidget(toContainer);

			((QVBoxLayout)bottomContainer.Layout()).InsertWidget(0, toolbar);
			
			m_ConversationWidget.LoadTheme("Mockie", "Orange - Icon Left");

			handler.Start();
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
			bool isSimilar   = m_PreviousContent != null && content.IsSimilarToContent(m_PreviousContent);
			//bool replaceLast = m_PreviousContent is ChatContentStatus && 
			//	               content is ChatContentStatus && 
			//	               ((ChatContentStatus)m_PreviousContent).CoalescingKey == ((ChatContentStatus)content).CoalescingKey;
			bool replaceLast = m_PreviousContent is ChatContentTyping;
			
			m_PreviousContent = content;
			
			Application.Invoke(delegate {
				m_ConversationWidget.AppendContent(content, isSimilar, false, replaceLast);	
				if (content is ChatContentMessage && !IsActive) {
					UrgencyHint = true;
				}
			});
		}
		
		bool HandleKeyEvent(QKeyEvent kevent)
		{
			if ((kevent.Modifiers() & (uint)Qt.KeyboardModifier.ControlModifier) == 0 && kevent.Key() == (int)Qt.Key.Key_Return || kevent.Key() == (int)Qt.Key.Key_Enter) {
				// FIXME: Need to clean this HTML up...
				// string html = textEdit.Html;
				string html = textEdit.PlainText;
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
	}
}
