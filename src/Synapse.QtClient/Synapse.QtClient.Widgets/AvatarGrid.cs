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
		IAvatarGridModel<T>  m_Model;
		QGraphicsScene       m_Scene;
		RosterItem<T>        m_HoverItem;
		InfoPopup<T>         m_InfoPopup;
		QTimer               m_TooltipTimer;
		
		Dictionary<string, RosterItemGroup> m_Groups = new Dictionary<string, RosterItemGroup>();
		
		// item => { groupName => rosterItem }
		Dictionary<T, Dictionary<string, RosterItem<T>>> m_Items = new Dictionary<T, Dictionary<string, RosterItem<T>>>();
		
		int  m_IconWidth    = 32;
		int  m_HeaderHeight = 16;		
		bool m_ListMode     = false;
		string m_LastTextFilter = null;

		bool m_AllGroupsCollapsed = false;

		bool m_SuppressTooltips = false;
		
		public event AvatarGridItemEventHandler<T> ItemActivated;
		
		public AvatarGrid(QWidget parent) : base(parent)
		{
			// FIXME: Need a preference to turn this on/off.
			// this.SetViewport(new QGLWidget());
			
			m_Scene = new Scene(this);
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

			this.AcceptDrops = true;
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
				if (value < 16)
					m_IconWidth = 16;
				else if (value > 60)
					m_IconWidth = 60;
				else
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

		public bool ShowGroupCounts {
			get;
			set;
		}

		public bool SuppressTooltips {
			get {
				return m_SuppressTooltips;
			}
			set {
				m_SuppressTooltips = value;
				UpdateHoverItem();
			}
		}
		#endregion
				
		#region Model Events
		private void model_ItemAdded (IAvatarGridModel<T> model, T item)
		{
			if (!model.ModelUpdating) {
				Application.Invoke(delegate {
					AddItem(item);
					ResizeAndRepositionGroups();
				});
			}
		}
		
		private void model_ItemRemoved (IAvatarGridModel<T> model, T item)
		{
			if (!model.ModelUpdating) {
				Application.Invoke(delegate {
					RemoveItem(item);
				});
			}
		}
		
		private void model_ItemChanged (IAvatarGridModel<T> model, T item)
		{
			if (model.ModelUpdating) {
				return;
			}

			var s = Environment.StackTrace;
			Application.Invoke(delegate {
				bool visibilityChanged = false;
				bool groupsChanged = false;

				List<string> toRemove = new List<string>();

				// Check if item was added to any groups
				foreach (string groupName in model.GetItemGroups(item)) {
					if (!m_Items[item].ContainsKey(groupName)) {
						AddItemToGroup(item, groupName);
						groupsChanged = true;
					}
				}
				
				// Check if item needs to be removed from any groups, redraw others.
				// FIXME: Don't need to redraw items we just added.
				lock (m_Items) {					
					foreach (RosterItem<T> gitem in m_Items[item].Values) {
						RosterItemGroup group = (RosterItemGroup)gitem.ParentItem();
						var groups = model.GetItemGroups(item);
						if (groups.Contains(group.Name) || (groups.Count() == 0 && group.Name == "No Group")) {
							if (model.IsVisible(item) != gitem.IsVisible()) {
								visibilityChanged = true;
							} else {
								gitem.Update();
							}
						} else {
							toRemove.Add(group.Name);
							groupsChanged = true;
						}
					}
				}
				
				foreach (string groupName in toRemove) {
					RemoveItemFromGroup(item, groupName, false);
				}

				if (visibilityChanged || groupsChanged) {
					ResizeAndRepositionGroups();
				}				
			});
		}

		private void model_Refreshed (object o, EventArgs args)
		{
			Application.Invoke(delegate {
				Console.WriteLine("Model Refreshed");
				
				lock (m_Groups) {
					foreach (var groupItem in m_Groups.Values.ToArray()) {
						RemoveGroup(groupItem);
					}
					if (m_Groups.Count > 0) {
						throw new Exception(String.Format("Something went wrong, groups should be empty, had {0}!", m_Groups.Count));
					}
				}
				
				foreach (var item in m_Model.Items) {
					AddItem(item);
				}
				
				ResizeAndRepositionGroups();

				Console.WriteLine("End Model Refreshed");
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

		private void AddGroup (string groupName)
		{
			lock (m_Groups) {
				if (!m_Groups.ContainsKey(groupName)) {
					RosterItemGroup group = new RosterItemGroup(this, groupName);
					group.SetVisible(false);
					m_Scene.AddItem(group);
					m_Groups.Add(groupName, group);

					ResizeAndRepositionGroups();
				}
			}
		}

		private void RemoveGroup (string groupName)
		{
			RosterItemGroup group = (RosterItemGroup) m_Groups[groupName];
			RemoveGroup(group);
		}

		void RemoveGroup (RosterItemGroup group)
		{		
			m_Groups.Remove(group.Name);

			foreach (var child in group.Children()) {
				if (child is RosterItem<T>)
					RemoveItemFromGroup(((RosterItem<T>)child).Item, group.Name, false);
			}
			m_Scene.DestroyItemGroup(group);

			ResizeAndRepositionGroups();
		}

		IEnumerable<RosterItemGroup> SortedGroups {
			get {
				return m_Groups.Values.OrderBy(g => m_Model.GetGroupOrder(g.Name));
			}
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

		protected override void MouseReleaseEvent (Qyoto.QMouseEvent arg1)
		{
			base.MouseReleaseEvent(arg1);
		}
		
		protected override void MouseMoveEvent (Qyoto.QMouseEvent arg1)
		{
			base.MouseMoveEvent(arg1);
			UpdateHoverItem();
		}

		bool AllGroupsCollapsed {
			get {
				return m_AllGroupsCollapsed;
			}
			set {
				if (m_AllGroupsCollapsed != value) {
					m_AllGroupsCollapsed = value;
					ResizeAndRepositionGroups();
				}
				SuppressTooltips = value;
			}
		}
		
		void ResizeAndRepositionGroups ()
		{			
			int iconWidth  = (IconSize + IconPadding);
			int iconHeight = (IconSize + IconPadding);
			
			int groupY = IconPadding;
			int vScroll = this.VerticalScrollBar().Value;

			int newWidth = this.Viewport().Width();
			
			bool filterChanged = (m_LastTextFilter != m_Model.TextFilter);
			
			lock (m_Groups) {
				foreach (RosterItemGroup group in this.SortedGroups) {
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
						if (filterChanged) {
							group.SetVisible(groupVisible);
						} else {
							group.BeginFade(groupVisible);
						}
					}

					if (group.Y() != groupY) {
						if (groupVisibilityChanged || !group.IsVisible() || (group.X() == 0 && group.Y() == 0) || filterChanged) {
							group.SetPos(0, groupY);
						} else {
							group.BeginMove(new QPointF(0, groupY));
						}
					}
					
					if (groupVisible) {						
						itemY += m_HeaderHeight + IconPadding;
						
						int itemCount = children.Count;
						if (itemCount > 0) {
							int perRow = Math.Max((((int)newWidth - IconPadding) / iconWidth), 1);
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
										item.BeginFade(itemVisible);
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
										if (groupVisibilityChanged || !item.IsVisible() || (item.X() == 0 && item.Y() == 0) || filterChanged) {
											item.SetPos(x, itemY);
										} else {
											item.BeginMove(new QPointF(x, itemY));
										}
									}
		
									x += iconWidth;
									thisRow ++;
								}
							}

							if (thisRow > 0)
								itemY += iconHeight;

							group.RowCount = row + 1;
						} else {
							group.RowCount = 0;
						}
					} else {
						group.RowCount = 0;
					}
					
					groupY += itemY;
				}
								
				// Update the scene's height
				int newHeight = groupY;
				var currentRect = m_Scene.SceneRect;
				if (currentRect.Width() != newWidth || currentRect.Height() != newHeight) {
					m_Scene.SetSceneRect(0, 0, newWidth, newHeight);
				}
	
				// Restore the scroll position
				if (this.VerticalScrollBar().Value != vScroll) {
					this.VerticalScrollBar().SetValue(vScroll);
				}
			}
			
			m_LastTextFilter = m_Model.TextFilter;
		}

		void AddItem (T item)
		{			
			var groups = m_Model.GetItemGroups(item);
		
			foreach (string groupName in groups) {
				AddItemToGroup(item, groupName);
			}
		}

		void AddItemToGroup (T item, string groupName)
		{
			lock (m_Groups) {
				if (!m_Groups.ContainsKey(groupName))
					AddGroup(groupName);
			}

			lock (m_Items) {
				if (!m_Items.ContainsKey(item))
					m_Items.Add(item, new Dictionary<string, RosterItem<T>>());

				// This should *never* happen.
				if (m_Items[item].ContainsKey(groupName)) {
					string err = String.Format("FIXME: '{0}' is already in group '{1}'!", m_Model.GetJID(item), groupName);
					if (item is RosterItem) {
						err += " All Groups: " + String.Join(",", m_Model.GetItemGroups(item).ToArray());
					}
				    throw new Exception(err);
				}
				    
				QGraphicsItemGroup group = m_Groups[groupName];
				RosterItem<T> graphicsItem = new RosterItem<T>(this, item, (uint)IconSize, (uint)IconSize, group);
				graphicsItem.SetVisible(false);
				group.AddToGroup(graphicsItem);
				m_Items[item].Add(groupName, graphicsItem);
			}
		}

		void RemoveItemFromGroup (T item, string groupName, bool resizeAndReposition)
		{			
			lock (m_Items) {
				var graphicsItem = m_Items[item][groupName];
				var group = (RosterItemGroup)graphicsItem.ParentItem();
				group.RemoveFromGroup(graphicsItem);
				m_Scene.RemoveItem(graphicsItem);
				m_Items[item].Remove(groupName);
				if (m_Items[item].Count == 0)
					m_Items.Remove(item);
			}

			if (resizeAndReposition)
				ResizeAndRepositionGroups();
		}

		void RemoveItem (T item)
		{
			foreach (var gitem in m_Items[item].Values) {
				var group = (RosterItemGroup)gitem.ParentItem();
				group.RemoveFromGroup(gitem);
				m_Scene.RemoveItem(gitem);
			}
			
			m_Items.Remove(item);

			ResizeAndRepositionGroups();

			// FIXME: stuck in a loop
			//if (group.ChildItems().Count == 0) {
			//	RemoveGroup(group);
			//}
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
			if (m_SuppressTooltips || !this.Viewport().Geometry.Contains(pos) || !this.IsVisible()) {
				m_TooltipTimer.Stop();
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
}
