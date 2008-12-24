//
// ServiceManager.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2007-2008 Novell, Inc.
//
// This file was copied from Banshee - http://banshee-project.org/
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
using System.Collections.Generic;
using Hyena;

using Mono.Addins;

using Synapse.Core;
using Synapse.Services;

namespace Synapse.ServiceStack
{
    public static class ServiceManager
    {
        private static Dictionary<string, IService> services = new Dictionary<string, IService> ();
        private static Dictionary<string, IExtensionService> extension_services = new Dictionary<string, IExtensionService> ();
        private static Stack<IService> dispose_services = new Stack<IService> ();
        private static List<Type> service_types = new List<Type> ();
        private static ExtensionNodeList extension_nodes;
        
        private static bool is_initialized = false;
        private static readonly object self_mutex = new object ();

        public static event EventHandler StartupBegin;
        public static event EventHandler StartupFinished;
        public static event ServiceStartedHandler ServiceStarted;
        
        public static void Initialize ()
        {
			Application.Client.Started += OnClientStarted;
		
            InitializeAddins ();
            RegisterDefaultServices ();
            RegisterAddinServices ();
        }
        
        private static void InitializeAddins ()
        {
			AddinManager.AddinLoaded += delegate(object sender, AddinEventArgs args) {
				Console.WriteLine("LOADED ADDIN: " + args.AddinId);	
			};
			
			AddinManager.AddinLoadError += delegate(object sender, AddinErrorEventArgs args) {
				Console.Error.WriteLine("ADDIN LOAD ERROR: " + args.Message + " " + args.Exception);
			};
			
            AddinManager.Initialize (Application.CommandLine.Contains ("uninstalled") 
                ? "." : Paths.ApplicationData);
            
            IProgressStatus monitor = Application.CommandLine.Contains ("debug-addins")
                ? new ConsoleProgressStatus (true)
                : null;
			
            if (Application.Debugging) {
                AddinManager.Registry.Rebuild (monitor);
            } else {
                AddinManager.Registry.Update (monitor);
            }
        }
           
        private static void RegisterAddinServices ()
        {
            extension_nodes = AddinManager.GetExtensionNodes ("/Synapse/ServiceManager/Service");
        }
        
        private static void RegisterDefaultServices ()
        {
			RegisterService<DBusServiceManager>();
            RegisterService<NotificationService>();
			RegisterService<ScreensaverService>();
			RegisterService<NowPlayingService>();
			RegisterService<NetworkService>();
        }
        
        private static void OnClientStarted(Client client)
        {
            DelayedInitialize ();
        }
        
        public static void Run()
        {
            lock (self_mutex) {          
                OnStartupBegin ();
                
                uint cumulative_timer_id = Log.InformationTimerStart ();
                
                foreach (Type type in service_types) {
                    RegisterService (type);
                }
                
                if (extension_nodes != null) {
                    foreach (TypeExtensionNode node in extension_nodes) {
                        StartExtension (node);
                    }
                }
                
                if (AddinManager.IsInitialized) {
                    AddinManager.AddExtensionNodeHandler ("/Synapse/ServiceManager/Service", OnExtensionChanged);
                }
                
                is_initialized = true;
                
                Log.InformationTimerPrint (cumulative_timer_id, "All services are started {0}");
                
                OnStartupFinished ();
            }
        }
        
        private static IService RegisterService (Type type)
        {
            IService service = null;
            
            try {
                uint timer_id = Log.DebugTimerStart ();
                service = (IService)Activator.CreateInstance (type);
                RegisterService (service);
                
                Log.DebugTimerPrint (timer_id, String.Format (
                    "Core service started ({0}, {{0}})", service.ServiceName));
                
                OnServiceStarted (service);
                
                if (service is IDisposable) {
                    dispose_services.Push (service);
                }

                if (service is IInitializeService) {
                    ((IInitializeService)service).Initialize ();
                }
                
                return service;
            } catch (Exception e) {
                if (service is IRequiredService) {
                    Log.ErrorFormat ("Error initializing required service {0}",
                            service == null ? type.ToString () : service.ServiceName, false);
                    throw;
                }
                
                Log.Warning (String.Format ("Service `{0}' not started: {1}", type.FullName, 
                    e.InnerException != null ? e.InnerException.Message : e.Message));
                Log.Exception (e.InnerException ?? e);
            }
            
            return null;
        }
        
        private static void StartExtension (TypeExtensionNode node)
        {
            if (extension_services.ContainsKey (node.Path)) {
                return;
            }
        
            IExtensionService service = null;
                    
            try {
                uint timer_id = Log.DebugTimerStart ();
                
                service = (IExtensionService)node.CreateInstance (typeof (IExtensionService));
                service.Initialize ();
                RegisterService (service);

                DelayedInitialize (service);
            
                Log.DebugTimerPrint (timer_id, String.Format (
                    "Extension service started ({0}, {{0}})", service.ServiceName));
            
                OnServiceStarted (service);
                
                extension_services.Add (node.Path, service);
            
                dispose_services.Push (service);
            } catch (Exception e) {
                Log.Exception (e.InnerException ?? e);
                Log.Warning (String.Format ("Extension `{0}' not started: {1}", 
                    service == null ? node.Path : service.GetType ().FullName, e.Message));
            }
        }
        
