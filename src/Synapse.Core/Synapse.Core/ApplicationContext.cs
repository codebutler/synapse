//
// ApplicationContext.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2007 Novell, Inc.
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

using Hyena;
using Hyena.CommandLine;

namespace Synapse.Core
{
    public delegate void InvokeHandler ();

    public static class ApplicationContext
    {
        static ApplicationContext () 
        {
            Log.Debugging = Debugging;
        }
    
        private static CommandLineParser command_line = new CommandLineParser ();
        public static CommandLineParser CommandLine {
            set { command_line = value; }
            get { return command_line; }
        }
        
        private static Layout command_line_layout;
        public static Layout CommandLineLayout {
            get { return command_line_layout; }
            set { command_line_layout = value; }
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
        
        public static System.Globalization.CultureInfo InternalCultureInfo {
            get { return System.Globalization.CultureInfo.InvariantCulture; }
        }
    }
}
