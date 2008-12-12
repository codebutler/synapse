//
// AvatarGrid.cs
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
using System.Linq;
using System.Collections.Generic;
using Synapse.ServiceStack;
using Qyoto;
using Synapse.UI;
using Synapse.Xmpp;
using jabber.protocol.iq;

namespace Synapse.QtClient
{
	public delegate void AvatarGridItemEventHandler<T> (AvatarGrid<T> grid, T item);
	
	public class AvatarGrid<T> : QGraphicsView
	{
		IAvatarGridModel<T>   m_Model;
		QGraphicsScene        m_Scene;
		GraphicsRosterItem<T> m_HoverItem;
		List<QTimeLine>       m_FadeTimeLines = new List<QTimeLine>();
		List<QTimeLine>       m_MoveTimeLines = new List<QTimeLine>();
		InfoPopup<T>          m_InfoPopup;
			
		Dictionary<string, QGraphicsItemGroup> m_Groups = new Dictionary<string, QGraphicsItemGroup>();
			
		int m_IconWidth    = 32;
		int m_HeaderHeight = 16;
		int m_IconPadding  = 10;

		public event AvatarGridItemEventHandler<T> ItemActivated;
		
		public AvatarGrid(QWidget parent) : base(parent)
		{	
			m_Scene = new QGraphicsScene(this);
			this.SetScene(m_Scene);

			m_InfoPopup = new InfoPopup<T>(this);
			m_InfoPopup.DoubleClicked += delegate {
				MouseDoubleClickEvent(null);
			};
			m_InfoPopup.RightClicked += delegate (QPoint pos) {
				Emit.CustomContextMenuRequested(this.MapFromGlobal(pos));
			};
		}

		#region Public Properties
		public IAvatarGridModel<T> Model
		{
			set {
				if (m_Model != null)
					throw new InvalidOperationException("Model has already been set");

				m_Model = value;
				m_Model.ItemAdded   += model_ItemAdded;
				m_Model.ItemRemoved += model_ItemRemoved;
				m_Model.ItemChanged += model_ItemChanged;
				m_Model.Refreshed   += model_Refreshed;
				m_Model.ItemsChanged += model_ItemsChanged;
				model_Refreshed(null, EventArgs.Empty);
			}
			get {
				return m_Model;
			}
		}

		public T HoverItem {
			get {
				if (m_HoverItem != null)
					return m_HoverItem.Item;
				else
					return default(T);
			}
		}

		public int IconWidth {
			get {
				return m_IconWidth;
			}
			set {
				m_IconWidth = value;
				ResizeAndRepositionGroups();
			}
		}

		public int IconHeight {
			get {
				//return (m_IconWidth + 10);
				return m_IconWidth;
			}
		}		
		#endregion
				
		#region Model Events
		private void model_ItemAdded (IAvatarGridModel<T> model, T item)
		{
			bool updating = model.ModelUpdating;
			Application.Invoke(delegate {
				AddItem(item, !updating);
			});
		}
		
		private void model_ItemRemoved (IAvatarGridModel<T> model, T item)
		{
			Application.Invoke(delegate {
				List<string> groupsToRemove = new List<string>();
				foreach (var pair in m_Groups) {
					QGraphicsItemGroup group = pair.Value;
					foreach (QGraphicsItem gitem in group.Children()) {
						if (gitem is GraphicsRosterItem<T>) {
							if (((GraphicsRosterItem<T>)gitem).Item.Equals(item)) {
								group.RemoveFromGroup(gitem);
								break;
							}
						}
					}

					// Empty group has two items (name and arrow)
					if (group.ChildItems().Count == 2) {
						groupsToRemove.Add(pair.Key);
					}
				}
				groupsToRemove.ForEach(k => RemoveGroup(k));
				ResizeAndRepositionGroups();
			});
		}
		
		private void model_ItemChanged (IAvatarGridModel<T> model, T item)
		{
			Application.Invoke(delegate {
				foreach (QGraphicsItem gitem in m_Scene.Items()) {
					if (gitem is GraphicsRosterItem<T>) {
						if (((GraphicsRosterItem<T>)gitem).Item.Equals(item)) {
							if (model.IsVisible(item) != gitem.IsVisible()) {
								ResizeAndRepositionGroups();
								return;
		 					} else {
								gitem.Update();
							}
						}
					}
				}
			});
		}

