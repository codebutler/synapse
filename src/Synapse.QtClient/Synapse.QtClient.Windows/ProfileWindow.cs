//
// UserProfileWindow.cs
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
using Qyoto;
using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Xmpp;
using jabber;
using jabber.protocol.iq;
using jabber.protocol.client;
using TemplateEngine;

namespace Synapse.QtClient.Windows
{
	// FIXME: Rename to UserProfileWindow!
	public partial class ProfileWindow
	{
		public ProfileWindow (Account account, JID jid) : base ()
		{
			SetupUi();
	
			webView.SetHtml("<p>Loading...</p>");
	
			account.RequestVCard(jid, delegate (object sender, IQ iq, object data) {
				if (iq.Type == IQType.result)
					Populate((VCard)iq.FirstChild);
				else
					Populate(null);
			}, null);
			
			Gui.CenterWidgetOnScreen(this);
		}
	
		void Populate (VCard vcard)
		{
			string template = null;
			
			if (vcard == null) {
				template = "Unable to view profile.";
			} else {
				template = Util.ReadResource("profile.html");
	
				// FIXME: This is just a quick hack. Really need to do all the replacement at once.
				template = template.Replace("@@NAME@@", vcard.FullName);
				template = template.Replace("@@JID@@", vcard.JabberId);
				template = template.Replace("@@NICKNAME@@", vcard.Nickname);
	
				try {
					template = template.Replace("@@BIRTHDAY@@", vcard.Birthday.ToString());
				} catch {
					template = template.Replace("@@BIRTHDAY@@", String.Empty);
				}
	
				var email = vcard.GetEmail(EmailType.home);
				if (email != null)
					template = template.Replace("@@EMAIL@@", email.UserId);
				else
					template = template.Replace("@@EMAIL@@", String.Empty);
	
				if (vcard.Url != null)
					template = template.Replace("@@WEBSITE@@", vcard.Url.ToString());
				else
					template = template.Replace("@@WEBSITE@@", String.Empty);
				
				template = template.Replace("@@ADDRESS@@", FormatAddress(vcard.GetAddress(AddressLocation.home)));
	
				var phone = vcard.GetTelephone(TelephoneType.voice, TelephoneLocation.home);
				if (phone != null)
					template = template.Replace("@@PHONE@@", phone.Number);
				else
					template = template.Replace("@@PHONE@@", String.Empty);
				
				template = template.Replace("@@ABOUT@@", vcard.Description);
	
				var org = vcard.Organization;
				if (org != null) {
					template = template.Replace("@@COMPANY@@", org.OrgName);
					template = template.Replace("@@DEPARTMENT@@", vcard.Organization.Unit);
				} else {
					template = template.Replace("@@COMPANY@@", String.Empty);
					template = template.Replace("@@DEPARTMENT@@", String.Empty);
				}
							
				template = template.Replace("@@POSITION@@", vcard.Title);
				template = template.Replace("@@ROLE@@", vcard.Role);
	
				email = vcard.GetEmail(EmailType.work);
				if (email != null)
					template = template.Replace("@@WORK_EMAIL@@", email.UserId);
				else
					template = template.Replace("@@WORK_EMAIL@@", String.Empty);
				
				template = template.Replace("@@WORK_ADDRESS@@", FormatAddress(vcard.GetAddress(AddressLocation.work)));
	
				phone = vcard.GetTelephone(TelephoneType.voice, TelephoneLocation.work);
				if (phone != null)
					template = template.Replace("@@WORK_PHONE@@", phone.Number);
				else
					template = template.Replace("@@WORK_PHONE@@", String.Empty);
			}
			QApplication.Invoke(delegate {
				webView.SetHtml(template);
			});
		}
		
		string FormatAddress (VCard.VAddress address)
		{
			if (address == null)
				return string.Empty;
			
			return String.Format("{0}<br/>{1}, {2} {3}<br/>{4} {5}",
			                     address.Street,
			                     address.Locality,
			                     address.Region,
			                     address.PostalCode,
			                     address.Country,
			                     address.Extra);	
		}
	}
}