        private static void OnExtensionChanged (object o, ExtensionNodeEventArgs args) 
        {
            lock (self_mutex) {
                TypeExtensionNode node = (TypeExtensionNode)args.ExtensionNode;
                
                if (args.Change == ExtensionChange.Add) {
                    StartExtension (node);
                } else if (args.Change == ExtensionChange.Remove && extension_services.ContainsKey (node.Path)) {
                    IExtensionService service = extension_services[node.Path];
                    extension_services.Remove (node.Path);
                    services.Remove (service.ServiceName);
                    ((IDisposable)service).Dispose ();
                    
                    Log.DebugFormat ("Extension service disposed ({0})", service.ServiceName);
                    
                    // Rebuild the dispose stack excluding the extension service
                    IService [] tmp_services = new IService[dispose_services.Count - 1];
                    int count = tmp_services.Length;
                    foreach (IService tmp_service in dispose_services) {
                        if (tmp_service != service) {
                            tmp_services[--count] = tmp_service;
                        }
                    }
                    dispose_services = new Stack<IService> (tmp_services);
                }
            }
        }

        private static bool delayed_initialized, have_client;
        private static void DelayedInitialize ()
        {
            lock (self_mutex) {
                if (!delayed_initialized) {
                    have_client = true;
                    foreach (IService service in services.Values) {
                        DelayedInitialize (service);
                    }
                    delayed_initialized = true;
                }
            }
        }
        
        private static void DelayedInitialize (IService service)
        {
            if (have_client && service is IDelayedInitializeService) {
                Log.DebugFormat ("Delayed Initializating {0}", service);
                ((IDelayedInitializeService)service).DelayedInitialize ();
            }
        }
        
        public static void Shutdown ()
        {
            lock (self_mutex) {
                while (dispose_services.Count > 0) {
                    IService service = dispose_services.Pop ();
                    ((IDisposable)service).Dispose ();
                    Log.DebugFormat ("Service disposed ({0})", service.ServiceName);
                }
                
                services.Clear ();
            }
        }
        
        public static void RegisterService (IService service)
        {
            lock (self_mutex) {
                services.Add (service.ServiceName, service);
                
                if(service is IDBusExportable) {
                    DBusServiceManager.RegisterObject ((IDBusExportable)service);
                }
            }
        }
        
        public static void RegisterService<T> () where T : IService
        {
            lock (self_mutex) {
                if (is_initialized) {
                    RegisterService (Activator.CreateInstance <T> ());
                } else {
                    service_types.Add (typeof (T));
                }
            }
        }
        
        public static bool Contains (string serviceName)
        {
            lock (self_mutex) {
                return services.ContainsKey (serviceName);
            }
        }

        public static bool Contains<T> () where T : class, IService
        {
            return Contains (typeof (T).Name);
        }
        
        public static IService Get (string serviceName)
        {
            if (services.ContainsKey (serviceName)) {
                return services[serviceName]; 
            }
            
            return null;
        }
        
        public static T Get<T> (string serviceName) where T : class, IService
        {
            return Get (serviceName) as T;
        }
        
        public static T Get<T> () where T : class, IService
        {
            Type type = typeof (T);
            T service = Get (type.Name) as T;
            if (service == null && type.GetInterface ("Synapse.ServiceStack.IRegisterOnDemandService") != null) {
                return RegisterService (type) as T;
            }

            if (service == null)
                throw new Exception(String.Format("Service not found: {0}", type.Name));
			
            return service;
        }
        
        private static void OnStartupBegin ()
        {
            EventHandler handler = StartupBegin;
            if (handler != null) {
                handler (null, EventArgs.Empty);
            }
        }
        
        private static void OnStartupFinished ()
        {
            EventHandler handler = StartupFinished;
            if (handler != null) {
                handler (null, EventArgs.Empty);
            }
        }
        
        private static void OnServiceStarted (IService service)
        {
            ServiceStartedHandler handler = ServiceStarted;
            if (handler != null) {
                handler (new ServiceStartedArgs (service));
            }
        }
        
        public static int StartupServiceCount {
            get { return service_types.Count + (extension_nodes == null ? 0 : extension_nodes.Count) + 1; }
        }
        
        public static int ServiceCount {
            get { return services.Count; }
        }
        
        public static bool IsInitialized {
            get { return is_initialized; }
        }
        
        public static DBusServiceManager DBusServiceManager {
            get { return Get<DBusServiceManager> (); }
        }
    }
}
