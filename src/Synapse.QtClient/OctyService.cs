
using System;
using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;
using Synapse.QtClient;
using Synapse.QtClient.Windows;
using Qyoto;

namespace Synapse.QtClient
{
	public class OctyService : IService, IDelayedInitializeService
	{
		static readonly string OCTYBOT_JID = "octy@extremeboredom.net";
		
		public void DelayedInitialize ()
		{
			var accountService = ServiceManager.Get<AccountService>();
			accountService.AccountReceivedRoster += HandleAccountReceivedRoster;
		}
		
		public string ServiceName {
			get {
				return "OctyService";
			}
		}
		
		public void HandleAccountReceivedRoster(Account account)
		{
			if (account.ConnectionState == AccountConnectionState.Connected && 
			    String.IsNullOrEmpty(account.GetProperty("AskedAboutOctyBot")) && 
			    account.Roster[OCTYBOT_JID] == null) 
			{
				account.SetProperty("AskedAboutOctyBot", "true");
				QApplication.Invoke(delegate {
					var octyDialog = new AddOctyDialog();
					if (octyDialog.Exec() == (int)QDialog.DialogCode.Accepted) {
						AddOcty(account);
					}
				});
			}
		}
		
		void AddOcty (Account account)
		{
			account.AddRosterItem(OCTYBOT_JID, null, new [] { "Friends" }, AddRosterItemComplete);
		}
		
		void AddRosterItemComplete (object sender, jabber.protocol.client.IQ response, object data)
		{
			if (response.Type != jabber.protocol.client.IQType.set) {
				QApplication.Invoke(delegate {
					QMessageBox.Critical(Gui.MainWindow, "Failed to add octy", "Server returned an error.");
				});
			}
		}
	}
}
