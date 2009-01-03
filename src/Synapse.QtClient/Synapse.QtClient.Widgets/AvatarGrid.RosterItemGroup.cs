//
// AvatarGrid.RosterItemGroup.cs
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

namespace Synapse.QtClient.Widgets
{	
	public partial class AvatarGrid<T> : QGraphicsView
	{
		class RosterItemGroup : QGraphicsItemGroup, IFadableItem
		{
			AvatarGrid<T> m_Grid;
			QFont         m_Font;
			string        m_GroupName;
			QRectF        m_Rect;
			bool          m_Expanded = true;
			double        m_Opacity = 1;
			int           m_TextWidth;
			int           m_RowCount;
			
			public RosterItemGroup (AvatarGrid<T> grid, string groupName)
			{
				m_Grid      = grid;
				m_GroupName = groupName;
				
				m_Font = new QFont(m_Grid.Font);
				m_Font.SetPointSize(8); // FIXME: Set to m_Grid.HeaderHeight.
				m_Font.SetBold(true);
				
				QFontMetrics metrics = new QFontMetrics(m_Font);
				m_TextWidth = metrics.Width(m_GroupName);
				
				m_Rect = new QRectF(m_Grid.IconPadding, 0, 0, 0);

				base.SetHandlesChildEvents(false);
				base.SetAcceptHoverEvents(true);
			}

			public double Opacity {
				get {
					return m_Opacity;
				}
				set {
					m_Opacity = value;
					this.Update();
				}
			}
			
			public bool IsExpanded {
				get {
					return m_Expanded;
				}
				set {
					m_Expanded = value;
					this.Update();
				}
			}

			public string Name {
				get {
					return m_GroupName;
				}
			}
			
			public int RowCount {
				get {
					return m_RowCount;
				}
				set {
					m_RowCount = value;
				}
			}
			
			public void BeginFade(bool fadeIn)
			{
				// FIXME: Start an animation.
				this.Opacity = fadeIn ? 1 : 0;
				this.SetVisible(fadeIn);
			}

			public void BeginMove(QPointF pos)
			{
				// FIXME: Start an animation.
				this.SetPos(pos);
			}			
			
			public override QRectF BoundingRect ()
			{
				// FIXME: We need to animate the change in height when we 
				// collapse/expand... or at the very least not change height 
				// until all item animations are complete.
				// Right now, we don't see any items fade out on collapse 
				// because the group size changes right away.

				m_Rect.SetLeft(m_Grid.IconPadding);
				m_Rect.SetWidth(m_Grid.Viewport().Width() - (m_Grid.IconPadding * 2));
				if (IsExpanded)
					m_Rect.SetHeight(m_RowCount * (m_Grid.IconSize + m_Grid.IconPadding));
				else
					m_Rect.SetHeight(m_Grid.HeaderHeight);							
				return m_Rect;
			}
			
			public override void Paint (Qyoto.QPainter painter, Qyoto.QStyleOptionGraphicsItem option, Qyoto.QWidget widget)
			{
				painter.SetOpacity(m_Opacity);

				var color = m_Grid.Palette.Color(QPalette.ColorRole.Text);
				
				// Group Name
				painter.SetFont(m_Font);
				painter.SetPen(new QPen(color));
				painter.DrawText(BoundingRect(), m_GroupName);

				// Group expander arrow
				painter.Save();
				painter.Translate(m_Grid.IconPadding + m_TextWidth + 4, 5); // FIXME: These numbers probably shouldn't be hard coded.
				QPainterPath path = new QPainterPath();
				if (m_Expanded) {
					path.MoveTo(0, 0);
					path.LineTo(4, 0);
					path.LineTo(2, 2);
					path.LineTo(0, 0);
				} else {
					path.MoveTo(2, 0);
					path.LineTo(2, 4);
					path.LineTo(0, 2);
					path.LineTo(2, 0);
				}
				painter.SetPen(new QPen(color));
				painter.SetBrush(new QBrush(color));
				painter.DrawPath(path);
				painter.Restore();

				//painter.SetPen(new QPen(new QColor("red")));
				//painter.DrawRect(BoundingRect());
			}

			protected override void MouseReleaseEvent (Qyoto.QGraphicsSceneMouseEvent arg1)
			{
				if (arg1.Button() == Qt.MouseButton.LeftButton) {
					var pos = arg1.Pos();
					var pos1 = arg1.ButtonDownPos(Qt.MouseButton.LeftButton);
					if (pos.Y() < m_Grid.HeaderHeight && pos1.Equals(pos)) {
						this.IsExpanded = !this.IsExpanded;
						m_Grid.ResizeAndRepositionGroups();
					}
				}
			}

			// Nothing works without this.
			protected override void MousePressEvent (Qyoto.QGraphicsSceneMouseEvent arg1)
			{
			}

			protected override void MouseMoveEvent (Qyoto.QGraphicsSceneMouseEvent evnt)
			{
				Console.WriteLine("Group Mouse Move");
				
				var app = ((QApplication)QApplication.Instance());
				if (new QLineF(evnt.ScreenPos(), evnt.ButtonDownScreenPos(Qt.MouseButton.LeftButton))
				.Length() < app.StartDragDistance) {
					return;
				}
	
				QDrag drag = new QDrag(evnt.Widget());
				QMimeData mime = new QMimeData();
				drag.SetMimeData(mime);
	
				drag.Exec();
			}
		}
	}
}
