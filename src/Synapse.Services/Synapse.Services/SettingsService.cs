//
// SettingsService.cs
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
using System.IO;
using System.Xml.Serialization;
using Synapse.Core;
using Synapse.ServiceStack;

namespace Synapse.Services
{	
	public class SettingsService : IService, IRequiredService, IInitializeService, IDisposable
	{
		string m_FileName = Path.Combine(Paths.ApplicationData, "settings.xml");
		SerializableDictionary<string, object> m_Settings;
		
		public void Initialize ()
		{
			if (File.Exists(m_FileName)) {
				XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, object>));
				using (StreamReader reader = new StreamReader(m_FileName)) {
					m_Settings = (SerializableDictionary<string, object>)serializer.Deserialize(reader);
				}
			} else {
				m_Settings = new SerializableDictionary<string, object>();
			}
		}

		public void Set (string name, object val)
		{
			m_Settings[name] = val;
			Save();
		}

		public T Get<T> (string name)
		{
			if (m_Settings.ContainsKey(name))
				return (T)m_Settings[name];
		    else
		    	return default(T);
		}

		public string ServiceName {
			get {
				return "SettingsService";
			}
		}
		
		public void Dispose ()
		{
			Save();
		}

		void Save ()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, object>));
			using (StreamWriter writer = new StreamWriter(m_FileName)) {
				serializer.Serialize(writer, m_Settings);
			}
		}
	}
}
