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
		QTimer                m_TooltipTimer;
		
		Dictionary<string, QGraphicsItemGroup> m_Groups = new Dictionary<string, QGraphicsItemGroup>();
			
		int m_IconWidth    = 32;
		int m_HeaderHeight = 16;
		
		bool m_ListMode =  false;

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

			m_TooltipTimer = new QTimer(this);
			m_TooltipTimer.SingleShot = true;
			m_TooltipTimer.Interval = 500;
			QObject.Connect(m_TooltipTimer, Qt.SIGNAL("timeout()"), this, Qt.SLOT("tooltipTimer_timeout()"));
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

		public bool ListMode {
			get {
				return m_ListMode;
			}
			set {
				m_ListMode = value;
				ResizeAndRepositionGroups();
			}
		}
		
		public int IconSize {
			get {
				return m_IconWidth;
			}
			set {
				m_IconWidth = value;
				ResizeAndRepositionGroups();
			}
		}

		public int IconPadding {
			get {
				return m_IconWidth / 4;
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
			int iconWidth  = (IconSize + IconPadding);
			int iconHeight = (IconSize + IconPadding);
			
			int y = IconPadding;

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
						nameItem.SetPos(IconPadding, y);
						
						QRectF nameItemRect = nameItem.BoundingRect();
						QGraphicsPathItem arrowItem  = (QGraphicsPathItem)children[1];
						arrowItem.SetPos(IconPadding + nameItemRect.Width() + 4, y + (nameItemRect.Height() / 2) - 2);
						
						y += m_HeaderHeight + IconPadding;
	
						// The rest of the children are items
						int itemCount = children.Count - 2;
						if (itemCount > 0) {
							int perRow = Math.Max((((int)m_Scene.Width() - IconPadding) / iconWidth), 1);

							if (m_ListMode)
								perRow = 1;
							
							int rows = Math.Max(itemCount / perRow, 1);
	
							int x       = IconPadding;
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
										x = IconPadding;
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
			int newHeight = y + IconPadding;
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
	
					GraphicsRosterItem<T> graphicsItem = new GraphicsRosterItem<T>(this, item, (uint)IconSize,
					                                                               (uint)IconSize, group);
					graphicsItem.SetVisible(false);
					group.AddToGroup(graphicsItem);
				}

				if (resizeAndReposition)
					ResizeAndRepositionGroups();
			}			
		}

		[Q_SLOT]
		void tooltipTimer_timeout()
		{			
			m_InfoPopup.Show();
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

			Console.WriteLine(arg1.Pos().X() + " " + arg1.Pos().Y());
			
			var oldItem = m_HoverItem;
			
			var item = this.ItemAt(arg1.Pos());
			if (item is GraphicsRosterItem<T>) {
				m_HoverItem = (GraphicsRosterItem<T>)item;
				m_HoverItem.Update();

				if (m_InfoPopup.Item != m_HoverItem) {
					m_TooltipTimer.Stop();
					m_InfoPopup.Item = m_HoverItem;					
					m_TooltipTimer.Start();
				}
			} else {
				m_TooltipTimer.Stop();
				m_HoverItem = null;
				m_InfoPopup.Item = null;
			}

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
			QRectF        m_Rect;
			
			public GraphicsRosterItem (AvatarGrid<T> grid, T item, double width, double height, QGraphicsItem parent)
			{
				m_Grid    = grid;
				m_Item    = item;
				m_Rect = new QRectF(0, 0, 0, 0);
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
				int iconSize  = m_Grid.IconSize;

				painter.SetOpacity(m_Opacity);
				
				QPixmap pixmap = (QPixmap)m_Grid.Model.GetImage(m_Item);				
				if (pixmap != null)
					painter.DrawPixmap(0, 0, iconSize, iconSize, pixmap);
				else
					painter.DrawRect(0, 0, iconSize, iconSize);

				/*
				if (IsHover) {
					painter.DrawRect(BoundingRect());
				}
				*/
				
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
				get {
					return m_Item;
				}
				set {
					m_Item = value;
					if (m_Item != null) {
						QPixmap pixmap = (QPixmap)m_Grid.Model.GetImage(m_Item.Item);
						m_PixmapItem.Rect = new QRect(0, 0, m_Grid.IconSize, m_Grid.IconSize);
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

						x -= (60 / 2) - (m_Grid.IconSize / 2) + 6;
						y -= (60 / 2) - (m_Grid.IconSize / 2) + 6;						
						
						this.Move(x, y);
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
