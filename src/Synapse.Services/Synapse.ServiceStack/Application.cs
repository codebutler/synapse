//
// Application.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//   Eric Butler <eric@extremeboredom.net>
//
// Copyright (C) 2007 Novell, Inc.
//
// This file is based on code copied from Banshee - http://banshee-project.org/
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using Mono.Unix;
using Synapse.Core;
using Hyena;
using Hyena.CommandLine;

namespace Synapse.ServiceStack
{    
    public delegate bool ShutdownRequestHandler ();
	
    public static class Application
    {   
        public static event ShutdownRequestHandler ShutdownRequested;

		private static IClient s_Client;
        private static bool shutting_down;

        public static void Initialize (IClient client)
        {
			if (client == null)
				throw new ArgumentNullException("client");
			s_Client = client;
			
			Log.InformationFormat("Starting Synapse ({0})", client.ClientId);
			
			AppDomain.CurrentDomain.UnhandledException += HandleAppDomainCurrentDomainUnhandledException;
			
			try {
				PlatformHacks.SetProcessName("synapse");
			} catch (Exception ex) {
				Log.WarningFormat("Failed to set process name: {0}", ex.Message);
			}
				
			CommandLine = new CommandLineParser();
			
            ServiceManager.Initialize();
        }

        static void HandleAppDomainCurrentDomainUnhandledException (object sender, UnhandledExceptionEventArgs args)
        {
			Exception ex = (Exception)args.ExceptionObject;
			Console.Error.WriteLine("UNHANDLED EXCEPTION: " + ex);
			string desktopPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
			string crashFileName = Path.Combine(desktopPath, String.Format("synapse-crash-{0}.log", DateTime.Now.ToFileTime()));
			string crashLog = args.ExceptionObject.ToString();
			Util.WriteToFile(crashFileName, crashLog);

			s_Client.ShowErrorWindow("Unhandled Exception", ex);
			
			Shutdown();
        }

		public static IClient Client {
			get {
				return s_Client;
			}
		}

		public static CommandLineParser CommandLine {
    		get; set;
		}
		
        public static void Run ()
		{
			if (s_Client == null)
				throw new InvalidOperationException("Must call Initialize() first");
			
            ServiceManager.Run ();
		}
        
        private static bool? debugging = null;
        public static bool Debugging {
            get {
                if (debugging == null) {
                    debugging = CommandLine.Contains ("debug");
                    debugging |= EnvironmentIsSet ("SYNAPSE_DEBUG");
                }
                
                return debugging.Value;
            }
        }
        
        public static bool EnvironmentIsSet (string env)
        {
            return !String.IsNullOrEmpty (Environment.GetEnvironmentVariable (env));
        }
		
        public static bool ShuttingDown {
            get { return shutting_down; }
        }
     
        public static void Shutdown ()
        {
            shutting_down = true;
			
            if (OnShutdownRequested ()) {
                Dispose ();
            }
            shutting_down = false;
        }
           
        private static bool OnShutdownRequested ()
        {
            ShutdownRequestHandler handler = ShutdownRequested;
            if (handler != null) {
                foreach (ShutdownRequestHandler del in handler.GetInvocationList ()) {
                    if (!del ()) {
                        return false;
                    }
                }
            }
            
            return true;
        }
		
        private static void Dispose ()
        {
            ServiceManager.Shutdown();
			s_Client.Exit();
        }
    }
}
