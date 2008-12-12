// MyClass.cs created with MonoDevelop
// User: eric at 3:49 PM 10/10/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Synapse.UI.Views;
using Synapse.ServiceStack;
using Synapse.Xmpp;

namespace Synapse.UI.Controllers
{
	public class LoginWindowController : AbstractController<ILoginWindowView>
	{
		public LoginWindowController()
		{
			base.View.LoginButtonClicked += OnLoginButtonClicked;
			base.View.QuitButtonClicked += OnQuitButtonClicked;
			base.View.Show();
		}

		private void OnLoginButtonClicked (object o, EventArgs args)
		{
			string user     = "test";
			string domain   = "localhost";
			string resource = "DRAGON_IS_AWESOME";
			string server   = String.Empty;
			string password = "foobar";
			
			Account account = new Account(user, domain, resource, server);
			account.Password = password;
			account.StateChanged += OnAccountStateChanged;
			
			ServiceManager.Get<AccountService>().AddAccount(account);

			account.Connect();
		}

		private void OnQuitButtonClicked (object o, EventArgs args)
		{
			Application.Shutdown();
		}

		private void OnAccountStateChanged(Account account)
		{
			if (account.State == AccountState.Disconnected) {
				//m_View.SetDisconnected(null);
			} else {
				//m_View.SetConnecting();

				if (account.State == AccountState.Connected) {
					account.StateChanged -= OnAccountStateChanged;
					base.View.Close();
				}
			}
		}                                  
	}
}
