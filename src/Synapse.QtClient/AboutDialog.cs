using System;
using Qyoto;

namespace Synapse.QtClient.Windows
{
	public partial class AboutDialog : QDialog
	{
		public AboutDialog (QWidget parentWindow) : base (parentWindow)
		{
			SetupUi();
			
			Gui.CenterWidgetOnScreen(this);
		}
	}
}