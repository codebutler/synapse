//
// GnomeScreensaverProvder.cs
//
// Based on code from Banshee - http://banshee-project.org
//
// Author:
//   Christopher James Halse Rogers <raof@ubuntu.com>
//
// Copyright (C) 2008 Novell, Inc.
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

using NDesk.DBus;

using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Services;


namespace Synapse.PlatformServices.Linux
{
	public class GnomeScreensaverProvider : IScreensaverProvider
	{
		const string DBUS_INTERFACE = "org.gnome.ScreenSaver";
		const string DBUS_PATH = "/org/gnome/ScreenSaver";

		IGnomeScreensaver manager;
		uint? cookie;

		public GnomeScreensaverProvider ()
		{
			if (Manager == null) {
				Hyena.Log.Information("GNOME screensaver service not found");
			}
		}

		IGnomeScreensaver Manager {
			get {
				if (manager == null) {
					if (!Bus.Session.NameHasOwner(DBUS_INTERFACE)) {
						return null;
					}
					
					manager = Bus.Session.GetObject<IGnomeScreensaver>(DBUS_INTERFACE, new ObjectPath(DBUS_PATH));
					
					if (manager == null) {
						Hyena.Log.ErrorFormat("The {0} object could not be located on the DBus interface {1}", DBUS_PATH, DBUS_INTERFACE);
					}
				}
				return manager;
			}
		}

		public void Inhibit ()
		{
			if (!cookie.HasValue && Manager != null) {
				cookie = Manager.Inhibit("Synapse", String.Empty);
			}
		}

		public void UnInhibit ()
		{
			if (cookie.HasValue && Manager != null) {
				Manager.UnInhibit(cookie.Value);
				cookie = null;
			}
		}

		public bool SessionIdle {
			get {
				if (Manager != null)
					return Manager.GetSessionIdle();
				else
					return false;
			}
		}

		public TimeSpan SessionIdleTime {
			get {
				if (Manager != null) {
					uint seconds = Manager.GetSessionIdleTime();
					return TimeSpan.FromSeconds(seconds);
				} else
					return TimeSpan.Zero;
			}
		}
	}

	[Interface("org.gnome.ScreenSaver")]
	internal interface IGnomeScreensaver
	{
		bool GetActive ();
		void SetActive (bool active);
		uint GetActiveTime ();
		bool GetSessionIdle ();
		uint GetSessionIdleTime ();
		void Lock ();
		void Cycle ();
		void SimulateUserActivity ();
		uint Inhibit (string appname, string reason);
		void UnInhibit (uint inhibitId);
		uint Throttle (string appname, string reason);
		void UnThrottle (uint throttleId);
	}
}