		private void model_Refreshed (object o, EventArgs args)
		{
			Application.Invoke(delegate {
				m_Scene.Clear();
				m_Groups.Clear();
				
				foreach (var item in m_Model.Items) {
					AddItem(item, false);
				}
				
				ResizeAndRepositionGroups();
			});
		}

		void model_ItemsChanged (object o, EventArgs args)
		{
			Application.Invoke(delegate {
				ResizeAndRepositionGroups();
			});
		}
		
		#endregion

		#region Group Management

		private void AddGroup (string groupName, bool resizeAndReposition)
		{
			if (!m_Groups.ContainsKey(groupName)) {
				QGraphicsItemGroup group = new QGraphicsItemGroup();
				group.SetVisible(false);
				m_Scene.AddItem(group);
				m_Groups.Add(groupName, group);

				/*
				QLinearGradient gradient = new QLinearGradient(0, 0, 1, 0);
				gradient.SetCoordinateMode(QGradient.CoordinateMode.ObjectBoundingMode);
            	gradient.SetColorAt(0, new QColor("#4F4F4F"));
            	gradient.SetColorAt(1, Qt.GlobalColor.transparent);
				
				QGraphicsRectItem bgItem = new QGraphicsRectItem(group);
				bgItem.SetBrush(new QBrush(gradient));
				bgItem.SetPen(new QPen(Qt.PenStyle.NoPen));
				group.AddToGroup(bgItem);
				*/

				// Group Name
				QGraphicsSimpleTextItem nameItem = new QGraphicsSimpleTextItem(groupName, group);
				nameItem.SetBrush(new QBrush(new QColor("#CCC")));
				nameItem.SetZValue(100);
				QFont font = nameItem.Font();
				font.SetPointSize(8);
				font.SetBold(true);
				nameItem.SetFont(font);
				group.AddToGroup(nameItem);

				// Group expander arrow
				QPainterPath path = new QPainterPath();
				path.MoveTo(0, 0);
				path.LineTo(5, 0);
				path.LineTo(2, 3);
				path.LineTo(0, 0);
				QGraphicsPathItem arrowItem = new QGraphicsPathItem(path, group);
				arrowItem.SetPen(new QPen(Qt.GlobalColor.transparent));
				arrowItem.SetBrush(new QBrush(new QColor("#CCC")));
				group.AddToGroup(arrowItem);

				if (resizeAndReposition)
					ResizeAndRepositionGroups();
			}
		}

		private void RemoveGroup (string groupName)
		{
			QGraphicsItemGroup group = (QGraphicsItemGroup) m_Groups[groupName];
			m_Scene.RemoveItem(group);
			m_Groups.Remove(groupName);
			m_Scene.DestroyItemGroup(group);

			ResizeAndRepositionGroups();
		}
		
		#endregion

