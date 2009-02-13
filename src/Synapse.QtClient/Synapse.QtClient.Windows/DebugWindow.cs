//
// DebugWindow.cs
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
using System.Collections.Generic;
using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;
using Qyoto;

namespace Synapse.QtClient.Windows
{
	public partial class DebugWindow : QWidget
	{
		Dictionary<Account, QWidget> m_AccountXmlWidgets = new Dictionary<Account, QWidget>();
		
		public DebugWindow ()
		{
			SetupUi();
	
			((QBoxLayout)m_XmlToolBox.Layout()).Spacing = 0;

			AccountService accountService = ServiceManager.Get<AccountService>();
			
			accountService.AccountAdded   += HandleAccountAdded;
			accountService.AccountRemoved += HandleAccountRemoved;
			
			foreach (Account account in accountService.Accounts) {
				HandleAccountAdded(account);
			}
		}
	
		public void HandleAccountAdded(Account account)
		{
			QApplication.Invoke(delegate {
				QTextEdit textEdit = new QTextEdit(this);
				textEdit.FrameShape = QFrame.Shape.NoFrame;
				textEdit.ReadOnly = true;
				
				QWidget widget = new QWidget();
				
				QVBoxLayout layout = new QVBoxLayout(widget);
				layout.Margin = 0;
				layout.AddWidget(textEdit);
				
				m_XmlToolBox.AddItem(widget, account.Jid);
		
				m_AccountXmlWidgets.Add(account, widget);
		
				account.Client.OnWriteText += delegate(object sender, string txt) {
					QApplication.Invoke(delegate {
						if (enableConsoleCheckBox.Checked)
							textEdit.Append("<b>" + Qt.Escape(txt) + "</b><br/>");
					});
				};
		
				account.Client.OnReadText += delegate(object sender, string txt) {
					QApplication.Invoke(delegate {
						if (enableConsoleCheckBox.Checked)
							textEdit.Append(Qt.Escape(txt) + "<br/>");
					});
				};
			});
		}
	
		public void HandleAccountRemoved(Account account)
		{
			QApplication.Invoke(delegate {
				QWidget widget = m_AccountXmlWidgets[account];
				m_AccountXmlWidgets.Remove(account);
				widget.Dispose();
			});
		}
	
		[Q_SLOT]
		void on_clearConsoleButton_clicked ()
		{
			
		}
	}
}