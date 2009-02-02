//
// PList.cs: Parses mac plist files.
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
using System.Xml;

namespace Synapse.Core
{
	public class PList
	{
		Dictionary<string, object> values = new Dictionary<string, object>();
		
		public PList(string fileName)
		{		
			XmlDocument document = new XmlDocument();
			document.Load(fileName);
			XmlNodeList dictChildren = document.SelectSingleNode("/plist/dict").ChildNodes;
			for (int i = 0; i < dictChildren.Count; i++) {
				XmlNode keyNode = dictChildren[i];
				if (keyNode.Name == "key") {
					string key = keyNode.InnerText;
					XmlNode valueNode = dictChildren[++i];
					
					if (values.ContainsKey(key)) {
						throw new Exception("Key already exists!");
					}
					
					switch (valueNode.Name) {
					case "string":
						values[key] = valueNode.InnerText;
						break;
					case "real":
					case "integer":
						// XXX: Don't always store this as a long. Inspect the 
						// number and figure out the best type to use.
						values[key] = Convert.ToInt64(valueNode.InnerText);
						break;
					case "true":
						values[key] = true;
						break;
					case "false":
						values[key] = false;
						break;
					case "date":
					case "data":
					case "array":
					case "dict":
					default:
						throw new Exception(String.Format("Unsupported type: {0}", valueNode.Name));	                
					}
                } else {
					throw new Exception("Unexpected node");
				}
			}
		}
		
		public T GetValue<T>(string key)
		{
			if (values.ContainsKey(key)) {
				return (T)values[key];
			} else {
				return default(T);
				//throw new Exception("Key not found");
			}
		}
		
		public bool ContainsKey(string key)
		{
			return values.ContainsKey(key);
		}
	}
}