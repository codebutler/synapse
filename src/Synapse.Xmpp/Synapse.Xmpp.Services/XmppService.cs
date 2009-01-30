//
// XmppService.cs:
//
// Authors:
//   Eric Butler <eric@extremeboredom.net>
//
// Copyright (c) 2008 Eric Butler
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
using System.Collections.Generic;
using Synapse.Core;
using Synapse.Core.ExtensionMethods;
using Synapse.ServiceStack;
using Synapse.Services;
using Synapse.Xmpp;
using jabber.protocol.client;
using Mono.Addins;

namespace Synapse.Xmpp.Services
{	
	public class XmppService : IRequiredService, IInitializeService
	{
		public void Initialize ()
		{			
			// FIXME: This is not an ideal place for these. Perhaps create an XmppService?
			var feed = ServiceManager.Get<ActivityFeedService>();
			feed.AddTemplate("presence", "Presence Changes", "is now {0}", "are now {0}");
			feed.AddTemplate("music", "Friend Events", "is listening to", "are listening to");
			feed.AddTemplate("mood", "Friend Events", "is feeling {0}", "are feeling {0}");

			var joinMucAction = new NotificationAction() {
				Name = "join", 
				Label = "Join Conference",
				Callback = delegate (IActivityFeedItem item, NotificationAction action) {
					var xmppItem = (XmppActivityFeedItem)item;
					xmppItem.Account.JoinMuc(xmppItem.ActionItem);
				}
			};
			feed.AddTemplate("invite", null, "invites you to join {0}", "invites you to join {0}",
				new Dictionary<string, object> {
					{ "DesktopNotify", true },
					{ "ShowInMainWindow", true }
				}, joinMucAction
			);

			var approveAction = new NotificationAction {
				Name = "approve",
				Label = "Approve",
				Callback = delegate (IActivityFeedItem item, NotificationAction action) {
					var xmppItem = (XmppActivityFeedItem)item;

					var presence = new Presence(xmppItem.Account.Client.Document);
					presence.To = xmppItem.FromJid;
					presence.Type = PresenceType.subscribed;
					xmppItem.Account.Client.Write(presence);

					// FIXME: Show some sort of "friendname has been added!" notification?
					// FIXME: Should show AddFriendWindow instead so that nickname/groups can be set?
					xmppItem.Account.AddRosterItem(xmppItem.FromJid, null, null, null);
				}
			};

			var denyAction = new NotificationAction {
				Name = "deny",
				Label = "Deny",
				Callback = delegate (IActivityFeedItem item, NotificationAction action) {
					var xmppItem = (XmppActivityFeedItem)item;
					var presence = new Presence(xmppItem.Account.Client.Document);
					presence.To = xmppItem.FromJid;
					presence.Type = PresenceType.unsubscribed;
					xmppItem.Account.Client.Write(presence);
				}
			};
			
			feed.AddTemplate("subscribe", null, "wants to be friends with you", "want to friends with you", new Dictionary<string, object> {
				{ "DesktopNotify", true },
			 	{ "ShowInMainWindow", true }
			}, approveAction, denyAction);
			
			feed.AddTemplate("subscribed", null, "is now your friend", "are now your friend", new Dictionary<string, object> {
				{ "DesktopNotify", true },
			 	{ "ShowInMainWindow", true }
			});			

			feed.AddTemplate("unsubscribe", null, "is no longer friends with you", "are no longer friends with you", new Dictionary<string, object> {
				{ "DesktopNotify", true },
			 	{ "ShowInMainWindow", true }
			});

			feed.AddTemplate("unsubscribed", null, "is no longer your friend", "are no longer your friend", new Dictionary<string, object> {
				{ "DesktopNotify", true },
			 	{ "ShowInMainWindow", true }
			});

			feed.AddTemplate("account-error", null, "Error with {0}", null, new Dictionary<string, object> {
				{ "DesktopNotify", true },
			 	{ "ShowInMainWindow", true }
			});
			
			feed.AddTemplate("unknown-account-error", null, "Error with {0}", null, new Dictionary<string, object> {
				{ "DesktopNotify", true },
			 	{ "ShowInMainWindow", true }
			}, new [] {
				new NotificationAction { 
					Name     = "details",
					Label    = "Show Details", 
					Callback = delegate (IActivityFeedItem item, NotificationAction action) {
						var xmppItem = (XmppActivityFeedItem)item;
						Exception ex = (Exception)item.Data;
						Application.Client.ShowErrorWindow("Error with {0}".FormatWith(xmppItem.Account.Jid.Bare), ex);
					}
				} /*,
				new NotificationAction {
					Name = "bug",
					Label = "Report Bug",
					Callback = delegate (IActivityFeedItem item, NotificationAction action) {
						// FIXME:
					}
				} */
			});
		}

		public string ServiceName {
			get {
				return "XmppService";
			}
		}
	}
}
