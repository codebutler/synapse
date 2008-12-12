//
// DbusService.cs:
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
using System.Collections.Generic;
using Mono.Addins;
using Synapse.Core;
using NDesk.DBus;
using org.freedesktop.DBus;

namespace Synapse.Services
{
	[Extension ("/Synapse/Services")]
	public class DbusService : AbstractService
	{
		protected override void DoStart ()
		{
			BusG.Init();
		}
		
		protected override void DoStop ()
		{
		}
	}
}

namespace org.freedesktop
{
	public delegate void TrackChangeEventHandler(IDictionary<string,object> stuffs);
	[Interface("org.freedesktop.MediaPlayer")]
	public interface IMediaPlayer
	{
		event TrackChangeEventHandler TrackChange;
	}	
}