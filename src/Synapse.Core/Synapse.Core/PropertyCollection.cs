//
// PropertyColleciton.cs:
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

namespace Synapse.Core
{
	public class PropertyCollection
	{
		Dictionary<string, object> m_Dictionary;
		
		public PropertyCollection()
		{
			m_Dictionary = new Dictionary<string, object>();
		}
		
		public T Get<T>(string name)
		{
			lock (m_Dictionary) {
				return (T)m_Dictionary[name];
			}
		}
		
		public void Set(string name, object value)
		{
			lock (m_Dictionary) {
				m_Dictionary[name] = value;
			}
		}
		
		public bool Contains(string name)
		{
			lock (m_Dictionary) {
				return m_Dictionary.ContainsKey(name);
			}
		}
		
		public void Remove(string name)
		{
			lock (m_Dictionary) {
				m_Dictionary.Remove(name);
			}
		}
	}
}
