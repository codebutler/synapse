//
// ChatsWindowController.cs
// 
// Copyright (C) 2008 Eric Butler
//
// Authors:
//   Eric Butler <eric@extremeboredom.net>
//
// This program is free software: you can redistribute it and/or modifys
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
using Synapse.ServiceStack;
using Synapse.UI.Services;
using Synapse.UI.Views;

namespace Synapse.UI.Controllers
{
 	public class TabbedChatsWindowController : AbstractController<ITabbedChatsWindowView>
	{
		public TabbedChatsWindowController()
		{
			Application.InvokeAndBlock(delegate {
				base.InitializeView();
			});
			
			var gui = ServiceManager.Get<GuiService>();
			gui.ChatWindowOpened += HandleChatWindowOpened;
			gui.ChatWindowClosed += HandleChatWindowClosed;
			gui.ChatWindowFocused += HandleChatWindowFocused;
		}

		void HandleChatWindowOpened (AbstractChatWindowController window, bool focus)
		{
			Application.Invoke(delegate {
				base.View.AddChatWindow(window, focus);
			});
		}

		void HandleChatWindowClosed (AbstractChatWindowController window)
		{
			Application.Invoke(delegate {
				base.View.RemoveChatWindow(window);
			});
		}

		void HandleChatWindowFocused (AbstractChatWindowController window)
		{
			Application.Invoke(delegate {
				base.View.FocusChatWindow(window);
			});
		}		
	}
}
