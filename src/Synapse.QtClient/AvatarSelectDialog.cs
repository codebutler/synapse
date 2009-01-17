//
// AvatarSelectDialog.cs
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
using System.Text;
using System.Net;
using System.IO;
using Qyoto;
using Synapse.Xmpp;
using Synapse.ServiceStack;
using Synapse.UI;
using Mono.Addins;

public partial class AvatarSelectDialog : QDialog
{
	Account m_Account;
	
	public AvatarSelectDialog (Account account)
	{
		if (account == null)
			throw new ArgumentNullException("account");
		
		SetupUi();

		m_Account = account;

		avatarLabel.Pixmap = (QPixmap)AvatarManager.GetAvatar(account.Jid);
		
		if (account.VCard != null && (!String.IsNullOrEmpty(account.VCard.Nickname) || !String.IsNullOrEmpty(account.VCard.FullName)))
			lineEdit.Text = !String.IsNullOrEmpty(account.VCard.Nickname) ? account.VCard.Nickname : account.VCard.FullName;
		else
			lineEdit.Text = account.Jid.User;
		
		foreach (var node in AddinManager.GetExtensionNodes("/Synapse/UI/AvatarProviders")) {
			IAvatarProvider provider = (IAvatarProvider)((TypeExtensionNode)node).CreateInstance();
			var tab = new AvatarProviderTab(provider, this);
			tabWidget.AddTab(tab, provider.Name);
			tab.Show();
		}

		if (tabWidget.Children().Count > 0) {
			var firstTab = (AvatarProviderTab)tabWidget.Widget(0);
			firstTab.Update(lineEdit.Text);
		} else {
			// FIXME: Show a "no providers" message.
		}
	}

	[Q_SLOT]
	public void setAvatarUrl(string url)
	{
		avatarLabel.Pixmap = new QPixmap("resource:/loading.gif");

		var request = (HttpWebRequest)HttpWebRequest.Create(url);
		request.BeginGetResponse(delegate (IAsyncResult result) {
			HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
			byte[] buffer = new byte[response.ContentLength];
			using (Stream stream = response.GetResponseStream()) {
				int offset = 0;
				int remaining = buffer.Length;
				while (remaining > 0) {
					int read = stream.Read(buffer, offset, remaining);
					if (read <= 0)
						throw new EndOfStreamException(String.Format("End of stream reached with {0} bytes left to read", remaining));
					remaining -= read;
					offset += read;
				}
			}
			
			Application.Invoke(delegate {
				QPixmap pixmap = new QPixmap();
				Pointer<byte> p = new Pointer<byte>(buffer);
				pixmap.LoadFromData(p, (uint)buffer.Length);
				SetAvatar(pixmap);
			});
		}, null);
	}
	
	[Q_SLOT]
	void on_searchButton_clicked ()
	{
		((AvatarProviderTab)tabWidget.CurrentWidget()).Update(lineEdit.Text);
	}

	[Q_SLOT]
	void on_browseButton_clicked ()
	{
		var dialog = new QFileDialog(this.TopLevelWidget(), "Select Avatar");
		if (dialog.Exec() == (int)DialogCode.Accepted && dialog.SelectedFiles().Count > 0) {
			string fileName = dialog.SelectedFiles()[0];
			SetAvatar(new QPixmap(fileName));
		}
	}
	
	[Q_SLOT]
	void on_clearButton_clicked ()
	{
		SetAvatar(null);
	}

	void SetAvatar (QPixmap pixmap)
	{
		if (pixmap == null) {
			pixmap = new QPixmap("resource:/default-avatar.png");
		}

		// FIXME:
		// The image SHOULD use less than eight kilobytes (8k) of data; this restriction is to be enforced by the publishing client.
		// The image height and width SHOULD be between thirty-two (32) and ninety-six (96) pixels; the recommended size is sixty-four (64) pixels high and sixty-four (64) pixels wide.
		// The image SHOULD be square.
		
		avatarLabel.Pixmap = pixmap;

		// FIXME: Update server
	}
	
	class AvatarProviderTab : QWebView
	{
		IAvatarProvider m_Provider;
			
		public AvatarProviderTab (IAvatarProvider provider, QWidget parent) : base (parent)
		{
			m_Provider = provider;

			QObject.Connect(base.Page().MainFrame(), Qt.SIGNAL("javaScriptWindowObjectCleared()"), delegate {
				base.Page().MainFrame().AddToJavaScriptWindowObject("AvatarSelectDialog", parent.TopLevelWidget());
			});
			
			base.SetHtml(String.Empty);
		}

		public void Update (string text)
		{
			base.SetHtml("Searching...");
			
			m_Provider.BeginGetAvatars(text, this.ReceivedAvatars);
		}

		public IAvatarProvider Provider {
			get {
				return m_Provider;
			}
		}

		void ReceivedAvatars (object sender, AvatarInfo[] avatars)
		{
			StringBuilder builder = new StringBuilder();
			foreach (AvatarInfo info in avatars) {
				builder.AppendFormat("<div style=\"float: left; padding: 6px;\"> <a href=\"javascript:AvatarSelectDialog.setAvatarUrl('{0}')\" title=\"{1}\"><img style=\"width: 75px; height: 75px;\" alt=\"{1}\" src=\"{2}\"/></a></div>", info.Url, info.Title, info.ThumbnailUrl);
			}
			Console.WriteLine(builder.ToString());

			Application.Invoke(delegate {
				base.SetHtml(builder.ToString());
			});
		}
	}
}
