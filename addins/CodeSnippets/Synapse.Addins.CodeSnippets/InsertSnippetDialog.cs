//
// InsertSnippetDialog.cs
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
using System.Text;
using System.Collections.Generic;
using System.Xml;
using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.UI.Chat;
using Synapse.QtClient;
using Synapse.QtClient.Windows;
using Qyoto;
using Mono.Addins;

namespace Synapse.Addins.CodeSnippets
{
	public partial class InsertSnippetDialog : QDialog
	{		
		public InsertSnippetDialog (QWidget parent) : base (parent)
		{
			SetupUi();
	
			buttonBox.StandardButtons = (uint) QDialogButtonBox.StandardButton.Ok | (uint) QDialogButtonBox.StandardButton.Cancel;
			
			ChatWindow chatWindow = (ChatWindow)parent;
			toLabel.Text = (chatWindow.Handler is ChatHandler) ? ((ChatHandler)chatWindow.Handler).Jid.ToString() : ((MucHandler)chatWindow.Handler).Room.JID.ToString();

			var service = ServiceManager.Get<CodeSnippetsService>();
			foreach (var highlighter in service.Highlighters) {
				typeComboBox.AddItem(highlighter.FullName, highlighter.Name);
			}

			QObject.Connect(this, Qt.SIGNAL("accepted()"), this, Qt.SLOT("dialog_accepted()"));
			
			Gui.CenterWidgetOnScreen(this);
		}

		[Q_SLOT]
		void on_typeComboBox_currentIndexChanged (int index)
		{
			UpdatePreview();
		}
		
		[Q_SLOT]
		void on_tabWidget_currentChanged (int index)
		{
			if (index == 1) {
				UpdatePreview();
			}
		}

		[Q_SLOT]
		void dialog_accepted ()
		{	
			var handler = ((ChatWindow)this.ParentWidget()).Handler;
			
			var mimeType = typeComboBox.ItemData(typeComboBox.CurrentIndex).ToString();
			
			var service = ServiceManager.Get<CodeSnippetsService>();
			service.SendMessage(handler, mimeType, textEdit.PlainText);
		}

		void UpdatePreview ()
		{
			string mimeType = typeComboBox.ItemData(typeComboBox.CurrentIndex);			
			var service = ServiceManager.Get<CodeSnippetsService>();
			webView.SetHtml(service.GeneratePreview(mimeType, textEdit.PlainText));
		}
	}
}