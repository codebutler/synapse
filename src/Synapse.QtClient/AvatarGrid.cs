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
		
		Dictionary<string, RosterItemGroup> m_Groups = new Dictionary<string, RosterItemGroup>();
			
		int  m_IconWidth    = 32;
		int  m_HeaderHeight = 16;		
		bool m_ListMode     = false;

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
			m_InfoPopup.MouseMoved += delegate {
				UpdateHoverItem();
			};

			m_TooltipTimer = new QTimer(this);
			m_TooltipTimer.SingleShot = true;
			m_TooltipTimer.Interval = 500;
			QObject.Connect(m_TooltipTimer, Qt.SIGNAL("timeout()"), this, Qt.SLOT("tooltipTimer_timeout()"));

			this.InstallEventFilter(this);
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
		
		public int HeaderHeight {
			get {
				return m_HeaderHeight;
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

					// Empty group still has a header
					if (group.ChildItems().Count == 1) {
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
				RosterItemGroup group = new RosterItemGroup(this, groupName);
				group.SetVisible(false);
				m_Scene.AddItem(group);
				m_Groups.Add(groupName, group);

				if (resizeAndReposition)
					ResizeAndRepositionGroups();
			}
		}

		private void RemoveGroup (string groupName)
		{
			RosterItemGroup group = (RosterItemGroup) m_Groups[groupName];
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
			
			int groupY = IconPadding;

			int vScroll = this.VerticalScrollBar().Value;

			// Stop any existing move animations.
			m_MoveTimeLines.ForEach(t => t.Stop());
			m_MoveTimeLines.Clear();

			QTimeLine fadeTimeline = new QTimeLine(500);
			fadeTimeline.curveShape = QTimeLine.CurveShape.LinearCurve;
			
			QTimeLine moveTimeline = new QTimeLine(500);
			moveTimeline.curveShape = QTimeLine.CurveShape.LinearCurve;
		
			lock (m_Groups) {
				foreach (RosterItemGroup group in m_Groups.Values) {
					if (group.Y() != groupY) {
						if (!group.IsVisible() || (group.X() == 0 && group.Y() == 0)) {
							group.SetPos(0, groupY);
						} else {
							var groupMoveAnimation = new QGraphicsItemAnimation(moveTimeline);
							groupMoveAnimation.SetTimeLine(moveTimeline);
							groupMoveAnimation.SetItem(group);
							groupMoveAnimation.SetPosAt(1, new QPointF(0, groupY));
						}
					}
					
					int itemY = 0;
					
					var children = group.ChildItems();

					int visibleChildren = 0;
					foreach (QGraphicsItem child in children) {
						if (child is GraphicsRosterItem<T>) {
							if (m_Model.IsVisible(((GraphicsRosterItem<T>)child).Item)) {
								visibleChildren ++;
							}
						}
					}

					bool groupVisibilityChanged = false;
					bool groupVisible = visibleChildren > 0;
					if (group.IsVisible() != groupVisible) {
						// FIXME: This doesn't work correctly. Animation only ends up getting like 2 steps.
						var groupFadeAnimation = new FadeInOutAnimation(groupVisible, fadeTimeline);
						groupFadeAnimation.SetTimeLine(fadeTimeline);
						groupFadeAnimation.SetItem(group);
						groupVisibilityChanged = true;
					}
					
					if (groupVisible) {						
						itemY += m_HeaderHeight + IconPadding;
						
						int itemCount = children.Count;
						if (itemCount > 0) {
							int perRow = Math.Max((((int)m_Scene.Width() - IconPadding) / iconWidth), 1);

							if (m_ListMode)
								perRow = 1;
							
							int rows = Math.Max(itemCount / perRow, 1);
	
							int x       = IconPadding;
							int row     = 0;
							int thisRow = 0;

							// Arrange the items
							for (int n = 0; n < itemCount; n ++) {
								GraphicsRosterItem<T> item = (GraphicsRosterItem<T>)children[n];

								bool itemVisible = m_Model.IsVisible(item.Item) && group.IsExpanded;
								if (item.IsVisible() != itemVisible) {
									if (groupVisibilityChanged) {
										// No need to fade children in this case.
										item.SetVisible(itemVisible);
									} else {
										var fadeAnimation = new FadeInOutAnimation(itemVisible, fadeTimeline);
										fadeAnimation.SetTimeLine(fadeTimeline);
										fadeAnimation.SetItem(item);
									}
								}
								if (itemVisible) {
									// Move down to the next row if needed.
									if (thisRow == perRow) {
										row ++;
										thisRow = 0;
										x = IconPadding;
										itemY += iconHeight;
									}

									if (item.X() != x || item.Y() != itemY) {
										if (groupVisibilityChanged || !item.IsVisible() || (item.X() == 0 && item.Y() == 0)) {
											item.SetPos(x, itemY);
										} else {
											var moveAnimation = new QGraphicsItemAnimation(moveTimeline);
											moveAnimation.SetTimeLine(moveTimeline);
											moveAnimation.SetItem(item);
											moveAnimation.SetPosAt(1, new QPointF(x, itemY));
										}
									}
		
									x += iconWidth;
									thisRow ++;
								}
							}

							if (thisRow > 0)
								itemY += iconHeight;
						}
					}
					
					groupY += itemY;
				}
			}
			
			// Update the scene's height
			int newWidth = this.Viewport().Width();
			int newHeight = groupY + IconPadding;
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
			UpdateHoverItem();
			if (m_InfoPopup.Item != null) {
				m_InfoPopup.Show();
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
			UpdateHoverItem();
		}

		public override bool EventFilter (Qyoto.QObject arg1, Qyoto.QEvent arg2)
		{
			if (arg2.type() == QEvent.TypeOf.HoverLeave) {
				UpdateHoverItem();
			}
			return base.EventFilter (arg1, arg2);
		}

		void UpdateHoverItem()
		{			
			var oldItem = m_HoverItem;

			var pos = this.MapFromGlobal(QCursor.Pos());
			var item = this.ItemAt(pos);

			// Since we map the point to scene coords, we could accidently 
			// focus items outside the visible viewport.
			if (!this.Geometry.Contains(pos)) {
				m_HoverItem = null;
				m_InfoPopup.Item = null;
			} else {				
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
					
					// Allow a buffer around the active item so that the tooltip 
					// can change items without having to be closed/re-opened.
					if (m_InfoPopup.Item != null) {
						var itemPos = this.MapFromScene(m_InfoPopup.Item.X(), m_InfoPopup.Item.Y());
						QRectF rect = new QRectF(itemPos.X() - IconPadding,
						                         itemPos.Y() - IconPadding,
						                         m_InfoPopup.Item.BoundingRect().Width() + IconPadding + IconPadding,
						                         m_InfoPopup.Item.BoundingRect().Height() + IconPadding + IconPadding);
						if (!rect.Contains(pos)) {
							m_InfoPopup.Item = null;
						}
					}
				}
			}

			if (oldItem != null && oldItem != m_HoverItem) {
				oldItem.Update();
			}
		}

		class FadeInOutAnimation : QGraphicsItemAnimation
		{
			bool m_FadeIn;
			
			public FadeInOutAnimation (bool fadeIn, QObject parent) : base (parent)
			{
				m_FadeIn = fadeIn;
			}

			public new void SetItem (QGraphicsItem item)
			{
				var fadeItem = (IFadeable)item;
				if (!fadeItem.IsVisible() && m_FadeIn) {
					fadeItem.Opacity = 0;
					fadeItem.SetVisible(true);
					fadeItem.Update();
				}
				
				base.SetItem(item);
			}
			
			protected override void AfterAnimationStep (double step)
			{
				base.AfterAnimationStep (step);
				
				var opacity = m_FadeIn ? step : 1 - step;

				var fadeItem = (IFadeable)base.Item();
				fadeItem.Opacity = opacity;
				if (step == 1 && !m_FadeIn) {
					base.Item().SetVisible(false);
				}
			}
		}

		interface IFadeable : IQGraphicsItem
		{
			double Opacity {
				get;
				set;
			}
			
		}

		class RosterItemGroup : QGraphicsItemGroup, IFadeable
		{
			AvatarGrid<T> m_Grid;
			QFont         m_Font;
			string        m_GroupName;
			QRectF        m_Rect;
			bool          m_Expanded = true;
			double        m_Opacity = 1;
			
			public RosterItemGroup (AvatarGrid<T> grid, string groupName)
			{
				m_Grid      = grid;
				m_GroupName = groupName;
				
				m_Font = new QFont(m_Grid.Font);
				m_Font.SetPointSize(8); // FIXME: Set to m_Grid.HeaderHeight.
				m_Font.SetBold(true);
				
				m_Rect = new QRectF(m_Grid.IconPadding, 0, 0, 0);
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
			
			public override QRectF BoundingRect ()
			{
				m_Rect.SetWidth(m_Grid.Viewport().Width() - (m_Grid.IconPadding * 2));
				if (IsExpanded)
					m_Rect.SetHeight(base.ChildrenBoundingRect().Height());
				else
					m_Rect.SetHeight(m_Grid.HeaderHeight);							
				return m_Rect;
			}
			
			public override void Paint (Qyoto.QPainter painter, Qyoto.QStyleOptionGraphicsItem option, Qyoto.QWidget widget)
			{
				painter.SetOpacity(m_Opacity);

				// Group Name
				painter.SetFont(m_Font);
				painter.SetPen(new QPen(new QColor("#CCC")));
				painter.DrawText(BoundingRect(), m_GroupName);

				QFontMetrics metrics = new QFontMetrics(m_Font);
				int textWidth = metrics.Width(m_GroupName);

				// Group expander arrow
				painter.Save();
				painter.Translate(m_Grid.IconPadding + textWidth + 4, 5); // FIXME: These numbers probably shouldn't be hard coded.
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
				painter.SetPen(new QPen((new QColor("#CCC"))));
				painter.SetBrush(new QBrush(new QColor("#CCC")));				
				painter.DrawPath(path);
				painter.Restore();

				//painter.DrawRect(BoundingRect());
			}

			protected override void MousePressEvent (Qyoto.QGraphicsSceneMouseEvent arg1)
			{
				var pos = arg1.Pos();
				if (pos.Y() < m_Grid.HeaderHeight) {
					this.IsExpanded = !this.IsExpanded;
					m_Grid.ResizeAndRepositionGroups();
				}
				base.MousePressEvent (arg1);
			}
		}
		
		class GraphicsRosterItem<T> : QGraphicsItem, IFadeable
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

				// Parent opacity overrides item opacity.
				var parentGroup = (RosterItemGroup)base.Group();
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

			public event EventHandler MouseMoved;
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
						m_PixmapItem.Update();

						var text = String.Format(@"<span style='font-size: 9pt; font-weight: bold'>{0}</span>
						                         <br/><span style='font-size: 7.5pt'>{1}</span>
						                         <br/><span style='font-size: 7.5pt; color: #666'><b>{2}</b>{3}",
						                         m_Grid.Model.GetName(m_Item.Item),
						                         m_Grid.Model.GetJID(m_Item.Item),
						                         m_Grid.Model.GetPresence(m_Item.Item),
						                         m_Grid.Model.GetPresenceMessage(m_Item.Item));
						m_Label.SetText(text);
						
						m_Scene.SceneRect = m_Scene.ItemsBoundingRect();

						var itemRect = m_Item.SceneBoundingRect();
						
						var point = m_Grid.MapToGlobal(m_Grid.MapFromScene(itemRect.X(), itemRect.Y()));
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
				if (arg2.type() == QEvent.TypeOf.HoverMove) {
					if (MouseMoved != null)
						MouseMoved(this, EventArgs.Empty);
				} else if (arg2.type() == QEvent.TypeOf.ContextMenu) {
					var mouseEvent = (QContextMenuEvent)arg2;
					this.Hide();
					if (RightClicked != null) {
						RightClicked(this.MapToGlobal(mouseEvent.Pos()));
					}
					
				// This one isn't really needed.
				} else if (arg2.type() == QEvent.TypeOf.HoverLeave) {
					this.Hide();
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
