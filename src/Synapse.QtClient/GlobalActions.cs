
using System;

using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;
using Synapse.QtClient.Windows;

using Qyoto;

namespace Synapse.QtClient
{
	public class GlobalActions : QObject
	{
		QAction m_QuitAction;
		QAction m_ShowPreferencesAction;
		QAction m_SendFeedbackAction;
		QAction m_ShowBrowserAction;
		QAction m_NewMessageAction;
		QAction m_JoinMucAction;
		QAction m_ChangeStatusAction;
		QAction m_EditProfileAction;
		QAction m_AboutAction;
		QMenu m_AccountsMenu;		
		
		QMenu   m_PresenceMenu;
		QAction m_AvailableAction;
		QAction m_FreeToChatAction;
		QAction m_AwayAction;
		QAction m_ExtendedAwayAction;
		QAction m_DoNotDisturbAction;
		QAction m_OfflineAction;
		
		public GlobalActions()
		{
			m_AccountsMenu = new QMenu();
			
			m_QuitAction = new QAction(Gui.LoadIcon("application-exit"), "Quit", this);
			m_QuitAction.Shortcut = new QKeySequence("Ctrl+Q");
			QObject.Connect(m_QuitAction, Qt.SIGNAL("triggered()"), HandleQuitActionTriggered);
			
			m_ShowPreferencesAction = new QAction(Gui.LoadIcon("preferences-desktop"), "Preferences", this);
			QObject.Connect(m_ShowPreferencesAction, Qt.SIGNAL("triggered()"), HandleShowPreferencesActionTriggered);
			
			m_SendFeedbackAction = new QAction("Send Feedback...", this);
			QObject.Connect(m_SendFeedbackAction, Qt.SIGNAL("triggered()"), HandleSendFeedbackActionTriggered);
			
			m_ShowBrowserAction = new QAction(Gui.LoadIcon("system-search"), "Discover Services...", this);
			QObject.Connect(m_ShowBrowserAction, Qt.SIGNAL("triggered()"), HandleShowBrowserActionTriggered);
						
			m_NewMessageAction = new QAction(Gui.LoadIcon("document-new"), "New Message...", this);
			
			m_JoinMucAction = new QAction(Gui.LoadIcon("internet-group-chat"), "Create/Join Conference...", this);
			
			m_EditProfileAction = new QAction(Gui.LoadIcon("user-info"), "Edit Profile...", this);
			QObject.Connect(m_EditProfileAction, Qt.SIGNAL("triggered()"), HandleEditProfileActionTriggered);
			
			m_AboutAction = new QAction(Gui.LoadIcon("help-about"), "About", this);
			QObject.Connect(m_AboutAction, Qt.SIGNAL("triggered()"), HandleAboutActionTriggered);

			m_PresenceMenu = new QMenu();

			QActionGroup group = new QActionGroup(this);
			group.Exclusive = true;
			
			m_AvailableAction = m_PresenceMenu.AddAction("Available");
			group.AddAction(m_AvailableAction);
			m_AvailableAction.Checkable = true;
			
			m_FreeToChatAction = m_PresenceMenu.AddAction("Free To Chat");
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
			
			m_ChangeStatusAction = new QAction("Change Status", this);
			m_ChangeStatusAction.SetMenu(m_PresenceMenu);
		}
		
		public QAction QuitAction {
			get {
				return m_QuitAction;
			}
		}

		public QAction ShowPreferencesAction {
			get {
				return m_ShowPreferencesAction;
			}
		}
		
		public QAction SendFeedbackAction {
			get {
				return m_SendFeedbackAction;
			}
		}
		
		public QAction ShowBrowserAction {
			get {
				return m_ShowBrowserAction;	
			}
		}
		
		public QAction NewMessageAction {
			get {
				return m_NewMessageAction;
			}
		}
		
		public QAction JoinConferenceAction {
			get {
				return m_JoinMucAction;
			}
		}
		
		public QAction EditProfileAction {
			get {
				return m_EditProfileAction;
			}
		}
			
		public QAction ChangeStatusAction {
			get {
				return m_ChangeStatusAction;
			}
		}
		
		public QAction AboutAction {
			get {
				return m_AboutAction;
			}
		}
		
		void HandleQuitActionTriggered ()
		{
			Application.Shutdown();
		}
		
		void HandleShowPreferencesActionTriggered ()
		{
			Gui.ShowPreferencesWindow();
		}
		
		void HandleSendFeedbackActionTriggered ()
		{
			Util.Open("http://firerabbit.lighthouseapp.com/projects/23238-synapse/tickets/new");
		}
		
		void HandleShowBrowserActionTriggered ()
		{
			// FIXME: Using ShowAccountMenu here looks really bad.
			// Need to add the accounts into the menu under the action instead.
			var account = Gui.ShowAccountSelectMenu(null);				
			if (account != null) {
				var window = new ServiceBrowserWindow(account);
				window.Show();
			}
		}
		
		void HandleEditProfileActionTriggered ()
		{
			// FIXME: Using ShowAccountMenu here looks really bad.
			// Need to add the accounts into the menu under the action instead.
			var account = Gui.ShowAccountSelectMenu(null);				
			if (account != null) {
				var dialog = new EditProfileDialog(account, Gui.MainWindow);
				dialog.Show();
			}
		}
		
		void HandleAboutActionTriggered ()
		{
			var aboutDialog = new AboutDialog(Gui.MainWindow);
			aboutDialog.Show();
		}
	}
}
