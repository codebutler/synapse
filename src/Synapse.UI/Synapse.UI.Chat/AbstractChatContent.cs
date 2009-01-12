//
// AbstractChatContent.cs
// 
// Copyright (C) 2009 Eric Butler
//
// Authors:
//   Eric Butler <eric@extremeboredom.net>
//
// Based on code from the Adium project.
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
using Synapse.Xmpp;
using jabber;

namespace Synapse.UI.Chat
{
	public abstract class AbstractChatContent
	{
		Account      m_Account;
		string       m_MessageHtml;
		List<string> m_CustomDisplayClasses = new List<string>();
		JID          m_Source;
		JID          m_Destination;
		DateTime     m_Date;

		public AbstractChatContent (Account account, JID source, JID destination)
			: this (account, source, destination, DateTime.Now)
		{			
		}
		
		public AbstractChatContent (Account account, JID source, JID destination, DateTime date)
		{
			if (account == null)
				throw new ArgumentNullException("account");

			if (date == null)
				throw new ArgumentNullException("date");
			
			m_Account     = account;
			m_Source      = source;
			m_Destination = destination;
			m_Date        = date;
		}
		
		public string MessageHtml {
			get {
				return m_MessageHtml;
			}
			set {
				m_MessageHtml = value;
			}
		}

		public bool IsSimilarToContent(AbstractChatContent otherContent)
		{
			if (this.Source == otherContent.Source && this.Type == otherContent.Type) {
				TimeSpan span = otherContent.Date.Subtract(this.Date);
				return (span.TotalSeconds < 300);
			}
			return false;
		}

		public bool IsFromSameDayAsContent(AbstractChatContent otherContent)
		{
			// FIXME:
			throw new NotImplementedException();
		}

		/*
		public abstract AIChat Chat {
			get;
		}

		public abstract AIListObject Source {
			get;
		}

		public abstract AIListObject Destination {
			get;
		}
		
		public abstract NSDictionary UserInfo {
			get;
		}
		*/

		public Account Account {
			get {
				return m_Account;
			}
		}
		
		public JID Source {
			get {
				return m_Source;
			}
		}

		public JID Destination {
			get {
				return m_Destination;
			}
		}
		
		public DateTime Date {
			get {
				return m_Date;
			}
		}

		public bool FilterContent {
			get;
			set;
		}

		public bool TrackContent {
			get;
			set;
		}
		
		public bool DisplayContent {
			get;
			set;
		}

		public bool DisplayContentImmediately {
			get;
			set;
		}

		public bool SendContent {
			get;
			set;
		}

		public bool PostProcessContent {
			get;
			set;
		}

		public bool IsOutgoing {
			get;
			set;
		}

		public virtual string[] DisplayClasses {
			get {
				List<string> classes = new List<string>();
				classes.Add(IsOutgoing ? "outgoing" : "incoming");
				return classes.ToArray();
			}
		}

		public void AddDisplayClass(string className)
		{
			m_CustomDisplayClasses.Add(className);
		}

		public abstract ChatContentType Type {
			get;
		}
	}

	public enum ChatContentType {
		Typing,
		Message,
		Notification,
		Event,
		Status,
		Context,
		FileTransfer
	}
}
