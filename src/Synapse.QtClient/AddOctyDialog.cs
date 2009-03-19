//
// AddOctyDialog.cs
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
using Synapse.Xmpp;

namespace Synapse.QtClient.Windows
{
	public partial class AddOctyDialog : QDialog
	{
		public AddOctyDialog ()
		{
			SetupUi();
			
			QObject.Connect(webView.Page().MainFrame(), Qt.SIGNAL("javaScriptWindowObjectCleared()"), HandleJavaScriptWindowObjectCleared);
			webView.Page().linkDelegationPolicy = QWebPage.LinkDelegationPolicy.DelegateAllLinks;
			webView.Load("resource:/addocty.html");
			
			Gui.CenterWidgetOnScreen(this);
		}
		
		void HandleJavaScriptWindowObjectCleared ()
		{
			webView.Page().MainFrame().AddToJavaScriptWindowObject("AddOctyDialog", this);
		}
		
		[Q_SLOT]
		public void continueClicked (bool addBot)
		{
			if (addBot) {
				Accept();
			} else {
				Reject();
			}
			Hide();
		}
	}
}