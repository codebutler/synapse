//
// InsertLinkDialog.cs
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
using System.Linq;

using Qyoto;

namespace Synapse.QtClient.Windows
{
	public partial class InsertLinkDialog : QDialog
	{
		public InsertLinkDialog (QWidget parentWindow) : base (parentWindow)
		{
			SetupUi();
			buttonBox.StandardButtons = (uint)QDialogButtonBox.StandardButton.Ok | (uint)QDialogButtonBox.StandardButton.Cancel;
			Validate();
			
			Gui.CenterWidgetOnScreen(this);
		}
		
		public string Url {
			get {
				return urlLineEdit.Text;
			}
		}
		
		public string Text {
			get {
				return textLineEdit.Text;
			}
		}
		
		[Q_SLOT]
		void on_urlLineEdit_textChanged ()
		{
			Validate();
		}
		
		[Q_SLOT]
		void on_textLineEdit_textChanged ()
		{
			Validate();
		}
		
		void Validate ()
		{
			var button = buttonBox.Button(QDialogButtonBox.StandardButton.Ok);
			
			var validSchemes = new [] { "http", "https", "ftp", "xmpp" };
			
			Uri uri = null;
			button.Enabled = !String.IsNullOrEmpty(Url) && Uri.TryCreate(Url, UriKind.Absolute, out uri) && validSchemes.Contains(uri.Scheme.ToLower());
		}
	}
}
