//
// XmppUriQueryInfo.cs
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
using System.Web;

namespace Synapse.Xmpp
{
	public class XmppUriQueryInfo
	{
		string m_QueryType;
		IDictionary<string, string> m_Parameters;

		public static XmppUriQueryInfo ParseQuery (string query)
		{
			if (!query.StartsWith("?"))
				throw new ArgumentException("Invalid query. Must be begin with '?'.");
	
			query = query.Substring(1);
			
			string queryType  = null;
			var parameters = new Dictionary<string, string>();
			
			if (query.IndexOf(";") > -1) {
				queryType = query.Substring(0, query.IndexOf(";"));
				var paramPairs = query.Substring(query.IndexOf(";")+1).Split(';');
				foreach (var pair in paramPairs) {
					var kv = pair.Split('=');
					string key = HttpUtility.UrlDecode(kv[0]);
					string val = HttpUtility.UrlDecode(kv[1]);
					parameters.Add(key, val);
				}
			} else {
				queryType = query;
			}
	
			return new XmppUriQueryInfo(queryType, parameters);
		}

		private XmppUriQueryInfo (string queryType, IDictionary<string, string> parameters)
		{
			m_QueryType = queryType;
			m_Parameters = parameters;
		}

		public string QueryType {
			get {
				return m_QueryType;
			}
		}

		public IDictionary<string, string> Parameters {
			get {
				return m_Parameters;
			}
		}
	}
}
