using System;

using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.QtClient;

using Qyoto;

namespace Synapse.Addins.Twitter
{
	public partial class TwitterAddinPreferencesDialog : QDialog
	{
		public TwitterAddinPreferencesDialog (QWidget parentWindow) : base (parentWindow)
		{
			SetupUi();
			
			buttonBox.StandardButtons = (uint)QDialogButtonBox.StandardButton.Ok |
			                            (uint)QDialogButtonBox.StandardButton.Cancel;
			
			logoLabel.Pixmap = new QPixmap("resource:/twitter/twitm-48.png");
			
			var twitterService = ServiceManager.Get<TwitterService>();
			usernameLineEdit.Text = twitterService.Username;
			passwordLineEdit.Text = twitterService.Password;
			
			Gui.CenterWidgetOnScreen(this);
		}
		
		public override void Accept ()
		{
			base.Accept ();
			
			var twitterService = ServiceManager.Get<TwitterService>();
			twitterService.Username = usernameLineEdit.Text;
			twitterService.Password = passwordLineEdit.Text;
		}
	
	}
}