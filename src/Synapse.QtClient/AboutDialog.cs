using System;

using Qyoto;

using Synapse.Core;

namespace Synapse.QtClient.Windows
{
	public partial class AboutDialog : QDialog
	{
		QGraphicsScene m_Scene;
		QTimeLine      m_TimeLine;
		
		public AboutDialog (QWidget parentWindow) : base (parentWindow)
		{
			SetupUi();
			
			m_Scene = new QGraphicsScene(m_Scene);
			graphicsView.SetScene(m_Scene);
			m_Scene.SetSceneRect(0, 0, 200, 200);
			
			textLabel.Pixmap = new QPixmap("resource:/text.png");
			
			var octy = new QGraphicsPixmapItem(new QPixmap("resource:/octy.png"));
			octy.SetPos(0, 10);
			m_Scene.AddItem(octy);
			
			m_TimeLine = new QTimeLine(2000, m_Scene);
			m_TimeLine.curveShape = QTimeLine.CurveShape.EaseOutCurve;
			QObject.Connect(m_TimeLine, Qt.SIGNAL("finished()"), TimerFinished);

			QGraphicsItemAnimation animation = new QGraphicsItemAnimation(m_Scene);
			animation.SetItem(octy);
			animation.SetTimeLine(m_TimeLine);
			animation.SetPosAt(1, new QPointF(0, 0));
			
			m_TimeLine.Start();
			
			Gui.CenterWidgetOnScreen(this);
		}
		
		void TimerFinished()
		{
			m_TimeLine.ToggleDirection();
			m_TimeLine.Start();
		}
		
		[Q_SLOT]
		void on_textBrowser_anchorClicked (QUrl link)
		{
			if (link.ToString() == "#message-eric") {
				var account = Gui.ShowAccountSelectMenu(null);
				if (account != null) {
					Gui.TabbedChatsWindow.StartChat(account, new jabber.JID("eric@extremeboredom.net"));
				}
			}
		}
		
		[Q_SLOT]
		void on_sendFeedbackButton_clicked ()
		{
			Util.Open("http://firerabbit.lighthouseapp.com/projects/23238-synapse/tickets/new");
		}
	}
}