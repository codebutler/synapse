using System;

using Synapse.Core;
using Synapse.Xmpp;
using Synapse.ServiceStack;
using Synapse.QtClient;

using Qyoto;

namespace Synapse.Addins.Twitter
{
	public partial class TwitterConfigurationDialog : QDialog
	{
		Account m_Account;
		
		public TwitterConfigurationDialog (Account account, QWidget parentWindow) : base (parentWindow)
		{
			SetupUi();
			
			m_Account = account;
			
			buttonBox.StandardButtons = (uint)QDialogButtonBox.StandardButton.Ok |
			                            (uint)QDialogButtonBox.StandardButton.Cancel;
			
			logoLabel.Pixmap = new QPixmap("resource:/twitter/twitm-48.png");
			
			usernameLineEdit.Text = !String.IsNullOrEmpty(account.GetProperty("Twitter.Username")) ? account.GetProperty("Twitter.Username") : account.GetFeature<UserWebIdentities>().GetIdentity("twitter");
			passwordLineEdit.Text = !String.IsNullOrEmpty(account.GetProperty("Twitter.Password")) ? account.GetProperty("Twitter.Password") : String.Empty;
			
			Gui.CenterWidgetOnScreen(this);
		}
		
		public override void Accept ()
		{
			string oldUsername = m_Account.GetProperty("Twitter.Username");
			
			m_Account.SetProperty("Twitter.Username", usernameLineEdit.Text);
			m_Account.SetProperty("Twitter.Password", passwordLineEdit.Text);
			m_Account.GetFeature<UserWebIdentities>().SetIdentity("twitter", usernameLineEdit.Text);
			
			var service = ServiceManager.Get<TwitterService>();
			service.AccountConfigUpdated(m_Account, oldUsername, usernameLineEdit.Text, passwordLineEdit.Text);
			
			base.Accept();
		}	
	}
}