//
// PreferencesWindow.cs
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

using Qyoto;

using Synapse.ServiceStack;
using Synapse.UI;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;
using Synapse.QtClient;
using Synapse.QtClient.ExtensionNodes;

using Mono.Addins;

namespace Synapse.QtClient.Windows
{
	public partial class PreferencesWindow : QWidget
	{
		public PreferencesWindow ()
		{
			SetupUi();
			
			accountsList.HorizontalHeader().Hide();
			accountsList.VerticalHeader().Hide();
			accountsList.SetModel(new AccountsItemModel(accountsList));
			accountsList.SetItemDelegate(new PaddedItemDelegate(accountsList));		
			accountsList.HorizontalHeader().SetResizeMode(QHeaderView.ResizeMode.Stretch);
			accountsList.HorizontalHeader().SetResizeMode(1, QHeaderView.ResizeMode.ResizeToContents);
	
			extensionsList.HorizontalHeader().Hide();
			extensionsList.VerticalHeader().Hide();
			extensionsList.SetModel(new ExtensionsItemModel(extensionsList));
			extensionsList.SetItemDelegate(new PaddedItemDelegate(extensionsList));
			extensionsList.HorizontalHeader().SetResizeMode(QHeaderView.ResizeMode.Stretch);
			QObject.Connect(extensionsList, Qt.SIGNAL("activated(const QModelIndex &)"), delegate (QModelIndex index) {
				Addin addin = (Addin)index.InternalPointer();
				
				var nodes = AddinManager.GetExtensionNodes("/Synapse/QtClient/AddinPreferencesDialogs");
				foreach (QWidgetTypeExtensionNode node in nodes) {
					if (addin.Id.StartsWith(node.Addin.Id)) {
						QDialog dialog = (QDialog)node.CreateInstance(this);
						dialog.Show();
						dialog.Exec();
						break;
					}
				}
			});
		}
	
		public new void Show ()
		{
			base.Show();
			Gui.CenterWidgetOnScreen(this);
		}
	
		class AccountsItemModel : QAbstractItemModel
		{
			public AccountsItemModel (QObject parent) : base (parent)
			{
				ServiceManager.Get<AccountService>().AccountAdded += HandleAccountsChanged;
				ServiceManager.Get<AccountService>().AccountRemoved += HandleAccountsChanged;
			}
				
			public override int ColumnCount (Qyoto.QModelIndex parent)
			{
				return 2;
			}
				
			public override int RowCount (Qyoto.QModelIndex parent)
			{
				if (!parent.IsValid())
					return ServiceManager.Get<AccountService>().Accounts.Count;
				else
					return 0;
			}
	
			public override QModelIndex Parent (Qyoto.QModelIndex child)
			{
				return new QModelIndex();
			}
	
			public override QModelIndex Index (int row, int column, Qyoto.QModelIndex parent)
			{
				Account account = ServiceManager.Get<AccountService>().Accounts[row];
				return CreateIndex(row, column, account);
			}
	
			public override QVariant Data (Qyoto.QModelIndex index, int role)
			{
				Account account = (Account)index.InternalPointer();
				if (index.Column() == 0) {
					if (role == (int)Qt.ItemDataRole.DisplayRole) {
						return account.Jid.Bare;
					} else if (role == (int)Qt.ItemDataRole.CheckStateRole) {
						return (int)Qt.CheckState.Checked;
					}
				} else if (index.Column() == 1) {
					if (role == (int)Qt.ItemDataRole.DisplayRole) {
						return account.ConnectionState.ToString();
					}
				}
				return new QVariant();
			}
	
			public override uint Flags (Qyoto.QModelIndex index)
			{
				if (index.Column() == 0) {
					return (uint)Qt.ItemFlag.ItemIsUserCheckable | 
					       (uint)Qt.ItemFlag.ItemIsSelectable |
					       (uint)Qt.ItemFlag.ItemIsEnabled;
				} else {
					return (uint)Qt.ItemFlag.ItemIsSelectable;
				}
			}
	
			void HandleAccountsChanged (Account account)
			{
				Application.Invoke(delegate {
					Emit.LayoutChanged();
				});
			}
		}
	
		class ExtensionsItemModel : QAbstractItemModel
		{
			Addin[] m_Addins;
			
			public ExtensionsItemModel (QObject parent) : base (parent)
			{
				m_Addins = AddinManager.Registry.GetAddins();
			}
	
			public override int ColumnCount (Qyoto.QModelIndex parent)
			{
				return 1;
			}
	
			public override int RowCount (Qyoto.QModelIndex parent)
			{
				return m_Addins.Length;
			}
	
			public override QModelIndex Parent (Qyoto.QModelIndex child)
			{
				return new QModelIndex();
			}
	
			public override QModelIndex Index (int row, int column, Qyoto.QModelIndex parent)
			{
				return CreateIndex(row, column, m_Addins[row]);
			}
	
			public override QVariant Data (Qyoto.QModelIndex index, int role)
			{
				Addin addin = m_Addins[index.Row()];
				if (role == (int)Qt.ItemDataRole.DisplayRole) {
					return addin.Name;
				} else if (role == (int)Qt.ItemDataRole.CheckStateRole) {
					return addin.Enabled ? (int)Qt.CheckState.Checked : (int)Qt.CheckState.Unchecked;
				} else {
					return new QVariant();
				}
			}
			
			public override uint Flags (Qyoto.QModelIndex index)
			{
				return (uint)Qt.ItemFlag.ItemIsUserCheckable | 
				       (uint)Qt.ItemFlag.ItemIsSelectable |
				       (uint)Qt.ItemFlag.ItemIsEnabled;
			}
		}
		
		class PaddedItemDelegate : QStyledItemDelegate
		{
			public PaddedItemDelegate (QObject parent) : base (parent)
			{
			}
			
			public override QSize SizeHint (Qyoto.QStyleOptionViewItem option, Qyoto.QModelIndex index)
			{
				var hint = base.SizeHint(option, index);
				hint.SetHeight(hint.Height() + 12);
				return hint;
			}
		}
	}
}