		private void ResizeAndRepositionGroups ()
		{
			int iconWidth  = (IconWidth + m_IconPadding);
			int iconHeight = (IconHeight + m_IconPadding);
			
			int y = m_IconPadding;

			int vScroll = this.VerticalScrollBar().Value;

			// Stop any existing move animations.
			m_MoveTimeLines.ForEach(t => t.Stop());
			m_MoveTimeLines.Clear();

			QTimeLine fadeTimeline = new QTimeLine(500);
			fadeTimeline.curveShape = QTimeLine.CurveShape.LinearCurve;
			
			QTimeLine moveTimeline = new QTimeLine(500);
			moveTimeline.curveShape = QTimeLine.CurveShape.LinearCurve;
		
			lock (m_Groups) {
				foreach (QGraphicsItemGroup group in m_Groups.Values) {
					var children = group.ChildItems();

					int visibleChildren = 0;
					foreach (QGraphicsItem child in children) {
						if (child is GraphicsRosterItem<T>) {
							if (m_Model.IsVisible(((GraphicsRosterItem<T>)child).Item)) {
								visibleChildren ++;
							}
						}
					}

					bool groupVisible = visibleChildren > 0;
					if (group.IsVisible() != groupVisible) {
						// FIXME: Fade in/out
						group.SetVisible(groupVisible);						
					}
					
					if (groupVisible) {
						//QGraphicsItemAnimation animation;
						
						// First two children are known to be these
						// FIXME: Create a GroupHeaderItem or something and merge these together.
						QGraphicsSimpleTextItem nameItem = (QGraphicsSimpleTextItem)children[0];
						nameItem.SetPos(m_IconPadding, y);
						
						QRectF nameItemRect = nameItem.BoundingRect();
						QGraphicsPathItem arrowItem  = (QGraphicsPathItem)children[1];
						arrowItem.SetPos(m_IconPadding + nameItemRect.Width() + 4, y + (nameItemRect.Height() / 2) - 2);
						
						y += m_HeaderHeight + m_IconPadding;
	
						// The rest of the children are items
						int itemCount = children.Count - 2;
						if (itemCount > 0) {
							int perRow = Math.Max((((int)m_Scene.Width() - m_IconPadding) / iconWidth), 1);
							int rows = Math.Max(itemCount / perRow, 1);
	
							int x       = m_IconPadding;
							int row     = 0;
							int thisRow = 0;

							// Arrange the items
							for (int n = 2; n < itemCount + 2; n ++) {
								GraphicsRosterItem<T> item = (GraphicsRosterItem<T>)children[n];

								bool itemVisible = m_Model.IsVisible(item.Item);
								if (item.IsVisible() != itemVisible) {
									var fadeAnimation = new RosterItemFadeInOutAnimation<T>(itemVisible, fadeTimeline);
									fadeAnimation.SetTimeLine(fadeTimeline);
									fadeAnimation.SetItem(item);
								}
								if (itemVisible) {
									// Move down to the next row if needed.
									if (thisRow == perRow) {
										row ++;
										thisRow = 0;
										x = m_IconPadding;
										y += iconHeight;
									}

									if (!item.IsVisible() || (item.X() == 0 && item.Y() == 0)) {
										item.SetPos(x, y);
									} else {
										var moveAnimation = new QGraphicsItemAnimation(moveTimeline);
										moveAnimation.SetTimeLine(moveTimeline);
										moveAnimation.SetItem(item);
										moveAnimation.SetPosAt(1, new QPointF(x, y));
									}
		
									x += iconWidth;
									thisRow ++;
								}
							}
						}
				
						y += iconHeight;
					}
				}
			}
			
			// Update the scene's height
			int newWidth = this.Viewport().Width();
			int newHeight = y + m_IconPadding;
			var currentRect = m_Scene.SceneRect;
			if (currentRect.Width() != newWidth || currentRect.Height() != newHeight) {
				m_Scene.SetSceneRect(0, 0, newWidth, newHeight);
			}

			// Restore the scroll position
			if (this.VerticalScrollBar().Value != vScroll) {
				this.VerticalScrollBar().SetValue(vScroll);
			}

			if (fadeTimeline.Children().Count() > 0)  {
				QObject.Connect(fadeTimeline, Qt.SIGNAL("finished()"), delegate {
					m_FadeTimeLines.Remove(fadeTimeline);
				});
				m_FadeTimeLines.Add(fadeTimeline);
				fadeTimeline.Start();
			}
			
			if (moveTimeline.Children().Count() > 0)  {
				QObject.Connect(moveTimeline, Qt.SIGNAL("finished()"), delegate {
					m_MoveTimeLines.Remove(moveTimeline);
				});
				m_MoveTimeLines.Add(moveTimeline);
				moveTimeline.Start();
			}
		}

		void AddItem (T item, bool resizeAndReposition)
		{
			var groups = m_Model.GetItemGroups(item);

			// FIXME: Is there some sort of "Standard" name for this?
			// Otherwise, perhaps they should be drawn outside of any group?
			if (groups.Count() == 0)
				groups = new string[] { "No Group" };
			
			lock (m_Groups) {
				foreach (string groupName in groups) {
					if (!m_Groups.ContainsKey(groupName))
						AddGroup(groupName, resizeAndReposition);
					
					QGraphicsItemGroup group = m_Groups[groupName];
					group.SetVisible(false);
	
					GraphicsRosterItem<T> graphicsItem = new GraphicsRosterItem<T>(this, item, (uint)IconWidth,
					                                                               (uint)IconHeight, group);
					graphicsItem.SetVisible(false);
					group.AddToGroup(graphicsItem);
				}

				if (resizeAndReposition)
					ResizeAndRepositionGroups();
			}			
		}
		
		protected override void ResizeEvent (QResizeEvent evnt)
		{
			ResizeAndRepositionGroups();
		}

		protected override void MouseDoubleClickEvent (Qyoto.QMouseEvent arg1)
		{
			if (m_HoverItem != null) {
				if (ItemActivated != null)
					ItemActivated(this, m_HoverItem.Item);
			}
		}
		
