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
using Qyoto;
using Synapse.ServiceStack;
using Synapse.UI.Services;
using Synapse.UI.Views;
using Synapse.UI.Controllers;
using Synapse.UI;
using Synapse.UI.Actions.ExtensionNodes;
using Synapse.QtClient;
using jabber.connection;
using Mono.Addins;

public partial class ChatWindow : QWidget, IChatWindowView
{
	public event TextEventHandler TextEntered;
	public event EventHandler Closed;
	public event EventHandler UrgencyHintChanged;
	
	bool m_UrgencyHint = false;

	public ChatWindow (AbstractChatWindowController controller)
	{
		SetupUi();

		if (controller is MucWindowController) {
			var mucController = (MucWindowController)controller;
			participantsGrid.Model = mucController.GridModel;		
			m_ConversationWidget.ChatName = mucController.Room.JID;
			this.WindowTitle = mucController.Room.JID;
		} else if (controller is ChatWindowController) {
			var chatController = (ChatWindowController)controller;
			participantsGrid.Hide();
			this.WindowTitle = chatController.Jid.User; //FIXME: Show nickname from roster?
			this.WindowIcon = new QIcon(new QPixmap(String.Format("avatar:/{0}", Synapse.Xmpp.AvatarManager.GetAvatarHash(chatController.Jid.Bare))));
		}
	
		KeyPressEater eater = new KeyPressEater(this);
		eater.KeyEvent += HandleKeyEvent;
		textEdit.InstallEventFilter(eater);

		QToolBar toolbar = new QToolBar(this);
		toolbar.SetToolButtonStyle(Qt.ToolButtonStyle.ToolButtonIconOnly);
		
		QAction boldAction = new QAction(Helper.LoadIcon("format-text-bold", 16), "Bold", this);
		toolbar.AddAction(boldAction);
		
		QAction italicAciton = new QAction(Helper.LoadIcon("format-text-italic", 16), "Italic", this);
		toolbar.AddAction(italicAciton);
		
		QAction underlineAction = new QAction(Helper.LoadIcon("format-text-underline", 16), "Underline", this);
		toolbar.AddAction(underlineAction);

		foreach (IActionItemCodon node in AddinManager.GetExtensionNodes("/Synapse/UI/ChatWindow/FormattingToolbar")) {
			toolbar.AddAction((QAction)node.CreateInstance(this));
		}		
		
		((QVBoxLayout)bottomContainer.Layout()).InsertWidget(0, toolbar);
		
		m_ConversationWidget.LoadTheme("Mockie", "Orange - Icon Left");
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

	bool HandleKeyEvent(QKeyEvent kevent)
	{
		if ((kevent.Modifiers() & (uint)Qt.KeyboardModifier.ControlModifier) == 0 && kevent.Key() == (int)Qt.Key.Key_Return || kevent.Key() == (int)Qt.Key.Key_Enter) {
			if (TextEntered != null)
				TextEntered(textEdit.PlainText);
			textEdit.Clear();
			return true;
		} else {
			return false;
		}
	}

	public void AppendStatus (string status, string message)
	{
		m_ConversationWidget.AppendStatus(status, message);
	}
	
	public void AppendMessage(bool incoming, bool next, string userIconPath, string senderScreenName, string sender,
	                          string senderColor, string senderStatusIcon, string senderDisplayName,
	                          string message)
	{
		m_ConversationWidget.AppendMessage(incoming, next, userIconPath, senderScreenName, sender, senderColor,
		                                   senderStatusIcon, senderDisplayName, Qt.Escape(message));

		if (!IsActive) {
			UrgencyHint = true;
		}
	}

	protected override void CloseEvent(QCloseEvent evnt)
	{
		if (Closed != null)
			Closed(this, EventArgs.Empty);
		evnt.Accept();
	}


	protected override void FocusInEvent (Qyoto.QFocusEvent arg1)
	{
		base.FocusInEvent (arg1);
		
		textEdit.SetFocus();
		UrgencyHint = false;
	}

	bool IsActive {
		get {
			var gui = ServiceManager.Get<GuiService>();
			return (gui.ChatsWindow.View.IsActiveWindow && gui.ChatsWindow.View.CurrentChat == this);
		}
	}
}