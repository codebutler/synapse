//
// DBusServiceManager.cs
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
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Reflection;
    
using NDesk.DBus;
using org.freedesktop.DBus;

using Hyena;

namespace Synapse.ServiceStack
{
    public class DBusExportableAttribute : Attribute
    {
        private string service_name;
        public string ServiceName {
            get { return service_name; }
            set { service_name = value; }
        }
    }

    public class DBusServiceManager : IService
    {
        public const string ObjectRoot = "/im/synapse/Synapse";
        private Dictionary<object, ObjectPath> registered_objects = new Dictionary<object, ObjectPath> ();
        
        public static string MakeDBusSafeString (string str)
        {
            return str == null ? String.Empty : Regex.Replace (str, @"[^A-Za-z0-9]*", String.Empty);
        }
        
        public static string MakeObjectPath (IDBusExportable o)
        {
            StringBuilder object_path = new StringBuilder ();
            
            object_path.Append (ObjectRoot);
            object_path.Append ('/');
            
            Stack<string> paths = new Stack<string> ();
            
            IDBusExportable p = o.Parent;
            
            while (p != null) {
                paths.Push (String.Format ("{0}/", GetObjectName (p)));
                p = p.Parent;
            }
            
            while (paths.Count > 0) {
                object_path.Append (paths.Pop ());
            }
            
            object_path.Append (GetObjectName (o));
            
            return object_path.ToString ();
        }
        
        private static string GetObjectName (IDBusExportable o)
        {
            return o is IDBusObjectName ? ((IDBusObjectName)o).ExportObjectName : o.ServiceName;
        }
        
        public static string [] MakeObjectPathArray<T> (IEnumerable<T> collection) where T : IDBusExportable
        {
            List<string> paths = new List<string> ();
            
            foreach (IDBusExportable item in collection) {
                paths.Add (MakeObjectPath (item));
            }
            
            return paths.ToArray ();
        }
        
        public ObjectPath RegisterObject (IDBusExportable o)
        {
            return RegisterObject (DBusConnection.DefaultServiceName, o);
        }
        
        public ObjectPath RegisterObject (string serviceName, IDBusExportable o)
        {
            return RegisterObject (serviceName, o, MakeObjectPath (o));
        }
        
        public ObjectPath RegisterObject (object o, string objectName)
        {
            return RegisterObject (DBusConnection.DefaultServiceName, o, objectName);
        }
        
        public ObjectPath RegisterObject (string serviceName, object o, string objectName)
        {
            ObjectPath path = null;
            
            if (DBusConnection.Enabled && Bus.Session != null) {
                object [] attrs = o.GetType ().GetCustomAttributes (typeof (DBusExportableAttribute), true);
                if (attrs != null && attrs.Length > 0) {
                    DBusExportableAttribute dbus_attr = (DBusExportableAttribute)attrs[0];
                    if (!String.IsNullOrEmpty (dbus_attr.ServiceName)) {
                        serviceName = dbus_attr.ServiceName;
                    }
                }
            
                lock (registered_objects) {
                    registered_objects.Add (o, path = new ObjectPath (objectName));
                }
                
                string bus_name = DBusConnection.MakeBusName (serviceName);
                
                Log.DebugFormat ("Registering remote object {0} ({1}) on {2}", path, o.GetType (), bus_name);
                
                #pragma warning disable 0618
                Bus.Session.Register (bus_name, path, o);
                #pragma warning restore 0618
            }
            
            return path;
        }

        public void UnregisterObject (object o)
        {
            ObjectPath path = null;
            lock (registered_objects) {
                if (!registered_objects.TryGetValue (o, out path)) {
                    Log.WarningFormat ("Unable to unregister DBus object {0}, does not appear to be registered", 
                        o.GetType ());
                    return;
                }
                
                registered_objects.Remove (o);
            }
        
            Bus.Session.Unregister (path);
        }
        
        public static T FindInstance<T> (string objectPath) where T : class
        {
            return FindInstance<T> (DBusConnection.DefaultBusName, true, objectPath);
        }
        
        public static T FindInstance<T> (string serviceName, string objectPath) where T : class
        {
            return FindInstance<T> (serviceName, false, objectPath);
        }
        
        public static T FindInstance<T> (string serviceName, bool isFullBusName, string objectPath) where T : class
        {
            string busName = isFullBusName ? serviceName : DBusConnection.MakeBusName (serviceName);
            if (!DBusConnection.Enabled || !Bus.Session.NameHasOwner (busName)) {
                return null;
            }
            
            string full_object_path = objectPath;
            if (!objectPath.StartsWith (ObjectRoot)) {
                full_object_path = ObjectRoot + objectPath;
            }

            return Bus.Session.GetObject<T> (busName, new ObjectPath (full_object_path));
        }
        
        string IService.ServiceName {
            get { return "DBusServiceManager"; }
        }
    }
}
