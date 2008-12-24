//
// Client.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2008 Novell, Inc.
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
using System.Threading;
using Synapse.Core;

namespace Synapse.ServiceStack
{
	public delegate void InvokeHandler ();
	public delegate bool TimeoutHandler ();
	public delegate bool IdleHandler ();
	
    public abstract class Client : IDisposable
    {
        public event Action<Client> Started;

        public Client ()
        {
        }
        
        public virtual void Dispose ()
        {
        }
        
        public abstract string ClientId {
            get; 
        }

        private bool is_started;
        public bool IsStarted {
            get { return is_started; }
        }

        public virtual void Invoke (InvokeHandler handler)
        {
            RunIdle (delegate { handler (); return false; });
        }

		public virtual void InvokeAndBlock (InvokeHandler handler)
		{
			ManualResetEvent mutex = new ManualResetEvent(false);
			Invoke(delegate {
				handler();
				mutex.Set();
			});
			mutex.WaitOne();
		}

        public abstract uint RunIdle (IdleHandler handler);
        
        public abstract uint RunTimeout (uint milliseconds, TimeoutHandler handler);
        
        public abstract bool IdleTimeoutRemove (uint id);
		
		public abstract object CreateImage (byte[] data);
		
		public abstract object CreateImage (string fileName);

		public abstract object CreateAction (string id, string label, string icon, object parent);

		public abstract void ShowErrorWindow (string title, Exception error);
		
        protected void OnStarted ()
        {
            is_started = true;
            Hyena.Log.InformationFormat ("{0} Client Started", ClientId);
            Action<Client> handler = Started;
            if (handler != null) {
                handler (this);
            }
        }
    }
}
