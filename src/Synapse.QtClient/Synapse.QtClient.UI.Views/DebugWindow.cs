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
using Synapse.UI.Views;
using Synapse.UI.Controllers;
using Qyoto;

namespace Synapse.QtClient.UI.Views
{
	public partial class DebugWindow : QWidget, IDebugWindowView
	{
		OperationsModel m_Model;
		Dictionary<Account, QWidget> m_AccountXmlWidgets = new Dictionary<Account, QWidget>();
		
		public DebugWindow (DebugWindowController controller)
		{
			SetupUi();
			m_Model = new OperationsModel();
			m_OperationsTableView.selectionBehavior = QAbstractItemView.SelectionBehavior.SelectRows;
			m_OperationsTableView.VerticalHeader().Hide();
			m_OperationsTableView.SetModel(m_Model);
	
			((QBoxLayout)m_XmlToolBox.Layout()).Spacing = 0;
		}
	
		public void AddAccount(Account account)
		{
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
				Application.Invoke(delegate {
					if (enableConsoleCheckBox.Checked)
						textEdit.Append("<b>" + Qt.Escape(txt) + "</b><br/>");
				});
			};
	
			account.Client.OnReadText += delegate(object sender, string txt) {
				Application.Invoke(delegate {
					if (enableConsoleCheckBox.Checked)
						textEdit.Append(Qt.Escape(txt) + "<br/>");
				});
			};
		}
	
		public void RemoveAccount(Account account)
		{
			QWidget widget = m_AccountXmlWidgets[account];
			m_AccountXmlWidgets.Remove(account);
			widget.Dispose();
		}
	
		[Q_SLOT]
		void on_clearConsoleButton_clicked ()
		{
			
		}
	
		private class OperationsModel : QAbstractTableModel
		{
			OperationService m_Service;
			
			string[] m_ColumnNames = new string[] { "Name", "Status", "Started At" };
	
			public OperationsModel ()
			{
				m_Service = ServiceManager.Get<OperationService>();
				m_Service.OperationAdded   += HandleAccountAddedUpdated;
				m_Service.OperationUpdated += HandleAccountAddedUpdated;
			}
			
			public override QVariant Data (QModelIndex index, int role)
			{
				int row = index.Row();
				int col = index.Column();
			
				IOperation operation = m_Service.Operations[row];
				
				if (role == (int)Qt.ItemDataRole.DisplayRole) {
					if (col == 0)
						return String.Format("{0}: {1}", operation.Name, operation.Description);
					else if (col == 1)
						return operation.Status.ToString();
					else if (col == 2)
						return operation.StartedAt.ToShortTimeString();
				} else if (role == (int)Qt.ItemDataRole.ToolTipRole) {
					return operation.StackTrace;
				}
				return new QVariant();
			}
	
			public override int ColumnCount (Qyoto.QModelIndex parent)
			{
				return m_ColumnNames.Length;
			}
	
			public override int RowCount (Qyoto.QModelIndex parent)
			{
				return m_Service.Operations.Count;
			}
	
			public override QModelIndex Parent (Qyoto.QModelIndex child)
			{
				return null;
			}
	
			public override QVariant HeaderData (int section, Qt.Orientation orientation, int role)
			{
				if (role == (int)Qt.ItemDataRole.DisplayRole)
					return m_ColumnNames[section];
				else
					return new QVariant();
			}
	
			private void HandleAccountAddedUpdated (IOperation operation)
			{
				Application.Invoke(delegate {
					Emit.LayoutChanged();
				});
			}			
		}
	}
}