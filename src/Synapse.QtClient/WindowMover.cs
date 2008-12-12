//
// WindowMover.cs
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
	public class WindowMover : QObject
	{
		bool    m_Moving = false;
		int     m_OrigX;
		int     m_OrigY;
		QWidget m_ParentWidget;
		
		public WindowMover(QWidget widget) : base (widget)
		{
			m_ParentWidget = widget;
		}

		public bool EventFilter (QObject obj, QEvent evnt)
		{
			var type = evnt.type();

			if (type == QEvent.TypeOf.MouseButtonPress) {
				var mouseEvent = (QMouseEvent)evnt;
				if (mouseEvent.Button() == Qt.MouseButton.LeftButton) {
					m_Moving = true;
					m_OrigX = mouseEvent.X();
					m_OrigY = mouseEvent.Y();
					m_ParentWidget.Cursor = new QCursor(Qt.CursorShape.SizeAllCursor);
				}
				
			} else if (type == QEvent.TypeOf.MouseMove) {
				var mouseEvent = (QMouseEvent)evnt;
				if (m_Moving) {
					var pos = mouseEvent.GlobalPos();
					m_ParentWidget.Move(pos.X() - m_OrigX, pos.Y() - m_OrigY);
				}
				
			} else if (type == QEvent.TypeOf.MouseButtonRelease) {
				var mouseEvent = (QMouseEvent)evnt;
				if (m_Moving && mouseEvent.Button() == Qt.MouseButton.LeftButton) {
					m_Moving = false;
					m_ParentWidget.Cursor = new QCursor(Qt.CursorShape.ArrowCursor);
				}
			}
			
			return obj.EventFilter(obj, evnt);
		}
	}
}
