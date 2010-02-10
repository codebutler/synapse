//
// AppDelegate.cs
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

using System;
using Hyena;
using Monobjc;
using Monobjc.Cocoa;
using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Services;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;

namespace Synapse.CocoaClient
{
	[ObjectiveCClass]
	public class AppDelegate : NSObject, IClient
	{
		bool m_Started = false;
		NSStatusItem m_StatusItem;
		
		[ObjectiveCField] public MainWindowController mainWindowController;
		
		public AppDelegate ()
		{
			Load();
		}
		
		public AppDelegate (IntPtr nativePointer) : base(nativePointer)
		{
			Load();
		}
		
		void Load ()
		{
			Console.WriteLine("A");
			
			Synapse.ServiceStack.Application.Initialize(this);	
			
			// FIXME: I don't like all of these being here.
			ServiceManager.RegisterService<XmppService>();
			ServiceManager.RegisterService<AccountService>();
			ServiceManager.RegisterService<ShoutService>();
			ServiceManager.RegisterService<GeoService>();
			// OctyService, GuiService
		
			Synapse.ServiceStack.Application.Run();
		}

		[ObjectiveCMessage("applicationDidFinishLaunching:")]
		public void ApplicationDidFinishLaunching (NSNotification notification)
		{
			// Create menu icon
			var statusBar = NSStatusBar.SystemStatusBar;
			m_StatusItem = statusBar.StatusItemWithLength(-2).Retain<NSStatusItem>();
			m_StatusItem.Image = NSImage.ImageNamed("octy-22.png");
			
			// Show the main window!
			mainWindowController.FinishLoading();
			mainWindowController.ShowWindow(this);
			
			m_Started = true;
			Log.InformationFormat ("{0} Client Started", ClientId);
			var handler = Started;
			if (handler != null)
				Started(this);
		}
		
		#region IClient implementation
		public event Action<IClient> Started;
		
		public event NetworkStateChangeHandler NetworkStateChanged;
		
		public object CreateImage (byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException("data");
			
			var nsData = new NSData(data).Autorelease<NSData>();
			return new NSImage(nsData).Autorelease<NSImage>();
		}		
		
		public object CreateImage (string fileName)
		{
			if (String.IsNullOrEmpty(fileName))
				throw new ArgumentNullException("fileName");
			
			return new NSImage(fileName).Autorelease<NSImage>();
		}		
		
		public object CreateImageFromResource (string resourceName)
		{
			return NSImage.ImageNamed(resourceName);
		}
		
		public void DesktopNotify (ActivityFeedItemTemplate template, IActivityFeedItem item, string text)
		{
			// FIXME: Growl!!
			throw new System.NotImplementedException();
		}		
		
		public void ShowErrorWindow (string errorTitle, Exception error)
		{
			if (error != null)
				ShowErrorWindow(errorTitle, error.Message, error.ToString());
			else
				ShowErrorWindow(errorTitle, null, null);
		}		
		
		public void ShowErrorWindow (string title, string errorMessage, string errorDetail)
		{
			throw new System.NotImplementedException();
		}
		
		public string ClientId {
			get { return "Cocoa"; }
		}
		
		public bool IsStarted {
			get { return m_Started; }
		}
		
		public void Exit ()
		{
			NSApplication.NSApp.Terminate(this);
		}
		
		public override void Release ()
		{
			m_StatusItem.Release();
			base.Release ();
		}
		
		#endregion		
	}
}
