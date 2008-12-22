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
using Synapse.UI;
using Synapse.ServiceStack;
using Qyoto;

namespace Synapse.QtClient.UI
{	
	public partial class NoAccountsWidget : QWidget
	{
		QGraphicsScene m_Scene;
		QTimeLine      m_TimeLine;

		public event DialogValidateEventHandler AddNewAccount;
		
		public NoAccountsWidget()
		{
			SetupUi();
			
			m_Scene = new QGraphicsScene(m_Scene);
			m_GraphicsView.SetScene(m_Scene);
			m_Scene.SetSceneRect(0, 0, 200, 200);
			
			QGraphicsSvgItem octy = new QGraphicsSvgItem("resource:/octy.svg");
			octy.SetPos(0, 10);
			octy.SetParent(m_Scene);
			m_Scene.AddItem(octy);

			// TODO: Add bubbles!

			m_TimeLine = new QTimeLine(2000, m_Scene);
			m_TimeLine.curveShape = QTimeLine.CurveShape.EaseOutCurve;
			QObject.Connect(m_TimeLine, Qt.SIGNAL("finished()"), this, Qt.SLOT("TimerFinished()"));

			QGraphicsItemAnimation animation = new QGraphicsItemAnimation(m_Scene);
			animation.SetItem(octy);
			animation.SetTimeLine(m_TimeLine);
			animation.SetPosAt(1, new QPointF(0, 0));
			
			m_TimeLine.Start();
		}

		public string Login {
			get {
				return m_LoginLineEdit.Text;
			}
		}

		public string Password {
			get {
				return m_PasswordLineEdit.Text;
			}
		}
		
		[Q_SLOT]
		protected void TimerFinished()
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
			if (AddNewAccount != null) {
				DialogValidationResult result = AddNewAccount();
				if (!result.IsValid) {
					// FIXME: Show errors
				}
			}
		}		
	}
}
