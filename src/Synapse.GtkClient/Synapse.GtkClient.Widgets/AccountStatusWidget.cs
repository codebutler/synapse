//
// AccountStatusWidget.cs
// 
// Copyright (C) 2010 Eric Butler
//
// Authors:
//   Eric Butler <eric@codebutler.com>
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
using Gtk;
using Synapse.Xmpp;

namespace Synapse.GtkClient.Widgets
{
	public class AccountStatusWidget : HBox 
	{
		Account m_Account;
		
		Image m_Image;
		Label m_NameLabel;
		Label m_StatusLabel;
		
		public AccountStatusWidget (Account account)
		{		
			base.Spacing = 6;
			
			m_Image = new Image(null, "default-avatar.png");
			base.PackStart(m_Image, false, false, 0);
			
			var box = new VBox();
			box.Spacing = 3;
			
			m_NameLabel = new Label();
			m_NameLabel.Xalign = 0;
			box.PackStart(m_NameLabel, false, false, 0);
			
			m_StatusLabel = new Label();
			m_StatusLabel.Xalign = 0;
			box.PackStart(m_StatusLabel, false, false, 0);
			
			base.PackStart(box);
			
			m_Account = account;
			m_Account.ConnectionStateChanged += HandleConnectionStateChanged;
			m_Account.StatusChanged += HandleConnectionStateChanged;
			m_Account.MyVCardUpdated += HandleMyVCardUpdated;
			m_Account.AvatarManager.AvatarUpdated += HandleAvatarUpdated;
			HandleConnectionStateChanged(account);
			HandleAvatarUpdated(account.Jid.Bare, null);
			HandleMyVCardUpdated(null, EventArgs.Empty);
			HandleAvatarUpdated(account.Jid.Bare, account.GetProperty("AvatarHash"));
			
			this.ShowAll();
		}
		
		public Account Account {
			get { return m_Account; }
		}
		
		void HandleConnectionStateChanged (Account account)
		{
			Gtk.Application.Invoke(delegate {
				string text = null;
				string statusText = null;
				if (account.Status != null) {
					text = account.Status.AvailabilityDisplayName;
					if (!String.IsNullOrEmpty(account.Status.StatusText)) {
						statusText = account.Status.StatusText;
					}
				} else {
					text = account.ConnectionState.ToString();
				}
								
				if (statusText == null)
					m_StatusLabel.Markup = String.Format("<span foreground=\"white\" underline=\"single\">{0}</span>", text);
				else
					m_StatusLabel.Markup = String.Format("<span foreground=\"white\"><span underline=\"single\">{0}</span> - {1}</span>", text, statusText);
			});
		}

		void HandleAvatarUpdated (string jid, string avatarHash)
		{
			if (jid == m_Account.Jid.Bare) {				
				Gtk.Application.Invoke(delegate {
					Gdk.Pixbuf pixbuf = (Gdk.Pixbuf) AvatarManager.GetAvatar(avatarHash);
					m_Image.Pixbuf = pixbuf;
				});
			}
		}

		void HandleMyVCardUpdated (object sender, EventArgs e)
		{
			Gtk.Application.Invoke(delegate {
				string text = null;
				if (m_Account.VCard != null && !String.IsNullOrEmpty(m_Account.VCard.Nickname))
					text = m_Account.VCard.Nickname;
				else
					text = m_Account.Jid.Bare;				
				m_NameLabel.Markup = String.Format("<span foreground=\"white\" weight=\"bold\">{0}</span>", text);
			});
		}
	}
}

