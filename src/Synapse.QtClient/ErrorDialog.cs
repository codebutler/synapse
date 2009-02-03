//
// ErrorDialog.cs
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
using GLib;

namespace Synapse.QtClient
{
	public partial class ErrorDialog : Gtk.Dialog
	{
		public ErrorDialog(string errorTitle, Exception error)
		{
			this.Build();
			
			titleLabel.Markup = String.Format("<b>{0}</b>", Markup.EscapeText(errorTitle));
			if (error != null) {
				messageLabel.Text = error.Message;
				detailTextView.Buffer.Text = error.ToString();
			} else {
				messageLabel.Text = String.Empty;
				expander.Hide();
			}
		}
		
		protected virtual void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			this.Destroy();
		}

		protected virtual void expander_Activated (object sender, System.EventArgs e)
		{
			base.Resizable = expander.Expanded;
		}		
	}
}
