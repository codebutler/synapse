//
// ErrorDialog.cs
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
using Qyoto;

namespace Synapse.QtClient.Windows
{
	public partial class ErrorDialog : QDialog
	{
		public ErrorDialog (string errorTitle, string errorMessage, string errorDetail, QWidget parentWindow) : base (parentWindow)
		{
			SetupUi();
			
			iconLabel.Pixmap = Gui.LoadIcon("error").Pixmap(32);
			
			titleLabel.Text = "<b>" + Qt.Escape(errorTitle) + "</b>";
			messageLabel.Text = !String.IsNullOrEmpty(errorMessage) ? errorMessage : String.Empty;
			
			detailsTextEdit.Hide();
			
			if (!String.IsNullOrEmpty(errorDetail)) {
				detailsTextEdit.PlainText = errorDetail;	
			} else {
				showDetailsButtonContainer.Hide();
			}
			
			Gui.CenterWidgetOnScreen(this);
		}
	}
}