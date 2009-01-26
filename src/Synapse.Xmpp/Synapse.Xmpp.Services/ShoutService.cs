//
// ShoutService.cs
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
using System.Collections.Generic;
using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Services;
using Synapse.Xmpp;
using Mono.Addins;

namespace Synapse.Xmpp.Services
{
	public class ShoutService : IRequiredService, IInitializeService
	{
		List<IShoutHandler> m_ShoutHandlers = new List<IShoutHandler>();
		
		public void Initialize ()
		{
			var feed = ServiceManager.Get<ActivityFeedService>();
			feed.AddTemplate("shout", "shouts", "shout");
			
			var nodes = AddinManager.GetExtensionNodes("/Synapse/Xmpp/ActivityFeed/ShoutHandlers");
			foreach (var node in nodes) {
				IShoutHandler handler = (IShoutHandler) ((TypeExtensionNode)node).CreateInstance();
				m_ShoutHandlers.Add(handler);
			}
		}
		
		public void Shout (string message)
		{
			var accountService = ServiceManager.Get<AccountService>();
			foreach (var account in accountService.Accounts) {
				account.GetFeature<Microblogging>().Post(message);
			}

			foreach (var handler in m_ShoutHandlers) {
				handler.Shout(message);
			}
		}

		public string ServiceName {
			get {
				return "ShoutService";
			}
		}
	}
		
	public interface IShoutHandler
	{
		void Shout (string message);
	}

}
