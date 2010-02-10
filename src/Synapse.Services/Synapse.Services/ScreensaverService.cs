//
// ScreensaverService.cs
//
// Based on code from Banshee - http://banshee-project.org/
//
// Author:
//   Christopher James Halse Rogers <raof@ubuntu.com>
//
// Copyright (C) 2008 Novell, Inc.
//
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
using Synapse.ServiceStack;
using Hyena;
using Mono.Addins;

namespace Synapse.Services
{
	public interface IScreensaverProvider
	{
        void Inhibit();
        void UnInhibit();		
		
		bool SessionIdle { get; }
		TimeSpan SessionIdleTime { get; }
	}
	
	public class ScreensaverService : IService, IDelayedInitializeService, IDisposable
	{
		IScreensaverProvider m_Provider;
		bool m_Inhibited = false;
		
		public void DelayedInitialize ()
		{
			foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes("/Synapse/PlatformServices/ScreensaverProvider")) {
				try {
					m_Provider = (IScreensaverProvider)node.CreateInstance(typeof(IScreensaverProvider));
					Log.DebugFormat("Loaded IScreensaverProvider: {0}", m_Provider.GetType().FullName);
				} catch (Exception ex) {
					Log.Exception("IScreensaverProvider extension failed to load", ex);
				}
			}
		}
		
		public void Dispose ()
		{
			UnInhibit();
		}
		
        public void Inhibit ()
        {
            if (m_Provider != null && !m_Inhibited) {
                Log.Information("Inhibiting screensaver during fullscreen playback");
                m_Provider.Inhibit();
                m_Inhibited = true;
            }
        }

        public void UnInhibit ()
        {
            if (m_Provider != null && m_Inhibited) {
                Log.Information("Uninhibiting screensaver");
                m_Provider.UnInhibit();
                m_Inhibited = false;
			}
		}
		
		string IService.ServiceName {
			get { return "ScreensaverService"; }
		}
	}
}
