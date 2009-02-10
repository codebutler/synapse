//
// CodeMessageDisplayFormatter.cs
// 
// Copyright (C) 2009 Eric Butler
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
using System.Xml;
using Synapse.ServiceStack;
using Synapse.UI.Chat;
using jabber.protocol.client;

namespace Synapse.Addins.CodeSnippets
{	
	public class CodeMessageDisplayFormatter : IMessageDisplayFormatter
	{
		public bool SupportsMessage (string bodyHtml, Message message)
		{
			XmlElement bodyElement = message["body"];
			return (bodyElement != null && !String.IsNullOrEmpty(bodyElement.GetAttribute("language", CodeSnippetsService.CODE_NS)));
		}
		
		public string FormatMessage (string bodyHtml, Message message)
		{		
			var service = ServiceManager.Get<CodeSnippetsService>();
			return service.FormatMessageBody(message["body"]);
		}

		public bool StopAfter {
			get {
				return true;
			}
		}
	}
}
