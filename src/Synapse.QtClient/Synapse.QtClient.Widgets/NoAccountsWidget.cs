//
// NoAccountsWidget.cs
//
// Copyright (C) 2008 Eric Butler
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

using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;
using Synapse.UI;

using Qyoto;

using jabber;

namespace Synapse.QtClient.Widgets
{	
	public partial class NoAccountsWidget : QWidget
	{
		QGraphicsScene m_Scene;
		QTimeLine      m_TimeLine;
		
		public NoAccountsWidget(QWidget parent) : base (parent)
		{
			SetupUi();
			
			m_Scene = new QGraphicsScene(m_Scene);
			m_GraphicsView.SetScene(m_Scene);
			m_Scene.SetSceneRect(0, 0, 200, 200);
			
			var octy = new QGraphicsPixmapItem(new QPixmap("resource:/octy.png"));
			octy.SetPos(0, 10);
			m_Scene.AddItem(octy);

			// TODO: Add bubbles!

			m_TimeLine = new QTimeLine(2000, m_Scene);
			m_TimeLine.curveShape = QTimeLine.CurveShape.EaseOutCurve;
			QObject.Connect(m_TimeLine, Qt.SIGNAL("finished()"), HandleTimerFinished);

			QGraphicsItemAnimation animation = new QGraphicsItemAnimation(m_Scene);
			animation.SetItem(octy);
			animation.SetTimeLine(m_TimeLine);
			animation.SetPosAt(1, new QPointF(0, 0));
			
			m_TimeLine.Start();
		}
		
		void HandleTimerFinished()
		{
			m_TimeLine.ToggleDirection();
			m_TimeLine.Start();
		}

		[Q_SLOT]
		private void on_addAccountButton_clicked()
		{
			stackedWidget.CurrentIndex += 1;
		}
		
		[Q_SLOT]
		private void on_quitButton1_clicked()
		{
			Application.Shutdown();
		}
		
		[Q_SLOT]
		private void on_quitButton2_clicked()
		{
			Application.Shutdown();
		}

		[Q_SLOT]
		private void on_saveAccountButton_clicked()
		{
			JID jid = null;
			
			if (String.IsNullOrEmpty(m_LoginLineEdit.Text))
				QMessageBox.Critical(this.TopLevelWidget(), "Synapse", "Login may not be empty.");
			else if (!JID.TryParse(m_LoginLineEdit.Text, out jid))
				QMessageBox.Critical(this.TopLevelWidget(), "Synapse", "Login should look like 'user@server'.");
			else if (String.IsNullOrEmpty(new JID(m_LoginLineEdit.Text).User))
				QMessageBox.Critical(this.TopLevelWidget(), "Synapse", "Login should look like 'user@server'.");
			else if (String.IsNullOrEmpty(m_PasswordLineEdit.Text))
				QMessageBox.Critical(this.TopLevelWidget(), "Synapse", "Password may not be empty");
			else {
				var accountInfo = new AccountInfo(jid.User, jid.Server, m_PasswordLineEdit.Text, "Synapse");
				AccountService service = ServiceManager.Get<AccountService>();
				service.AddAccount(accountInfo);
			}
		}		
	}
}
