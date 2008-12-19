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

namespace Synapse.QtClient.Widgets
{
	public delegate void AvatarGridItemEventHandler<T> (AvatarGrid<T> grid, T item);
	
	public partial class AvatarGrid<T> : QGraphicsView
	{
		IAvatarGridModel<T>   m_Model;
		QGraphicsScene        m_Scene;
		RosterItem<T>         m_HoverItem;
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
						if (gitem is RosterItem<T>) {
							if (((RosterItem<T>)gitem).Item.Equals(item)) {
								group.RemoveFromGroup(gitem);
								m_Scene.RemoveItem(gitem);
								break;
							}
						}
					}

					if (group.ChildItems().Count == 0) {
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
					if (gitem is RosterItem<T>) {
						if (((RosterItem<T>)gitem).Item.Equals(item)) {
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
			m_Groups.Remove(groupName);

			foreach (var child in group.Children()) {
				m_Scene.RemoveItem(child);
			}
			m_Scene.DestroyItemGroup(group);

			ResizeAndRepositionGroups();
		}
		
		#endregion
		
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
		
		void ResizeAndRepositionGroups ()
		{
			int iconWidth  = (IconSize + IconPadding);
			int iconHeight = (IconSize + IconPadding);
			
			int groupY = 0;

			int vScroll = this.VerticalScrollBar().Value;

			// FIXME: This lock is causing a compiler error!
			//lock (m_Groups) {
				// Stop any existing move animations.
				m_MoveTimeLines.ForEach(t => t.Stop());
				m_MoveTimeLines.Clear();
	
				QTimeLine fadeTimeline = new QTimeLine(500);
				fadeTimeline.curveShape = QTimeLine.CurveShape.LinearCurve;
				
				QTimeLine moveTimeline = new QTimeLine(500);
				moveTimeline.curveShape = QTimeLine.CurveShape.LinearCurve;
		
				foreach (RosterItemGroup group in m_Groups.Values) {					
					int itemY = 0;
					
					var children = group.ChildItems();

					int visibleChildren = 0;
					foreach (QGraphicsItem child in children) {
						if (child is RosterItem<T>) {
							if (m_Model.IsVisible(((RosterItem<T>)child).Item)) {
								visibleChildren ++;
							}
						}
					}

					bool groupVisibilityChanged = false;
					bool groupVisible = visibleChildren > 0;
					if (group.IsVisible() != groupVisible) {
						var groupFadeAnimation = new FadeInOutAnimation(groupVisible, fadeTimeline);
						groupFadeAnimation.SetTimeLine(fadeTimeline);
						groupFadeAnimation.SetItem(group);
						groupVisibilityChanged = true;
					}

					if (group.Y() != groupY) {
						if (groupVisibilityChanged || !group.IsVisible() || (group.X() == 0 && group.Y() == 0)) {
							group.SetPos(0, groupY);
						} else {
							var groupMoveAnimation = new QGraphicsItemAnimation(moveTimeline);
							groupMoveAnimation.SetTimeLine(moveTimeline);
							groupMoveAnimation.SetItem(group);
							groupMoveAnimation.SetPosAt(1, new QPointF(0, groupY));
						}
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
								RosterItem<T> item = (RosterItem<T>)children[n];

								bool itemVisible = m_Model.IsVisible(item.Item) && group.IsExpanded;
								if (item.IsVisible() != itemVisible) {
									if (groupVisibilityChanged) {
										// No need to fade children in this case.
										item.Opacity = itemVisible ? 1 : 0;
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
			//}
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
	
					RosterItem<T> graphicsItem = new RosterItem<T>(this, item, (uint)IconSize,
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
				if (item is RosterItem<T>) {
					m_HoverItem = (RosterItem<T>)item;
					m_HoverItem.Update();
	
					if (m_InfoPopup.Item != m_HoverItem) {
						if (m_InfoPopup.IsVisible()) {
							m_InfoPopup.Item = m_HoverItem;
						} else {
							m_TooltipTimer.Stop();
							m_InfoPopup.Item = m_HoverItem;
							m_TooltipTimer.Start();
						}
					}
				} else {	
					m_TooltipTimer.Stop();
					m_HoverItem = null;
					
					// Allow a buffer around the active item so that the tooltip 
					// can change items without having to be closed/re-opened.
					if (m_InfoPopup.Item != null) {
						var itemRect = m_InfoPopup.Item.SceneBoundingRect();
						var itemPos = this.MapFromScene(itemRect.X(), itemRect.Y());
						QRectF rect = new QRectF(itemPos.X() - IconPadding,
						                         itemPos.Y() - IconPadding,
						                         itemRect.Width() + IconPadding + IconPadding,
						                         itemRect.Height() + IconPadding + IconPadding);
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
