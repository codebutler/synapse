//
// TwitterAddinPreferencesDialog.cs
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
using Synapse.ServiceStack;

namespace Synapse.Addins.TwitterAddin
{	
	public partial class TwitterAddinPreferencesDialog : Gtk.Dialog
	{
		public TwitterAddinPreferencesDialog()
		{
			this.Build();

			// FIXME: Stetic bug
			table1.RowSpacing = 6;

			var twitterService = ServiceManager.Get<TwitterService>();
			usernameEntry.Text = twitterService.Username;
			passwordEntry.Text = twitterService.Password;
		}

		protected override void OnResponse (Gtk.ResponseType response_id)
		{
			base.OnResponse (response_id);

			if (response_id == Gtk.ResponseType.Ok) {
				var twitterService = ServiceManager.Get<TwitterService>();
				twitterService.Username = usernameEntry.Text;
				twitterService.Password = passwordEntry.Text;
			}
			
			base.Destroy();
		}
	}
}