		protected override void MouseMoveEvent (Qyoto.QMouseEvent arg1)
		{
			base.MouseMoveEvent (arg1);
			
			var oldItem = m_HoverItem;
			
			var item = this.ItemAt(arg1.Pos());
			if (item is GraphicsRosterItem<T>) {				
				m_HoverItem = (GraphicsRosterItem<T>)item;
				m_HoverItem.Update();				
			} else {
				m_HoverItem = null;
			}
			
			m_InfoPopup.Item = m_HoverItem;

			if (oldItem != null) {
				oldItem.Update();
			}
		}

		class RosterItemFadeInOutAnimation<T> : QGraphicsItemAnimation
		{
			bool m_FadeIn;
			
			public RosterItemFadeInOutAnimation (bool fadeIn, QObject parent) : base (parent)
			{
				m_FadeIn = fadeIn;
			}

			public new void SetItem (QGraphicsItem item)
			{
				var gitem = (GraphicsRosterItem<T>)item;

				if (!gitem.IsVisible() && m_FadeIn) {
					gitem.Opacity = 0;
					gitem.SetVisible(true);
				}
				
				base.SetItem(item);
			}
			
			protected override void AfterAnimationStep (double step)
			{
				base.AfterAnimationStep (step);

				var opacity = m_FadeIn ? step : 1 - step;
				
				((GraphicsRosterItem<T>)base.Item()).Opacity = opacity;
				if (step == 1 && !m_FadeIn) {
					base.Item().SetVisible(false);
				}
			}
		}
		
		class GraphicsRosterItem<T> : QGraphicsItem
		{
			double        m_Opacity = 1;
			AvatarGrid<T> m_Grid;
			T             m_Item;
			QFont 		  m_Font;    // FIXME: Move this to the grid
			QFontMetrics  m_Metrics; // (this too)
			
			public GraphicsRosterItem (AvatarGrid<T> grid, T item, double width, double height, QGraphicsItem parent)
			{
				m_Grid    = grid;
				m_Item    = item;
				m_Font    = new QFont("Sans", 7);
				m_Metrics = new QFontMetrics(m_Font);
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
				QRectF boundingRect = BoundingRect();
				int width  = (int)boundingRect.Width();
				int height = (int)boundingRect.Height();

				painter.SetOpacity(m_Opacity);

				/*
				if (IsHover) {
					// FIXME: Do something prettier here.
					painter.Save();
					painter.SetBrush(new QBrush(Qt.GlobalColor.black));
					painter.DrawRect(0, 0, width, height);
					painter.Restore();
				}
				*/
				
				QPixmap pixmap = (QPixmap)m_Grid.Model.GetImage(m_Item);				
				if (pixmap != null)
					painter.DrawPixmap(0, 0, width, height, pixmap);
				else
					painter.DrawRect(0, 0, width, height);
			}

			// FIXME: This constantly crashes...
			public override QRectF BoundingRect ()
			{
				int width  = m_Grid.IconWidth;
				int height = m_Grid.IconHeight;
				return new QRectF(0, 0, width, height);
			}

			public bool IsHover {
				get {
					return (m_Grid.HoverItem != null && m_Grid.HoverItem.Equals(m_Item));
				}
			}
		}

		delegate void QPointEventHandler (QPoint pos);
		
		class InfoPopup<T> : QWidget
		{
			AvatarGrid<T>                m_Grid;
			QGraphicsScene               m_Scene;
			GraphicsRosterItem<T>        m_Item;
			ResizableGraphicsPixmapItem  m_PixmapItem;
		 	QLabel                       m_Label;
			MyGraphicsView               m_GraphicsView;

			public event QPointEventHandler RightClicked;
			
			public event EventHandler DoubleClicked {
				add {
					m_GraphicsView.DoubleClicked += value;
				}
				remove {
					m_GraphicsView.DoubleClicked -= value;
				}
			}
			
