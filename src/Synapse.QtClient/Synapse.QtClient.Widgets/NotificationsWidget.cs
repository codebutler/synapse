//
// NotificationsWidget.cs
// 
// Copyright (C) 2009 Eric Butler
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
using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Services;
using Synapse.Xmpp.Services;
using Qyoto;

namespace Synapse.QtClient.Widgets
{	
	public class NotificationsWidget : QFrame
	{		
		public NotificationsWidget(QWidget parent) : base (parent)
		{
			base.Hide();

			var service = ServiceManager.Get<ActivityFeedService>();
			service.NewItem += HandleNewItem;

			QVBoxLayout layout = new QVBoxLayout(this);
			layout.Margin = 6;
			layout.Spacing = 3;
		}

		void HandleNewItem (IActivityFeedItem item)
		{
			var service = ServiceManager.Get<ActivityFeedService>();
			if (service.Templates[item.Type].ShowInMainWindow) {
				Application.Invoke(delegate {
					this.Layout().AddWidget(new NotificationItemWidget(item, this));
					this.Show();
				});
			}
		}

		class NotificationItemWidget : QFrame
		{
			IActivityFeedItem m_Item;

			public NotificationItemWidget(IActivityFeedItem item, QWidget parent) : base (parent)
			{
				m_Item = item;
				item.ActionTriggered += HandleActionTriggered;

				QHBoxLayout layout = new QHBoxLayout(this);
				layout.Margin = 3;

				var service = ServiceManager.Get<ActivityFeedService>();
				ActivityFeedItemTemplate template = service.Templates[item.Type];

				var builder = new StringBuilder();
				builder.AppendFormat("<span style=\"font-size: 9pt;\">");
				builder.AppendFormat("<b>{0}</b>", item.FromName);
				builder.Append(" ");
				builder.AppendFormat(template.SingularText, "<b>{0}</b>".FormatWith(item.ActionItem));
				
				if (!item.Content.Blank()) {
					builder.Append(":");
					builder.Append("<p style=\"margin-left: 20px; margin-top: 0px; margin-bottom: 0px;\">");
					builder.Append(item.Content);
					builder.Append("</p>");
				} else {
					builder.Append(".");
				}

				if (template.Actions != null && template.Actions.Length > 0) {
					builder.Append("<br/>");
					foreach (var action in template.Actions) {
						builder.AppendFormat("<a style=\"color: white\" href=\"{0}\">{1}</a> ", action.Name, action.Label);
					}
				}

				builder.Append("</span>");
				
				QLabel label = new QLabel(builder.ToString(), this);
				QObject.Connect(label, Qt.SIGNAL("linkActivated(const QString &)"), delegate (string link) {
					m_Item.TriggerAction(link);
				});

				label.WordWrap = true;
				layout.AddWidget(label, 1);
				
				if (item is XmppActivityFeedItem) {
					label.ToolTip = String.Format("From: {0}", ((XmppActivityFeedItem)item).FromJid);
				}
				
				QPushButton closeButton = new QPushButton(this);
				QObject.Connect(closeButton, Qt.SIGNAL("clicked()"), delegate {
					Close();
				});
				closeButton.icon = Gui.LoadIcon("window-close", 16);
				layout.AddWidget(closeButton, 0);
			}

			public void Close ()
			{
				m_Item.ActionTriggered -= HandleActionTriggered;

				if (this.ParentWidget().Layout().Count() == 1) {
					this.ParentWidget().Hide();
				}
				
				this.ParentWidget().Layout().RemoveWidget(this);
				this.SetParent(null);
			}

			void HandleActionTriggered (object o, NotificationAction action)
			{
				Close();
			}
		}
	}
}
