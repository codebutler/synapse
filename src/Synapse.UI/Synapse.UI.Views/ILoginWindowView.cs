// ILoginWindowView.cs created with MonoDevelop
// User: eric at 3:52 PM 10/10/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Synapse.UI;

namespace Synapse.UI.Views
{
	public interface ILoginWindowView : IView
	{
		event EventHandler LoginButtonClicked;
		event EventHandler QuitButtonClicked;
		event EventHandler HelpButtonClicked;

		//void SetConnecting();
		//void SetDisconnected(string error);
		
		string Login {
			get;
			set;
		}

		string Password {
			get;
			set;
		}
	}
}
