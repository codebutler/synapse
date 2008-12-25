//
// EditGroupsController.cs
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
//

using System;
using System.Linq;
using Synapse.UI.Views;
using Synapse.Xmpp;
using Synapse.ServiceStack;
using jabber.protocol.iq;

namespace Synapse.UI.Controllers
{	
	public class EditGroupsWindowController : AbstractController<IEditGroupsWindowView>
	{
		string[] m_GroupNames;
		Item     m_RosterItem;
		
		public EditGroupsWindowController(Account account, Item rosterItem)
		{
			m_RosterItem = rosterItem;
			
			m_GroupNames = account.Roster.Select(jid => account.Roster[jid])
				.SelectMany(item => item.GetGroups())
				.Select(group => group.GroupName)
				.Distinct()
				.OrderBy(name => name)
				.ToArray();
						
			Application.Invoke(delegate {
				InitializeView();
				View.Show();
			});
		}

		public Item RosterItem {
			get {
				return m_RosterItem;
			}
		}
		
		public string[] GroupNames {
			get {
				return m_GroupNames;
			}
		}
	}
}
