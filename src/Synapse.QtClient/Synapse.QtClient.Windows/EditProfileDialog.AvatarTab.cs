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
using System.Drawing;
using System.Drawing.Imaging;

using Qyoto;

using Synapse.Xmpp;
using Synapse.Xmpp.Services;
using Synapse.ServiceStack;
using Synapse.UI;
using Synapse.UI.Services;

using Mono.Addins;

using jabber;
using jabber.protocol.client;
using jabber.protocol.iq;

namespace Synapse.QtClient.Windows
{
	public partial class EditProfileDialog : QDialog
	{
		void SetupAvatarTab ()
		{
			m_Account.AvatarManager.AvatarUpdated += HandleAvatarUpdated;
	
			avatarLabel.Pixmap = (QPixmap)AvatarManager.GetAvatar(m_Account.Jid);
			
			if (m_Account.VCard != null && (!String.IsNullOrEmpty(m_Account.VCard.Nickname) || !String.IsNullOrEmpty(m_Account.VCard.FullName)))
				avatarSearchLineEdit.Text = !String.IsNullOrEmpty(m_Account.VCard.Nickname) ? m_Account.VCard.Nickname : m_Account.VCard.FullName;
			else
				avatarSearchLineEdit.Text = m_Account.Jid.User;
			
			foreach (var node in AddinManager.GetExtensionNodes("/Synapse/UI/AvatarProviders")) {
				IAvatarProvider provider = (IAvatarProvider)((TypeExtensionNode)node).CreateInstance();
				var tab = new AvatarProviderTab(provider, this);
				avatarTabWidget.AddTab(tab, provider.Name);
				tab.Show();
			}
	
			if (avatarTabWidget.Count == 0) {
				// FIXME: Show a "no providers" message.
			}
		}
	
		void HandleAvatarUpdated(string jid, string avatarHash)
		{
			if (jid == m_Account.Jid.Bare) {
				QApplication.Invoke(delegate {
					avatarLabel.Pixmap = (QPixmap)AvatarManager.GetAvatar(avatarHash);
				});
			}
		}
	
		[Q_SLOT]
		void setAvatarUrl(string url)
		{
			avatarLabel.Pixmap = new QPixmap("resource:/loading.gif");
	
			var request = (HttpWebRequest)HttpWebRequest.Create(url);
			request.BeginGetResponse(delegate (IAsyncResult result) {
				HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
	
				Image image = Image.FromStream(response.GetResponseStream());
				byte[] buffer = null;
				using (MemoryStream stream = new MemoryStream()) {
					image.Save(stream, image.RawFormat);
					buffer = stream.GetBuffer();
				}
				
				SetAvatar(buffer, image.RawFormat);
			}, null);
		}
		
		[Q_SLOT]
		void on_avatarSearchButton_clicked ()
		{
			((AvatarProviderTab)avatarTabWidget.CurrentWidget()).Update(avatarSearchLineEdit.Text);
		}
	
		[Q_SLOT]
		void on_avatarBrowseButton_clicked ()
		{
			var dialog = new QFileDialog(this.TopLevelWidget(), "Select Avatar");
			dialog.fileMode = QFileDialog.FileMode.ExistingFile;
			if (dialog.Exec() == (int)DialogCode.Accepted && dialog.SelectedFiles().Count > 0) {
				string fileName = dialog.SelectedFiles()[0];
	
				try {
					byte[] buffer = null;
					Image image = Image.FromFile(fileName);
					using (MemoryStream stream = new MemoryStream()) {
						image.Save(stream, image.RawFormat);
						buffer = stream.GetBuffer();
					}			
					SetAvatar(buffer, image.RawFormat);
				} catch (Exception ex) {
					Application.Client.ShowErrorWindow("Failed to set avatar. The file you selected may be an unsupported image type or may be damaged.", ex);
				}
			}
		}
		
		[Q_SLOT]
		void on_clearAvatarButton_clicked ()
		{
			SetAvatar(null, null);
		}
	
		void SetAvatar (byte[] buffer, ImageFormat format)
		{			
			// FIXME: Handle this more gracefully, or prevent it from happening (see Account#HandleOnAuthenticate).
			if (m_Account.VCard == null) {
				throw new Exception("No VCard!");
			}
			
			if (buffer == null) {		
				if (m_Account.VCard.Photo != null) {
					m_Account.VCard.Photo.BinVal    = null;
					m_Account.VCard.Photo.ImageType = null;
				}
			} else {
				if (format == null)
					throw new ArgumentNullException("format");
				
				// FIXME:
				// The image SHOULD use less than eight kilobytes (8k) of data; this restriction is to be enforced by the publishing client.
				// The image height and width SHOULD be between thirty-two (32) and ninety-six (96) pixels; the recommended size is sixty-four (64) pixels high and sixty-four (64) pixels wide.
				// The image SHOULD be square.
	
				if (m_Account.VCard.Photo == null) {
					m_Account.VCard.Photo = new VCard.VPhoto(m_Account.Client.Document);
				}
				
				m_Account.VCard.Photo.ImageType = format;
				m_Account.VCard.Photo.BinVal = buffer;
			}
				
			m_Account.SaveVCard();
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
	
				QApplication.Invoke(delegate {
					base.SetHtml(builder.ToString());
				});
			}
		}
	}
}
