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
	public delegate void ShoutHandlerEventHandler (IShoutHandler handler);
	
	public class ShoutService : IRequiredService, IDelayedInitializeService
	{
		List<IShoutHandler> m_ShoutHandlers = new List<IShoutHandler>();
		
		public event ShoutHandlerEventHandler HandlerAdded;
		public event ShoutHandlerEventHandler HandlerRemoved;
		
		public void DelayedInitialize ()
		{
			var feed = ServiceManager.Get<ActivityFeedService>();
			feed.AddTemplate("shout", "Friend Events", "shouts", "shout");
			
			var nodes = AddinManager.GetExtensionNodes("/Synapse/Xmpp/ActivityFeed/ShoutHandlers");
			foreach (var node in nodes) {
				IShoutHandler handler = (IShoutHandler) ((TypeExtensionNode)node).CreateInstance();
				AddHandler(handler);
			}
		}

		public IEnumerable<IShoutHandler> Handlers {
			get {
				return m_ShoutHandlers.AsReadOnly();
			}
		}
		
		public void AddHandler (IShoutHandler handler)
		{
			m_ShoutHandlers.Add(handler);
			
			if (HandlerAdded != null)
				HandlerAdded(handler);
		}
		
		public void RemoveHandler (IShoutHandler handler)
		{
			m_ShoutHandlers.Remove(handler);
			
			if (HandlerRemoved != null)
				HandlerRemoved(handler);
		}
		
		public void Shout (string message, IShoutHandler[] handlers)
		{
			var accountService = ServiceManager.Get<AccountService>();
			
			if (accountService.ConnectedAccounts.Count == 0 && handlers.Length == 0) {
				throw new UserException("You are not connected.");
			}
			
			foreach (var account in accountService.ConnectedAccounts) {
				account.GetFeature<Microblogging>().Post(message);
			}

			foreach (var handler in handlers) {
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
		string Name {
			get;
		}
		void Shout (string message);
	}

}
