// 
// PlatformHacks.cs
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
using System.Text;
using System.Runtime.InteropServices;

namespace Synapse.Core
{
    public static class PlatformHacks
    {
        // For the SEGV trap hack (see below)
        [DllImport ("libc")]
        private static extern int sigaction (Mono.Unix.Native.Signum sig, IntPtr act, IntPtr oact);
        
        private static IntPtr mono_jit_segv_handler = IntPtr.Zero;
        
        public static void TrapMonoJitSegv ()
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix) {
                return;
            }
        
            // We must get a reference to the JIT's SEGV handler because 
            // GStreamer will set its own and not restore the previous, which
            // will cause what should be NullReferenceExceptions to be unhandled
            // segfaults for the duration of the instance, as the JIT is powerless!
            // FIXME: http://bugzilla.gnome.org/show_bug.cgi?id=391777
            
            try {
                mono_jit_segv_handler = Marshal.AllocHGlobal (512);
                sigaction (Mono.Unix.Native.Signum.SIGSEGV, IntPtr.Zero, mono_jit_segv_handler);
            } catch {
            }
        }
        
        public static void RestoreMonoJitSegv ()
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix || mono_jit_segv_handler.Equals (IntPtr.Zero)) {
                return;
            }
            
            // Reset the SEGV handle to that of the JIT again (SIGH!)
            try {
                sigaction (Mono.Unix.Native.Signum.SIGSEGV, mono_jit_segv_handler, IntPtr.Zero);
                Marshal.FreeHGlobal (mono_jit_segv_handler);
            } catch {
            }
        }
        
        [DllImport ("libc")] // Linux
        private static extern int prctl (int option, byte [] arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);
        
        [DllImport ("libc")] // BSD
        private static extern void setproctitle (byte [] fmt, byte [] str_arg);

        public static void SetProcessName (string name)
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix) {
                return;
            }
        
            try {
                if (prctl (15 /* PR_SET_NAME */, Encoding.ASCII.GetBytes (name + "\0"), 
                    IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) != 0) {
                    throw new ApplicationException ("Error setting process name: " + 
                        Mono.Unix.Native.Stdlib.GetLastError ());
                }
            } catch (EntryPointNotFoundException) {
                setproctitle (Encoding.ASCII.GetBytes ("%s\0"), 
                    Encoding.ASCII.GetBytes (name + "\0"));
            }
        }
        
        public static void TrySetProcessName (string name)
        {
            try {
                SetProcessName (name);
            } catch {
            }
        }
    }
}
