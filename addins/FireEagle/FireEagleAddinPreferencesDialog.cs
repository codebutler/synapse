//
// FireEagleAddinPreferencesDialog.cs
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
using System.Xml;

using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Services;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;
using Synapse.QtClient;

using Qyoto;

namespace Synapse.Addins.FireEagle
{
	public partial class FireEagleAddinPreferencesDialog : QDialog
	{
		public FireEagleAddinPreferencesDialog (QWidget parentWindow) : base (parentWindow)
		{
			SetupUi();
			
			logoLabel.Pixmap = new QPixmap("resource:/fireeagle/logo.png");
			
			waitLabel1.Hide();
			waitLabel2.Hide();
			
			var service = ServiceManager.Get<FireEagleService>();
			
			if (service.IsReady) {
				stackedWidget.CurrentIndex = 2;
				UpdateLocation();
			} else {
				stackedWidget.CurrentIndex = 0;
			}
			
			service.AuthorizationNeeded += HandleAuthorizationNeeded;
			service.ReceivedAccessToken += HandleReceivedAccessToken;
			service.AccessTokenCleared  += HandleAccessTokenCleared;
			
			Gui.CenterWidgetOnScreen(this);
		}
		
		[Q_SLOT]
		void on_setupButton_clicked ()
		{
			waitLabel1.Show();
			
			var service = ServiceManager.Get<FireEagleService>();
			
			service.GetRequestToken();
		}
		
		void HandleAuthorizationNeeded (string authorizationUrl)
		{			
			urlLineEdit.Text = authorizationUrl;
			
			stackedWidget.CurrentIndex = 1;
			
			Util.Open(authorizationUrl);
		}
		
		[Q_SLOT]
		void on_continueButton_clicked ()
		{
			waitLabel2.Show();
			
			var service = ServiceManager.Get<FireEagleService>();
			service.RequestAccessToken();
		}
		
		void HandleReceivedAccessToken (object o, EventArgs args)
		{			
			var service = ServiceManager.Get<FireEagleService>();
			service.Subscribe();
			
			stackedWidget.CurrentIndex = 2;
					
			UpdateLocation();
		}
		
		void UpdateLocation ()
		{
			var service = ServiceManager.Get<FireEagleService>();
			string xml = service.GetLocation();

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			
			XmlElement loc = (XmlElement)doc.SelectSingleNode("/rsp/user/location-hierarchy/location[@best-guess='true']");
			locationLabel.Text = loc["name"].InnerText;
		}
		
		[Q_SLOT]
		void on_reauthorizeButton_clicked ()
		{
			var service = ServiceManager.Get<FireEagleService>();
			service.ClearAccessToken();
		}
		
		void HandleAccessTokenCleared (object o, EventArgs args)
		{
			stackedWidget.CurrentIndex = 0;
		}
	}
}