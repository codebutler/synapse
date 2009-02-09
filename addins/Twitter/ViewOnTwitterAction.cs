//
// ViewOnTwitterAction.cs
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

using Synapse.Core;
using Synapse.Xmpp;
using Synapse.UI;
using Synapse.QtClient;
using Synapse.QtClient.Windows;
using Synapse.QtClient.Widgets;

using Qyoto;

namespace Synapse.Addins.Twitter
{	
	public class ViewOnTwitterAction : QAction, IUpdateableAction
	{
		public ViewOnTwitterAction(QWidget parent) : base (parent)
		{
			base.Text = "View on Twitter";
			base.icon = new QIcon(new QPixmap("resource:/twitter/twitm-16.png"));
			
			QObject.Connect(this, Qt.SIGNAL("triggered(bool)"), this, Qt.SLOT("on_triggered(bool)"));
		}
		
		public void Update ()
		{
			RosterItem item = ((RosterWidget)base.Parent()).SelectedItem;
			string twitterId = item.Account.GetFeature<UserWebIdentities>().GetIdentity(item.Item.JID, "twitter");
			base.Visible = !String.IsNullOrEmpty(twitterId);
		}
		
		[Q_SLOT]
		void on_triggered (bool isChecked)
		{
			RosterItem item = ((RosterWidget)base.Parent()).SelectedItem;
			string twitterId = item.Account.GetFeature<UserWebIdentities>().GetIdentity(item.Item.JID, "twitter");
			Util.Open("http://twitter.com/" + twitterId);
		}
	}
}
