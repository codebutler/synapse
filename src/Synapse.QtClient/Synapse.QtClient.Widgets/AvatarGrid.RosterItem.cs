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
using Synapse.UI;
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
					
			QTimeLine m_MoveAnimationTimeLine;
			QGraphicsItemAnimation m_MoveAnimation;
			
			QTimeLine m_FadeAnimationTimeLine;
			FadeInOutAnimation m_FadeAnimation;
			
			public RosterItem (AvatarGrid<T> grid, T item, double width, double height, QGraphicsItem parent) : base (parent)
			{
				m_Grid = grid;
				m_Item = item;
				m_Rect = new QRectF(0, 0, 0, 0);
				
				m_MoveAnimationTimeLine = new QTimeLine(500);
				
				m_MoveAnimation = new QGraphicsItemAnimation();
				m_MoveAnimation.SetItem(this);
				m_MoveAnimation.SetTimeLine(m_MoveAnimationTimeLine);				
								
				m_FadeAnimationTimeLine = new QTimeLine(500);
				
				m_FadeAnimation = new FadeInOutAnimation();
				m_FadeAnimation.SetItem(this);
				m_FadeAnimation.SetTimeLine(m_FadeAnimationTimeLine);

				// FIXME: This causes all sorts of problems.
				// this.SetCacheMode(QGraphicsItem.CacheMode.DeviceCoordinateCache);

				base.SetAcceptHoverEvents(true);
			}

			~RosterItem ()
			{
				m_FadeAnimationTimeLine.Stop();
				m_MoveAnimationTimeLine.Stop();
				m_FadeAnimation = null;
				m_FadeAnimationTimeLine = null;
				m_MoveAnimation = null;
				m_MoveAnimationTimeLine = null;
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
				painter.SetRenderHint(QPainter.RenderHint.Antialiasing, true);
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
				Gui.DrawAvatar(painter, iconSize, iconSize, pixmap);
				
				if (IsHover) {
					// FIXME: Do something?
				}
				
				if (m_Grid.ListMode) {
					var rect = BoundingRect();
					var pen = new QPen();
					pen.SetBrush(m_Grid.Palette.Text());
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
			}
	
			protected override void MouseMoveEvent (Qyoto.QGraphicsSceneMouseEvent evnt)
			{
				var app = ((QApplication)QApplication.Instance());
				if (new QLineF(evnt.ScreenPos(), evnt.ButtonDownScreenPos(Qt.MouseButton.LeftButton))
				.Length() < app.StartDragDistance) {
					return;
				}
			
				if (m_Grid.Model is IAvatarGridEditableModel<T>) {
					QDrag drag = new QDrag(evnt.Widget());
					QMimeData mime = new RosterItemMimeData<T>(this, m_Grid);
					drag.SetMimeData(mime);

					QPixmap pixmap = new QPixmap(m_Grid.IconSize, m_Grid.IconSize);
					pixmap.Fill(m_Grid.Palette.Color(QPalette.ColorRole.Base));
					var painter = new QPainter(pixmap);
					Paint(painter, null, null);
					painter.End();
					drag.SetPixmap(pixmap);
					
		
					drag.Exec((uint)Qt.DropAction.MoveAction | (uint)Qt.DropAction.CopyAction | (uint)Qt.DropAction.IgnoreAction);
				}
			}

			protected override void MouseReleaseEvent (Qyoto.QGraphicsSceneMouseEvent arg1)
			{
				
			}

			public void BeginFade(bool fadeIn)
			{
				m_FadeAnimationTimeLine.Stop();
				m_FadeAnimation.FadeIn = fadeIn;
				m_FadeAnimationTimeLine.Start();
			}

			public void BeginMove(QPointF pos)
			{
				m_MoveAnimationTimeLine.Stop();
				m_MoveAnimation.SetPosAt(0, base.Pos());
				m_MoveAnimation.SetPosAt(1, pos);
				m_MoveAnimationTimeLine.Start();
			}
		}

		class RosterItemMimeData<T> : QMimeData
		{
			RosterItem<T> m_Item;
			
			public RosterItemMimeData (RosterItem<T> item, QObject parent)
			{
				m_Item = item;
				SetParent(parent);
			}

			public RosterItem<T> Item {
				get {
					return m_Item;
				}
			}
		}

	}
}
