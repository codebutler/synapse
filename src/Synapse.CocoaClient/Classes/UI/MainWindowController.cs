//
// MainWindowController.cs
//
// Copyright (C) 2010 Eric Butler
//
// Authors:
//   Eric Butler <eric@codebutler.com>
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
using System.Linq;
using System.Collections.Generic;

using Monobjc;
using Monobjc.Cocoa;

using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;

using jabber;

namespace Synapse.CocoaClient
{
	[ObjectiveCClass]
	public class MainWindowController : NSWindowController
	{
		[ObjectiveCField] public NSView welcomeView;
		[ObjectiveCField] public NSView rosterView;
		
		[ObjectiveCField] public NSWindow addAccountWindow;
		[ObjectiveCField] public NSTextField loginField;
		[ObjectiveCField] public NSTextField passwordField;
		
		List<AccountStatusViewController> m_AccountViews = new List<AccountStatusViewController>();
		
		public MainWindowController ()
		{
		}
		
		public MainWindowController (IntPtr nativePointer) : base (nativePointer)
		{
		}
		
		[ObjectiveCMessage("awakeFromNib")]
		public void AwakeFromNib ()
		{
			var notificationCenter = NSNotificationCenter.DefaultCenter;
			notificationCenter.AddObserverSelectorNameObject
				(this,
				 "windowWillClose:".ToSelector(),
			     NSWindow.NSWindowWillCloseNotification,
			     base.Window);
		}
		
		public void FinishLoading ()
		{
			base.Window.BackgroundColor = NSColor.BlackColor;
			
			var accountService = ServiceManager.Get<AccountService>();
			accountService.AccountAdded   += AccountAdded;
			accountService.AccountRemoved += AccountRemoved;
			
			foreach (Account account in accountService.Accounts)
				AccountAdded(account);
		
			ToggleWelcomeView();
		}
		
		[ObjectiveCMessage("addAccountClicked:")]
		public void AddAccountClicked (Id sender)
		{
			NSApplication.NSApp.BeginSheetModalForWindowModalDelegateDidEndSelectorContextInfo
				(addAccountWindow, base.Window, null, IntPtr.Zero); 
		}
		
		[ObjectiveCMessage("cancelAddAccountClicked:")]
		public void CancelAddAccountClicked (Id sender)
		{
			addAccountWindow.OrderOut(this);
			NSApplication.NSApp.EndSheet(addAccountWindow);
		}
		
		[ObjectiveCMessage("saveAccountClicked:")]
		public void SaveAccountClicked (Id sender)
		{
			JID jid = null;
		
			string login = loginField.StringValue;
			string password = passwordField.StringValue;
			
			try {
				if (String.IsNullOrEmpty(login))
					throw new Exception("Login may not be empty.");
				else if (!JID.TryParse(login, out jid) || String.IsNullOrEmpty(jid.User))
					throw new Exception("Login should look like 'user@server'.");
				else if (String.IsNullOrEmpty(password))
					throw new Exception("Password may not be empty.");
			} catch (Exception ex) {
				var alert = NSAlert.AlertWithMessageTextDefaultButtonAlternateButtonOtherButtonInformativeTextWithFormat
					("Error", "OK", null, null, ex.Message, new object[] {});
				alert.RunModal();
				return;
			}		
			
			var accountInfo = new AccountInfo(jid.User, jid.Server, password, "Synapse");
			var accountService = ServiceManager.Get<AccountService>();
			accountService.AddAccount(accountInfo);
			
			CancelAddAccountClicked(this);
			loginField.StringValue = String.Empty;
			passwordField.StringValue = String.Empty;
		}
		
		[ObjectiveCMessage("windowWillClose:")]
		public void WindowWillClose (Id sender)
		{
			if (Synapse.ServiceStack.Application.ShuttingDown)
				return;
			
			var accountService = ServiceManager.Get<AccountService>();
			if (accountService.Accounts.Count() == 0) {
				Synapse.ServiceStack.Application.Shutdown();
			}
		}
		
		void AccountAdded (Account account)
		{
			ToggleWelcomeView();
			
			var viewController = new AccountStatusViewController(account);
			m_AccountViews.Add(viewController);
			
			base.Window.ContentView.AddSubview(viewController.View);
		}
		
		void AccountRemoved (Account account)
		{
			ToggleWelcomeView();
		}
		
		void ToggleWelcomeView ()
		{
			var contentView = base.Window.ContentView;
			
			var accountService = ServiceManager.Get<AccountService>();
			if (accountService.Accounts.Count() == 0) {
				if (rosterView.IsDescendantOf(contentView))
					rosterView.RemoveFromSuperview();
				
				if (!welcomeView.IsDescendantOf(contentView))
					contentView.AddSubview(welcomeView);
				
				base.Window.ContentMaxSize = new NSSize(383, Single.MaxValue);
			} else {
				if (welcomeView.IsDescendantOf(contentView))
					welcomeView.RemoveFromSuperview();
				
				if (!rosterView.IsDescendantOf(contentView))
					contentView.AddSubview(rosterView);
				
				base.Window.ContentMaxSize = new NSSize(Single.MaxValue, Single.MaxValue);
			}
		}
	}
}
