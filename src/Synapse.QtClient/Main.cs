//
// Main.cs
//
// Copyright (C) 2008-2009 Eric Butler
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
using System.Diagnostics;
using System.Threading;
using System.IO;

using Hyena;

using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Services;
using Synapse.UI.Services;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;

using Qyoto;
using QtWebKit;

using Notifications;

using Synapse.QtClient.Windows;

namespace Synapse.QtClient
{
	using System.Collections.Generic;
	
	public class Client : Synapse.ServiceStack.Client
	{
		public static void Main (string[] args)
		{
			new QtClient.Client(args);
		}

		QApplication m_App;
		ResourceFileEngineHandler m_ResourceFileEngineHandler;
		AvatarFileEngineHandler m_AvatarFileEngineHandler;
		
		public Client (string[] args)
		{	
			Log.Information ("Starting Synapse");
			
			try {
				PlatformHacks.SetProcessName("synapse");
			} catch (Exception ex) {
				Console.WriteLine("[WARNING] Failed to set process name: " + ex.Message);
			}
			
			GLib.Global.ProgramName = "Synapse";
			Gtk.Application.Init();
			
			m_App = new QApplication(args);
			m_App.QuitOnLastWindowClosed = false;
			m_App.ApplicationName = "Synapse";
			m_App.ApplicationVersion = "0.1";
			
			InitQtWebKit.InitSmoke();

			m_ResourceFileEngineHandler = new ResourceFileEngineHandler();
			m_AvatarFileEngineHandler = new AvatarFileEngineHandler();
			
			Application.Initialize(this);

			if (Application.Debugging) {			
				QtTraceListener listener = new QtTraceListener();
				listener.TraceOutputOptions = TraceOptions.Callstack;
				Debug.Listeners.Add(listener);
				Debug.Listeners.Add(new ConsoleTraceListener());
				Debug.WriteLine("Debug Mode On");
			}
			
			AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
			
			if (!Application.CommandLine.Contains("disable-dbus")) {
				try {
					NDesk.DBus.BusG.Init();
				} catch (Exception ex) {
					Console.Error.WriteLine("Failed to initialize DBUS: " + ex);
				}
			} else {
				Console.WriteLine("DBus disabled by request.");
			}
			
			// XXX: I dont like all of these being here.
			ServiceManager.RegisterService<Synapse.Xmpp.Services.XmppService>();
			ServiceManager.RegisterService<Synapse.Xmpp.Services.AccountService>();
			ServiceManager.RegisterService<Synapse.QtClient.OctyService>();
			ServiceManager.RegisterService<Synapse.Xmpp.Services.ShoutService>();
			ServiceManager.RegisterService<Synapse.Xmpp.Services.GeoService>();
			ServiceManager.RegisterService<GuiService>();
			
			QWebSettings.GlobalSettings().SetAttribute(QWebSettings.WebAttribute.DeveloperExtrasEnabled, true);
			QWebSettings.GlobalSettings().SetAttribute(QWebSettings.WebAttribute.PluginsEnabled, true);
			
			if (Application.CommandLine.Contains ("uninstalled"))
				ConversationWidget.ThemesDirectory = Path.Combine(Environment.CurrentDirectory, "themes");
			else
				ConversationWidget.ThemesDirectory = Path.Combine(Paths.InstalledApplicationData, "themes");
			
			Application.Run();
				
			QApplication.Invoke(delegate {
				/* Create the UI */
				Gui.MainWindow = new MainWindow();
				Gui.TrayIcon = new TrayIcon(m_App);
				Gui.TabbedChatsWindow = new TabbedChatsWindow();
				
				OnStarted();

				Gui.TrayIcon.Show();

				Gui.MainWindow.Show();
			});

			QApplication.Exec();
		}

		void HandleUnhandledException(object sender, UnhandledExceptionEventArgs args)
		{
			Exception ex = (Exception)args.ExceptionObject;
			Console.Error.WriteLine("UNHANDLED EXCEPTION: " + ex);
			string desktopPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
			string crashFileName = Path.Combine(desktopPath, String.Format("synapse-crash-{0}.log", DateTime.Now.ToFileTime()));
			string crashLog = args.ExceptionObject.ToString();
			Util.WriteToFile(crashFileName, crashLog);

			// FIXME: Figure out how to show a damn error dialog.
			
			QCoreApplication.Quit();
		}

		public QApplication QApp {
			get {
				return m_App;
			}
		}
		
		public override string ClientId {
			get { return "qtclient"; }
		}

		public override object CreateImage (string fileName)
		{
			return (object) new QPixmap(fileName);
		}

		public override object CreateImage (byte[] data)
		{
			throw new NotImplementedException();
		}
		
		public override void DesktopNotify (ActivityFeedItemTemplate template, IActivityFeedItem item, string text)
		{
			// FIXME: This will need to be different on windows/osx...
			QApplication.Invoke(delegate {
				Notification notif = new Notification(text, item.Content);
				foreach (var action in template.Actions) {
					notif.AddAction(action.Name, action.Label, delegate {
						item.TriggerAction(action.Name);
					});
				}			
				notif.Show ();
			});
		}

		public override void ShowErrorWindow (string errorTitle, string errorMessage, string errorDetail)
		{
			QApplication.Invoke(delegate {
				Gui.ShowErrorWindow(errorTitle, errorMessage, errorDetail);
			});
		}
			
		public override void Dispose ()
		{
			// FIXME: This isn't enough... so we have to resort to more drastic measures below.
			// Likely Qyoto bug.
			QCoreApplication.Quit();			
			System.Diagnostics.Process.GetCurrentProcess().Kill();
		}
	}
}
