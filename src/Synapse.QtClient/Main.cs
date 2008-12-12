// Main.cs
//
// Authors:
//   Eric Butler <eric@extremeboredom.net>
//
// Copyright (c) 2008 Eric Butler
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

using Hyena;
using Mono.Addins;

using Synapse.ServiceStack;
using Synapse.Core;
using Synapse.UI.Services;

using Qyoto;
using QtWebKit;

namespace Synapse.QtClient
{
	using System.Collections.Generic;
	
	public class Client : Synapse.ServiceStack.Client
	{
		ChatWindowContainerWindow m_ChatWindowContainerWindow;
		
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
			PlatformHacks.SetProcessName("synapse");

			Gtk.Application.Init();

			m_App = new QApplication(args);
			InitQtWebKit.InitSmoke();

			m_ResourceFileEngineHandler = new ResourceFileEngineHandler();
			m_AvatarFileEngineHandler = new AvatarFileEngineHandler();
			
			QtTraceListener listener = new QtTraceListener();
			listener.TraceOutputOptions = TraceOptions.Callstack;
			Debug.Listeners.Add(listener);
			Debug.WriteLine("Debug Mode On");
			
			Application.Initialize();
			
			Application.IdleHandler = delegate (IdleHandler callback) {
				if (Thread.CurrentThread.ManagedThreadId != 1) {
					QCoreApplication.Invoke(delegate {
						callback();
					});
				} else {
					callback();
				}
				return 0;
			};

			AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) {
				Console.Error.WriteLine("UNHANDLED EXCEPTION: " + e.ExceptionObject);
				/*
				ManualResetEvent mutex = new ManualResetEvent(false);
				Application.Invoke(delegate {
					QApplication.Quit();
					new QApplication(new string[0]);
					try {
					QMessageBox.Critical(null, "Unhandled Exception", e.ExceptionObject.ToString(), 
					                     (uint)QMessageBox.StandardButton.Abort);
					mutex.Set();
					Thread.CurrentThread.Abort();
					} catch (Exception eee) { Console.Error.WriteLine("WTF !!!! " + eee); }
				});
				mutex.WaitOne();
				*/
				Environment.Exit(-1);
			};
			
			// XXX: I dont like this being here.
			ServiceManager.RegisterService<Synapse.Xmpp.AccountService>();
			ServiceManager.RegisterService<Synapse.Xmpp.OperationService>();
			
			ServiceManager.RegisterService<GuiService>();
			ServiceManager.RegisterService<ActionService>();

			Application.FileImageCreatorHandler = delegate (string fileName) {
				return (object) new QPixmap(fileName);
			};

			Application.DataImageCreatorHandler = delegate (byte[] data) {
				throw new NotImplementedException();
			};

			Application.ActionCreatorHandler += delegate (string id, string label, string icon, object parent) {
				if (id == null) {
					QAction action = new QAction(null);
					action.SetSeparator(true);
					return action;
				} else {
					QAction action = new QAction(Helper.LoadIcon(icon, 16), label, (QObject)parent);
					QObject.Connect(action, Qt.SIGNAL("triggered(bool)"), delegate (bool chkd) {
						ServiceManager.Get<ActionService>().TriggerAction(id, action);
					});
					return action;
				}
			};
			
			QWebSettings.GlobalSettings().SetAttribute(QWebSettings.WebAttribute.DeveloperExtrasEnabled, true);
			
			string themesDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, "themes");
			ConversationWidget.ThemesDirectory = themesDirectory;
			
			Application.PushClient(this);
			Application.Run();
				
			Application.Invoke(delegate {
				OnStarted();

				// FIXME: This seems out of place here:
				m_ChatWindowContainerWindow = new ChatWindowContainerWindow();
			});

			QApplication.Exec();
		}
		
		public override string ClientId {
			get { return "qtclient"; }
		}

		public override void Dispose ()
		{
			QCoreApplication.Quit();
		}
	}
}