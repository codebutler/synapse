using System;
using Qyoto;
using Synapse.Xmpp;

namespace Synapse.QtClient.Windows
{
	public partial class AddOctyDialog : QDialog
	{
		public AddOctyDialog ()
		{
			SetupUi();
			
			QObject.Connect(webView.Page().MainFrame(), Qt.SIGNAL("javaScriptWindowObjectCleared()"), HandleJavaScriptWindowObjectCleared);
			webView.Page().linkDelegationPolicy = QWebPage.LinkDelegationPolicy.DelegateAllLinks;
			webView.Load("resource:/addocty.html");
			
			Gui.CenterWidgetOnScreen(this);
		}
		
		void HandleJavaScriptWindowObjectCleared ()
		{
			webView.Page().MainFrame().AddToJavaScriptWindowObject("AddOctyDialog", this);
		}
		
		[Q_SLOT]
		public void continueClicked (bool addBot)
		{
			if (addBot) {
				Accept();
			} else {
				Reject();
			}
			Hide();
		}
	}
}