//
// BookmarkedMUCsModel.cs
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
using System.Linq;
using Qyoto;
using Synapse.ServiceStack;
using Synapse.Services;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;
using jabber.protocol.iq;
using jabber.client;

namespace Synapse.QtClient
{
	public class BookmarkedMUCsModel : QAbstractItemModel
	{
		AccountService m_AccountService;
		                                
		public BookmarkedMUCsModel()
		{
			m_AccountService = ServiceManager.Get<AccountService>();
			m_AccountService.AccountAdded   += HandleAccountAdded;
			m_AccountService.AccountRemoved += HandleAccountRemoved;
			foreach (Account account in m_AccountService.Accounts)
				HandleAccountAdded(account);
		}

		void HandleAccountAdded(Account account)
		{
			account.ConnectionStateChanged += HandleConnectionStateChanged;
			account.BookmarkManager.OnConferenceAdd += HandleOnConference;
			account.BookmarkManager.OnConferenceRemove += HandleOnConference;
		}

		void HandleConnectionStateChanged(Account account)
		{
			Application.Invoke(delegate {
				Emit.LayoutChanged();
			});			
		}

		void HandleAccountRemoved(Account account)
		{
			account.BookmarkManager.OnConferenceAdd	   -= HandleOnConference;
			account.BookmarkManager.OnConferenceRemove -= HandleOnConference;			
		}

		void HandleOnConference(BookmarkManager manager, BookmarkConference conference)
		{
			Application.Invoke(delegate {
				Emit.LayoutChanged();
			});
		}
		
		public override QVariant Data (QModelIndex index, int role)
		{
			object item = index.InternalPointer();
			if (role == (int)Qt.ItemDataRole.DisplayRole) {
				if (item is Account)
					return ((Account)item).Jid.Bare;
				else if (item is BookmarkConference) {
					BookmarkConference conf = (BookmarkConference)item;
					return String.Format("{0} ({1})", conf.ConferenceName, conf.JID);
				}
			} 
			return new QVariant();
		}

		public override int ColumnCount (Qyoto.QModelIndex parent)
		{
			return 1;
		}
		
		public override int RowCount (Qyoto.QModelIndex parent)
		{
			if (!parent.IsValid()) {
				return m_AccountService.Accounts.Count;
			} else {
				if (parent.InternalPointer() is Account) {
					Account account = (Account)parent.InternalPointer();
					return account.BookmarkManager.Count;
				} else {
					return 0;
				}
			}
		}

		public override QModelIndex Parent (Qyoto.QModelIndex child)
		{
			if (!child.IsValid())
				return new QModelIndex();
			
			if (child.InternalPointer() is Account) {
				return new QModelIndex();
			} else {
				BookmarkConference bmark = (BookmarkConference)child.InternalPointer();
				Account account = m_AccountService.Accounts.FirstOrDefault(a => a.BookmarkManager.Conferences.Contains(bmark));
				if (account != null)
					return CreateIndex(m_AccountService.Accounts.IndexOf(account), 0, account);
				else
					return new QModelIndex();
			}
		}

		public override QModelIndex Index (int row, int column, Qyoto.QModelIndex parent)
		{
			if (!HasIndex(row, column, parent))
				return new QModelIndex();
                  
			if (!parent.IsValid()) {
				Account account = m_AccountService.Accounts[row];
				return CreateIndex(row, column, account);
			} else {
				Account account = (Account)parent.InternalPointer();
				BookmarkConference bmark = account.BookmarkManager.Conferences.ElementAt(row);
				return CreateIndex(row, column, bmark);
			}
		}
	}
}