			public InfoPopup (AvatarGrid<T> grid)
			{
				m_Grid = grid;
				base.WindowFlags = (uint)Qt.WindowType.FramelessWindowHint | (uint)Qt.WindowType.ToolTip;
				base.Resize(260, 95);
				base.SetStyleSheet("background: black; color: white");

				m_GraphicsView = new MyGraphicsView(this);
				m_GraphicsView.FrameShape = QFrame.Shape.NoFrame;
				m_GraphicsView.HorizontalScrollBarPolicy = Qt.ScrollBarPolicy.ScrollBarAlwaysOff;
				m_GraphicsView.VerticalScrollBarPolicy = Qt.ScrollBarPolicy.ScrollBarAlwaysOff;
				m_GraphicsView.SetMaximumSize(60, 60);
				m_GraphicsView.SetMinimumSize(60, 60);
				
				m_Scene = new QGraphicsScene(m_GraphicsView);

				m_PixmapItem = new ResizableGraphicsPixmapItem();
				m_Scene.AddItem(m_PixmapItem);
				
				m_GraphicsView.SetScene(m_Scene);
				
				m_Label = new QLabel(this);
				m_Label.Alignment = (uint)Qt.AlignmentFlag.AlignTop | (uint)Qt.AlignmentFlag.AlignLeft;
				m_Label.TextFormat = Qt.TextFormat.RichText;
				m_Label.WordWrap = true;
				m_Label.SizePolicy = new QSizePolicy(QSizePolicy.Policy.Expanding, QSizePolicy.Policy.Expanding);

				var leftLayout = new QVBoxLayout();
				leftLayout.AddWidget(m_GraphicsView);
				leftLayout.AddStretch();
				
				var layout = new QHBoxLayout(this);
				layout.Margin = 6;
				layout.Spacing = 6;
				layout.AddLayout(leftLayout);
				layout.AddWidget(m_Label);
				
				this.InstallEventFilter(this);
			}

			public GraphicsRosterItem<T> Item {
				set {
					m_Item = value;
					if (m_Item != null) {
						QPixmap pixmap = (QPixmap)m_Grid.Model.GetImage(m_Item.Item);
						m_PixmapItem.Rect = new QRect(0, 0, m_Grid.IconWidth, m_Grid.IconHeight);
						m_PixmapItem.Pixmap = pixmap;

						var text = String.Format(@"<span style='font-size: 9pt; font-weight: bold'>{0}</span>
						                         <br/><span style='font-size: 7.5pt'>{1}</span>
						                         <br/><span style='font-size: 7.5pt; color: #666'><b>{2}</b>{3}",
						                         m_Grid.Model.GetName(m_Item.Item),
						                         m_Grid.Model.GetJID(m_Item.Item),
						                         m_Grid.Model.GetPresence(m_Item.Item),
						                         m_Grid.Model.GetPresenceMessage(m_Item.Item));
						m_Label.SetText(text);
						
						m_Scene.SceneRect = m_Scene.ItemsBoundingRect();
						
						var point = m_Grid.MapToGlobal(m_Grid.MapFromScene(m_Item.X(), m_Item.Y()));
						int x = point.X();
						int y = point.Y();

						x -= (60 / 2) - (m_Grid.IconWidth / 2) + 6;
						y -= (60 / 2) - (m_Grid.IconHeight / 2) + 6;						
						
						this.Move(x, y);
						this.Show();
					} else {
						this.Hide();
						m_Label.SetText(String.Empty);
						m_PixmapItem.Pixmap = null;
					}
				}
			}

			public bool EventFilter (Qyoto.QObject arg1, Qyoto.QEvent arg2)
			{
				if (arg2.type() == QEvent.TypeOf.HoverLeave) {
					this.Hide();
				} else if (arg2.type() == QEvent.TypeOf.HoverMove) {
					var mouseEvent = (QHoverEvent)arg2;
					// Beautiful...
					if (m_Grid.ItemAt(m_Grid.MapFromGlobal(this.MapToGlobal(mouseEvent.Pos()))) != m_Item) {
						this.Hide();
					}
				} else if (arg2.type() == QEvent.TypeOf.ContextMenu) {
					var mouseEvent = (QContextMenuEvent)arg2;
					this.Hide();
					if (RightClicked != null)
						RightClicked(this.MapToGlobal(mouseEvent.Pos()));
				}
				return base.EventFilter (arg1, arg2);
			}
		}
	}
	
	class MyGraphicsView : QGraphicsView
	{
		public event EventHandler DoubleClicked;

		public MyGraphicsView (QWidget parent) : base (parent)
		{
		}
		
		protected override void MouseDoubleClickEvent (Qyoto.QMouseEvent arg1)
		{
			this.ParentWidget().Hide();
			if (DoubleClicked != null)
				DoubleClicked(this, EventArgs.Empty);
		}
	}
}
