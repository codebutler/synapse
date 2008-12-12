// Service.cs
//
// Authors:
//   Christian Hergert <chris@dronelabs.com>
//
// Copyright (c) 2008 Christian Hergert
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

using Mono.Addins;

namespace Synapse.Core
{
	public abstract class AbstractService : IService
	{
		public event EventHandler Started;
		public event EventHandler Stopped;
		
		protected bool m_Running = false;
		
		public bool Running {
			get {
				return m_Running;
			}
		}
		
		public void Start ()
		{
			DoStart ();
			m_Running = true;
			OnStarted (new EventArgs ());
		}
		
		public void Stop ()
		{
			DoStop ();
			m_Running = false;
			OnStopped (new EventArgs ());
		}
		
		protected virtual void OnStarted (EventArgs args)
		{
			if (Started != null)
				Started (this, args);
		}
		
		protected virtual void OnStopped (EventArgs args)
		{
			if (Stopped != null)
				Stopped (this, args);
		}
		
		protected abstract void DoStart ();
		protected abstract void DoStop ();
	}
}