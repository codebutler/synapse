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
using Synapse.UI;
using Qyoto;

namespace Synapse.QtClient.Widgets
{	
	public partial class AvatarGrid<T> : QGraphicsView
	{
		class RosterItemGroup : QGraphicsItemGroup, IFadableItem
		{
			AvatarGrid<T> m_Grid;
			QFont         m_Font;
			QFontMetrics  m_Metrics;
			string        m_GroupName;
			QRectF        m_Rect;
			bool          m_Expanded = true;
			double        m_Opacity = 1;
			int           m_RowCount;
			QRectF        m_ArrowRect;
			bool          m_LeftButtonDown = false;
			bool          m_ItemOver = false;
			
			QTimeLine m_MoveAnimationTimeLine;
			QGraphicsItemAnimation m_MoveAnimation;
			
			QTimeLine m_FadeAnimationTimeLine;
			FadeInOutAnimation m_FadeAnimation;
			
			public RosterItemGroup (AvatarGrid<T> grid, string groupName)
			{
				m_Grid      = grid;
				m_GroupName = groupName;
				
				m_MoveAnimationTimeLine = new QTimeLine(500);
				
				m_MoveAnimation = new QGraphicsItemAnimation();
				m_MoveAnimation.SetItem(this);
				m_MoveAnimation.SetTimeLine(m_MoveAnimationTimeLine);				
								
				m_FadeAnimationTimeLine = new QTimeLine(500);
				
				m_FadeAnimation = new FadeInOutAnimation();
				m_FadeAnimation.SetItem(this);
				m_FadeAnimation.SetTimeLine(m_FadeAnimationTimeLine);
				
				m_Font = new QFont(m_Grid.Font);
				m_Font.SetPointSize(8); // FIXME: Set to m_Grid.HeaderHeight.
				m_Font.SetBold(true);
				
				m_Metrics = new QFontMetrics(m_Font);
				
				m_Rect = new QRectF(m_Grid.IconPadding, 0, 0, 0);

				base.SetHandlesChildEvents(false);
				base.SetAcceptHoverEvents(true);
				base.SetAcceptDrops(true);
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
					return m_Grid.AllGroupsCollapsed ? false : m_Expanded;
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

			public void Remove ()
			{
				m_FadeAnimationTimeLine.Stop();
				m_MoveAnimationTimeLine.Stop();
				m_FadeAnimation = null;
				m_FadeAnimationTimeLine = null;
				m_MoveAnimation = null;
				m_MoveAnimationTimeLine = null;

				lock (m_Grid.m_Groups) {
					m_Grid.m_Groups.Remove(this.Name);
				}

				foreach (var child in base.Children()) {
					if (child is RosterItem<T>) {
						((RosterItem<T>)child).Remove(false);
					}
				}

				m_Grid.Scene().DestroyItemGroup(this);

				m_Grid.ResizeAndRepositionGroups();
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
					m_Rect.SetHeight(m_Grid.HeaderHeight + (m_RowCount * (m_Grid.IconSize + m_Grid.IconPadding)));
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

				string text = null;
				if (m_Grid.ShowGroupCounts) {
					text = String.Format("{0} ({1}/{2})", 
					                     m_GroupName,
					                     m_Grid.Model.NumOnlineItemsInGroup(m_GroupName),
					                     m_Grid.Model.NumItemsInGroup(m_GroupName));
				} else {
					text = m_GroupName;
				}

				painter.DrawText(BoundingRect(), text);
				
				int arrowX = m_Grid.IconPadding + m_Metrics.Width(text) + 4;
				int arrowY = 5;
				
				// Group expander arrow
				painter.Save();
				painter.Translate(arrowX, arrowY);
				QPainterPath path = new QPainterPath();
				if (IsExpanded) {
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

				m_ArrowRect = new QRectF(arrowX, 0, 4,  m_Grid.HeaderHeight);

				if (ItemOver) {
					painter.SetPen(new QPen(new QColor("red")));
					painter.DrawRect(BoundingRect());
				}
			}

			protected override void MouseReleaseEvent (Qyoto.QGraphicsSceneMouseEvent arg1)
			{
				if (arg1.Button() == Qt.MouseButton.LeftButton) {
					m_LeftButtonDown = false;
					var pos = arg1.Pos();
					var pos1 = arg1.ButtonDownPos(Qt.MouseButton.LeftButton);
					if (pos != null && pos1 != null && m_ArrowRect.Contains(pos1) && pos.Y() < m_Grid.HeaderHeight && pos1.Equals(pos)) {
						this.IsExpanded = !this.IsExpanded;
						m_Grid.ResizeAndRepositionGroups();
					}
				}
			}

			protected override void MousePressEvent (Qyoto.QGraphicsSceneMouseEvent arg1)
			{
				if (arg1.Button() == Qt.MouseButton.LeftButton)
					m_LeftButtonDown = true;
			}

			protected override void MouseMoveEvent (Qyoto.QGraphicsSceneMouseEvent evnt)
			{
				if (m_LeftButtonDown) {

					if (evnt.Pos().Y() > m_Grid.HeaderHeight)
						return;
					
					var app = ((QApplication)QApplication.Instance());
					if (new QLineF(evnt.ScreenPos(), evnt.ButtonDownScreenPos(Qt.MouseButton.LeftButton))
					.Length() < app.StartDragDistance) {
						return;
					}

					if (m_Grid.Model is IAvatarGridEditableModel<T>) {
						QDrag drag = new QDrag(evnt.Widget());
						drag.SetHotSpot(evnt.Pos().ToPoint());
	
						var mime = new RosterItemGroupMimeData(this, m_Grid);
						drag.SetMimeData(mime);
		
						var pixmap = new QPixmap((int)BoundingRect().Width(), m_Grid.HeaderHeight);
						pixmap.Fill(m_Grid.Palette.Color(QPalette.ColorRole.Base));
						var painter = new QPainter(pixmap);
						Paint(painter, null, null);
						painter.End();
						drag.SetPixmap(pixmap);
			
						drag.Exec();
					}
				}
			}
	
			protected override void DragMoveEvent (Qyoto.QGraphicsSceneDragDropEvent arg1)
			{
				if (arg1.MimeData() is RosterItemMimeData<T>) {
					arg1.Accept();
				} else {
					arg1.Ignore();
				}
			}

			protected override void DragEnterEvent (Qyoto.QGraphicsSceneDragDropEvent arg1)
			{
				if (arg1.MimeData() is RosterItemMimeData<T>)
					ItemOver = true;
			}
			
			protected override void DragLeaveEvent (Qyoto.QGraphicsSceneDragDropEvent arg1)
			{
				if (arg1.MimeData() is RosterItemMimeData<T>)
					ItemOver = false;
			}
			
			protected override void DropEvent (Qyoto.QGraphicsSceneDragDropEvent arg1)
			{
				if (arg1.MimeData() is RosterItemMimeData<T>) {
					arg1.Accept();

					ItemOver = false;
					
					var mimeData = (RosterItemMimeData<T>)arg1.MimeData();
					var oldGroup = (RosterItemGroup)mimeData.Item.ParentItem();

					var editableModel = (IAvatarGridEditableModel<T>)m_Grid.Model;					
					editableModel.AddItemToGroup(mimeData.Item.Item, this.Name);
					if (arg1.DropAction() != Qt.DropAction.CopyAction) {
						editableModel.RemoveItemFromGroup(mimeData.Item.Item, oldGroup.Name);
					}
					
				} else {
					arg1.Ignore();
				}
			}

			bool ItemOver {
				get {
					return m_ItemOver;
				}
				set {
					m_ItemOver = value;
					base.Update();
				}
			}
		}

		class RosterItemGroupMimeData : QMimeData
		{
			RosterItemGroup m_Group;
			
			public RosterItemGroupMimeData (RosterItemGroup group, QObject parent)
			{
				m_Group = group;
				SetParent(parent);
			}

			public RosterItemGroup Group {
				get {
					return m_Group;
				}
			}
		}
	}
}
