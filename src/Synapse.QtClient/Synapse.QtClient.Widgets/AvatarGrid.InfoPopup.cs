//
// AvatarGrid.InfoPopup.cs
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
using System.Text;
using Qyoto;

namespace Synapse.QtClient.Widgets
{	
	public partial class AvatarGrid<T> : QGraphicsView
	{
		delegate void QPointEventHandler (QPoint pos);
		
		class InfoPopup<T> : QWidget
		{
			AvatarGrid<T>                m_Grid;
			QGraphicsScene               m_Scene;
			RosterItem<T>                m_Item;
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

			public RosterItem<T> Item {
				get {
					return m_Item;
				}
				set {
					if (m_Item == value)
						return;
					
					m_Item = value;
					if (m_Item != null) {
						QPixmap pixmap = (QPixmap)m_Grid.Model.GetImage(m_Item.Item);
						m_PixmapItem.Rect = new QRect(0, 0, m_Grid.IconSize, m_Grid.IconSize);
						m_PixmapItem.Pixmap = pixmap;
						m_PixmapItem.Update();

						var builder = new StringBuilder();
						builder.AppendFormat(@"<span style='font-size: 9pt; font-weight: bold'>{0}</span>",
					                         m_Grid.Model.GetName(m_Item.Item));
						                                              
 						builder.AppendFormat(@"<br/><span style='font-size: 7.5pt'>{0}</span>",
					                         m_Grid.Model.GetJID(m_Item.Item));

						string presenceInfo = m_Grid.Model.GetPresenceInfo(m_Item.Item);
						if (!String.IsNullOrEmpty(presenceInfo)) {
							builder.Append("<br/><span style='font-size: 7.5pt'>");
							builder.Append(presenceInfo.Replace("\n", "<br/>"));
							builder.Append("</span>");
						}
						
						m_Label.SetText(builder.ToString());
						
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
				}				
				return base.EventFilter (arg1, arg2);
			}
		}
	}
}
