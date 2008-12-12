//
// Paths.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2005-2008 Novell, Inc.
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
using Mono.Unix;
 
namespace Synapse.Core
{
    public class Paths
    {
        public static string GetTempFileName (string dir)
        {
            return GetTempFileName (dir, null);
        }
        
        public static string GetTempFileName (string dir, string extension)
        {
            return GetTempFileName (new DirectoryInfo (dir), extension);
        }
        
        public static string GetTempFileName (DirectoryInfo dir, string extension)
        {
            string path = null;
            
            if (dir == null || !dir.Exists) {
                throw new DirectoryNotFoundException ();
            }

            do {
                string guid = Guid.NewGuid ().ToString ();
                string file = extension == null ? guid : String.Format ("{0}.{1}", guid, extension);
                path = Path.Combine (dir.FullName, file);
            } while (File.Exists (path));
        
            return path;
        }
        
        public static string Combine (string first, params string [] components)
        {
            if (String.IsNullOrEmpty (first)) {
                throw new ArgumentException ("First component must not be null or empty", "first");
            } else if (components == null || components.Length < 1) {
                throw new ArgumentException ("One or more path components must be provided", "components");
            }
            
            string result = first;
            
            foreach (string component in components) {
                result = Path.Combine (result, component);
            }
            
            return result;
        }

        public static string FindProgramInPath (string command)
        {
            foreach (string path in GetExecPaths ()) {
                string full_path = Path.Combine (path, command);
                try {
                    FileInfo info = new FileInfo (full_path);
                    // FIXME: System.IO is super lame, should check for 0755
                    if (info.Exists) {
                        return full_path;
                    }
                } catch {
                }
            }

            return null;
        }

        private static string [] GetExecPaths ()
        {
            string path = Environment.GetEnvironmentVariable ("PATH");
            if (String.IsNullOrEmpty (path)) {
                return new string [] { "/bin", "/usr/bin", "/usr/local/bin" };
            }

            // this is super lame, should handle quoting/escaping
            return path.Split (':');
        }
        
        public static string MakePathRelative (string path, string to)
        {
            if (String.IsNullOrEmpty (path) || String.IsNullOrEmpty (to)) {
                return null;
            }
            
            if (path == to) {
                return String.Empty;
            }
            
            if (to[to.Length - 1] != Path.DirectorySeparatorChar) {
                to = to + Path.DirectorySeparatorChar;
            }
            
            if (path.Length < to.Length) {
                return null;
            }
            
            return path.StartsWith (to)
                ? path.Substring (to.Length)
                : null;
        }
        
        private static string application_data = Path.Combine (Environment.GetFolderPath (
            Environment.SpecialFolder.ApplicationData), "synapse");
        
        public static string ApplicationData {
            get { 
                if (!Directory.Exists (application_data)) {
                    Directory.CreateDirectory (application_data);
                }
                
                return application_data; 
            }
        }
        
        private static string application_cache = Path.Combine (XdgBaseDirectorySpec.GetUserDirectory (
            "XDG_CACHE_HOME", ".cache"), "synapse");
        
        public static string ApplicationCache {
            get { return application_cache; }
        }
        
        public static string ExtensionCacheRoot {
            get { return Path.Combine (ApplicationCache, "extensions"); }
        }
        
        public static string DefaultLibraryPath {
            get { return XdgBaseDirectorySpec.GetUserDirectory ("XDG_MUSIC_DIR", "Music"); }
        }
        
        public static string TempDir {
            get {
                string dir = Path.Combine (ApplicationCache, "temp");
        
                if (File.Exists (dir)) {
                    File.Delete (dir);
                }
                
                Directory.CreateDirectory (dir);
                return dir;
            }
        }
        
        private static string installed_application_prefix = null;
        public static string InstalledApplicationPrefix {
            get {
                if (installed_application_prefix == null) {
                    installed_application_prefix = Path.GetDirectoryName (
                        System.Reflection.Assembly.GetExecutingAssembly ().Location);
                    
                    if (Directory.Exists (Paths.Combine (installed_application_prefix, "share", "synapse"))) {
                        return installed_application_prefix;
                    }
                        
                    DirectoryInfo entry_directory = new DirectoryInfo (installed_application_prefix);
                    
                    if (entry_directory != null && entry_directory.Parent != null && entry_directory.Parent.Parent != null) {
                        installed_application_prefix = entry_directory.Parent.Parent.FullName;
                    }
                }
                
                return installed_application_prefix;
            }
        }
        
        public static string InstalledApplicationDataRoot {
            get { return Path.Combine (InstalledApplicationPrefix, "share"); }
        }
        
        public static string InstalledApplicationData {
            get { return Path.Combine (InstalledApplicationDataRoot, "synapse"); }
        }
        
        public static string GetInstalledDataDirectory (string path)
        {
            return Path.Combine (InstalledApplicationData, path);
        }
    }
}
