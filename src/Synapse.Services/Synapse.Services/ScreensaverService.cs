// ScreensaverService.cs
//
// Authors:
//   Christian Hergert <chris@dronelabs.com>
//
// Copyright (C) 2008 Christian Hergert
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

using NDesk.DBus;

using Synapse.Core;
using Synapse.ServiceStack;

namespace Synapse.Services
{
	public delegate void ActiveChangedHandler (bool new_value);
	public delegate void SessionIdleChangedHandler (bool new_value);
	
	public class ScreensaverService : IService, IInitializeService
	{
		private const string BusName    = "org.gnome.ScreenSaver";
		private const string ObjectPath = "/org/gnome/ScreenSaver";
		
		private IScreenSaver m_screenSaver;
		
		/// <value>
		/// Returns the value of the current state of activity.
		/// </value>
		public event ActiveChangedHandler ActiveChanged;
		
		/// <value>
		/// Returns the value of the current state of activity.
		/// </value>
		public event SessionIdleChangedHandler SessionIdleChanged;
		
		/// <value>
		/// Emitted before an authentication request
		/// </value>
		public event EventHandler AuthenticationRequestBegin;
		
		/// <value>
		/// Emitted after an authentication request
		/// </value>
		public event EventHandler AuthenticationRequestEnd;
		
		/// <value>
		/// Request a change in the state of the screensaver.
		/// Set to true to request that the screensaver activate.
		/// Active means that the screensaver has blanked the
		/// screen and may run a graphical theme.  This does
		/// not necessary mean that the screen is locked.
		/// </value>
		public bool Active {
			get {
				return m_screenSaver.GetActive ();
			}
			set {
				m_screenSaver.SetActive (value);
			}
		}
		
		/// <value>
		/// Returns the number of seconds that the screensaver has
		/// been active.  Returns zero if the screensaver is not active.
		/// </value>
		public TimeSpan ActiveTime {
			get {
				uint seconds = m_screenSaver.GetActiveTime ();
				return TimeSpan.FromSeconds (seconds);
			}
		}
		
		/// <value>
		/// Returns the value of the current state of session idleness.
		/// </value>
		public bool SessionIdle {
			get {
				return m_screenSaver.GetSessionIdle ();
			}
		}
		
		/// <value>
		/// Returns the number of seconds that the session has
		/// been idle.  Returns zero if the session is not idle.
		/// </value>
		public TimeSpan SessionIdleTime {
			get {
				uint seconds = m_screenSaver.GetSessionIdleTime ();
				return TimeSpan.FromSeconds (seconds);
			}
		}
		
		/// <summary>
		/// Request that the screen be locked.
		/// </summary>
		public void Lock ()
		{
			m_screenSaver.Lock ();
		}
		
		/// <summary>
		/// Request that the screen saver theme by restarted and if applicable
		/// switch to the next one in the list.
		/// </summary>
		public void Cycle ()
		{
			m_screenSaver.Cycle ();
		}
		
		/// <summary>
		/// Simulate user activity which resets idle times.
		/// </summary>
		public void SimulateUserActivity ()
		{
			m_screenSaver.SimulateUserActivity ();
		}
		
		/// <summary>
		/// Request that saving the screen due to system idleness be blocked
		/// until UnInhibit is called or the calling process exits.
		/// </summary>
		/// <param name="appname">
		/// A <see cref="System.String"/> containing the application name.
		/// </param>
		/// <param name="reason">
		/// A <see cref="System.String"/> containing the reason for inhibit.
		/// </param>
		/// <returns>
		/// A <see cref="System.UInt32"/> containing a randomly generated
		/// cookie identifying the inhibit request. Use this to uninhibit.
		/// </returns>
		public uint Inhibit (string appname, string reason)
		{
			return m_screenSaver.Inhibit (appname, reason);
		}
		
		/// <summary>
		/// Cancel a previous call to Inhibit() identified by the cookie.
		/// </summary>
		/// <param name="inhibitId">
		/// A <see cref="System.UInt32"/> containing the cookie from a
		/// previous call to Inhibit().
		/// </param>
		public void UnInhibit (uint inhibitId)
		{
			m_screenSaver.UnInhibit (inhibitId);
		}
		
		/// <summary>
		/// Request that running themes while the screensaver is active
		/// be blocked until UnThrottle is called or the
		/// calling process exits.
		/// </summary>
		/// <param name="appname">
		/// A <see cref="System.String"/> containing the application name.
		/// </param>
		/// <param name="reason">
		/// A <see cref="System.String"/> containing the reason for throttling.
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/> containing a randomly generated
		/// cookie identifying the throttle request. Use this to UnThrottle().
		/// </returns>
		public uint Throttle (string appname, string reason)
		{
			return m_screenSaver.Throttle (appname, reason);
		}
		
		/// <summary>
		/// Cancel a previous call to Throttle() identified by the cookie.
		/// </summary>
		/// <param name="throttleId">
		/// A <see cref="System.Int32"/> containing the cookie from a
		/// previous call to Throttle().
		/// </param>
		public void UnThrottle (uint throttleId)
		{
			m_screenSaver.UnThrottle (throttleId);
		}
		
		public void Initialize ()
		{
			if (Application.CommandLine.Contains("disable-dbus"))
				return;
				
			if (!Bus.Session.NameHasOwner (BusName))
				throw new ApplicationException(String.Format("Name {0} has no owner", BusName));

			m_screenSaver = Bus.Session.GetObject<IScreenSaver> (BusName, new ObjectPath (ObjectPath));
			
			/* connect to the active changed event and forward it */
			m_screenSaver.ActiveChanged += delegate (bool isActive) {
				ActiveChangedHandler handler = ActiveChanged;
				if (handler != null)
					handler (isActive);
			};
			
			/* connect to the session idle event and forward it */
			m_screenSaver.SessionIdleChanged += delegate (bool isSessionIdle) {
				SessionIdleChangedHandler handler = SessionIdleChanged;
				if (handler != null)
					handler (isSessionIdle);
			};
			
			/* connect to the begin auth and forward it */
			m_screenSaver.AuthenticationRequestBegin += delegate () {
				EventHandler handler = AuthenticationRequestBegin;
				if (handler != null)
					handler (this, EventArgs.Empty);
			};
			
			/* connect to the end auth and forward it */
			m_screenSaver.AuthenticationRequestEnd += delegate () {
				EventHandler handler = AuthenticationRequestEnd;
				if (handler != null)
					handler (this, EventArgs.Empty);
			};
		}
		
		string IService.ServiceName {
			get { return "ScreensaverService"; }
		}
	}
	
	internal delegate void VoidHandler ();
	
	[Interface("org.gnome.ScreenSaver")]
	internal interface IScreenSaver
	{
		event ActiveChangedHandler ActiveChanged;
		event SessionIdleChangedHandler SessionIdleChanged;
		event VoidHandler AuthenticationRequestBegin;
		event VoidHandler AuthenticationRequestEnd;
		
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