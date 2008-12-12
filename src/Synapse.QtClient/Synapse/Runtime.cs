// Runtime.cs
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

using Hyena;
using Mono.Addins;

using Synapse.ServiceStack;

namespace Synapse.Core
{
	public class Runtime
	{
		public delegate void ApplicationStartedEventHandler();
		public static event ApplicationStartedEventHandler ApplicationStarted;
		
		public static void Start ()
		{
			ServiceManager.DefaultInitialize();

			var applicationStarted = Runtime.ApplicationStarted;
			if (applicationStarted != null) {
				applicationStarted();
			}
		}
	}
}