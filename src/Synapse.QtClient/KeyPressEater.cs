//
// KeyPressEater.cs
// 
// Copyright (C) 2008 Eric Butler
//
// Authors:
//   Eric Butler <eric@extremeboredom.net>
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
using Qyoto;

namespace Synapse.QtClient
{
	public delegate bool KeyEventHandler (QKeyEvent kevent);
	
	public class KeyPressEater : QObject
	{
		public event KeyEventHandler KeyEvent;
		
		public KeyPressEater (QObject parent) : base (parent)
		{
		}

		public KeyPressEater(KeyEventHandler handler, QObject parent) : base (parent)
		{
			this.KeyEvent += handler;
		}
			
		private bool EventFilter(QObject obj, QEvent evnt)
		{
			if (evnt.type() == QEvent.TypeOf.KeyPress) {
				if (KeyEvent != null) {
					if (KeyEvent((QKeyEvent)evnt)) {
						return true;
					}
				}
			}
			return obj.EventFilter(obj, evnt);
		}
	}
}
