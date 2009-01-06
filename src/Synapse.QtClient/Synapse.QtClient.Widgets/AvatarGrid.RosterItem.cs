//
// AvatarGrid.RosterItem.cs
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
		class RosterItem<T> : QGraphicsItem, IFadableItem
		{
			double        m_Opacity = 1;
			AvatarGrid<T> m_Grid;
			T             m_Item;
			QRectF        m_Rect;
			
			public RosterItem (AvatarGrid<T> grid, T item, double width, double height, QGraphicsItem parent) : base (parent)
			{
				m_Grid = grid;
				m_Item = item;
				m_Rect = new QRectF(0, 0, 0, 0);

				// FIXME: This causes all sorts of problems.
				// this.SetCacheMode(QGraphicsItem.CacheMode.DeviceCoordinateCache);

				base.SetAcceptHoverEvents(true);
			}

			public T Item {
				get {
					return m_Item;
				}
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

			public override void Paint (Qyoto.QPainter painter, Qyoto.QStyleOptionGraphicsItem option, Qyoto.QWidget widget)
			{
				int iconSize = m_Grid.IconSize;
				
				// Parent opacity overrides item opacity.
				var parentGroup = (RosterItemGroup)base.Group();
				if (parentGroup == null) // This happens while the item is being removed.
					return;				
				if (parentGroup.Opacity != 1)
					painter.SetOpacity(parentGroup.Opacity);
				else
					painter.SetOpacity(m_Opacity);
				
				QPixmap pixmap = (QPixmap)m_Grid.Model.GetImage(m_Item);				
				if (pixmap != null)
					painter.DrawPixmap(0, 0, iconSize, iconSize, pixmap);
				else
					painter.DrawRect(0, 0, iconSize, iconSize);

				if (IsHover) {
					painter.DrawRect(BoundingRect());
				}
				
				if (m_Grid.ListMode) {
					var rect = BoundingRect();
					var pen = new QPen();
					pen.SetBrush(new QBrush(new QColor(Qt.GlobalColor.white)));
					painter.SetPen(pen);

					int x = iconSize + m_Grid.IconPadding;
					painter.DrawText(x, 0, (int)rect.Width() - x, (int)rect.Height(), (int)Qt.TextFlag.TextSingleLine, m_Grid.Model.GetName(m_Item));
				}
			}

			// FIXME: This constantly crashes...
			public override QRectF BoundingRect ()
			{				
				if (!m_Grid.ListMode) {
					m_Rect.SetWidth(m_Grid.IconSize);
				} else {
					m_Rect.SetWidth(m_Grid.Viewport().Width() - (m_Grid.IconPadding * 2));
				}
				m_Rect.SetHeight(m_Grid.IconSize);
				return m_Rect;
			}

			public bool IsHover {
				get {
					return (m_Grid.HoverItem != null && m_Grid.HoverItem.Equals(m_Item));
				}
			}

			protected override void MousePressEvent (Qyoto.QGraphicsSceneMouseEvent arg1)
			{
				Console.WriteLine("item mouse press");
			}
	
			protected override void MouseMoveEvent (Qyoto.QGraphicsSceneMouseEvent evnt)
			{
				Console.WriteLine("Item Mouse Move");
				
				var app = ((QApplication)QApplication.Instance());
				if (new QLineF(evnt.ScreenPos(), evnt.ButtonDownScreenPos(Qt.MouseButton.LeftButton))
				.Length() < app.StartDragDistance) {
					return;
				}
	
				QDrag drag = new QDrag(evnt.Widget());
				QMimeData mime = new QMimeData();
				drag.SetMimeData(mime);
	
				drag.Exec((uint)Qt.DropAction.MoveAction | (uint)Qt.DropAction.CopyAction | (uint)Qt.DropAction.IgnoreAction);
	
				SetCursor(new QCursor(Qt.CursorShape.OpenHandCursor));
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
		}
	}
}
