//
// GtkClient.cs
// 
// Copyright (C) 2010 Eric Butler
//
// Authors:
//   Eric Butler <eric@codebutler.com>
//   Christian Hergert <chris@dronelabs.com>
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

using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Services;
using Synapse.Xmpp.Services;
using Synapse.GtkClient.Windows;

namespace Synapse.GtkClient
{
	class GtkClient : IClient
	{
		MainWindow m_MainWindow;
		bool m_Started;
		
		public static void Main (string[] args)
		{
			Gtk.Application.Init();
			Clutter.Application.Init();
			
			new GtkClient();
			
			Gtk.Application.Run();
		}
		
		public GtkClient ()
		{
			Synapse.ServiceStack.Application.Initialize(this);
						
			// FIXME: I don't like all of these being here.
			ServiceManager.RegisterService<XmppService>();
			ServiceManager.RegisterService<AccountService>();
			ServiceManager.RegisterService<ShoutService>();
			ServiceManager.RegisterService<GeoService>();
			// OctyService, GuiService
			
			Gtk.Application.Invoke(FinishLoading);		
		
			Synapse.ServiceStack.Application.Run();
		}
		
		void FinishLoading (object o, EventArgs args)
		{
			// FIXME: Create tray icon
			
			m_MainWindow = new MainWindow();	
			m_MainWindow.Show();
			
			m_Started = true;
			var handler = Started;
			if (handler != null)
				Started(this);
		}
		
		#region IClient implementation
		public event Action<IClient> Started;
		
		public object CreateImage (byte[] data)
		{
			return new Gdk.Pixbuf(data);
		}
		
		public object CreateImage (string fileName)
		{
			return new Gdk.Pixbuf(fileName);
		}
		
		public object CreateImageFromResource (string resourceName)
		{
			return new Gdk.Pixbuf(null, resourceName);
		}
		
		public void ShowErrorWindow (string title, string errorMessage, string errorDetail)
		{
			throw new System.NotImplementedException();
		}
		
		public void ShowErrorWindow (string errorTitle, Exception error)
		{
			ShowErrorWindow(errorTitle, error.Message, error.ToString());
		}
				
		public void DesktopNotify (Synapse.Services.ActivityFeedItemTemplate template, Synapse.Services.IActivityFeedItem item, string text)
		{
			throw new System.NotImplementedException();
		}
		
		public void Exit ()
		{
			Gtk.Application.Quit();
		}
		
		public string ClientId {
			get { return "Gtk"; }
		}
		
		public bool IsStarted {
			get {
				return m_Started;
			}
		}		
		#endregion
	}
}
