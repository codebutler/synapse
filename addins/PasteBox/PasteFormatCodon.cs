// PasteFormatCodon.cs
// 
// Copyright (C) 2009 [name of author]
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
//

using System;
using Mono.Addins;

namespace Synapse.Addins.PasteBox
{
	public class PasteFormatCodon : ExtensionNode
	{
		[NodeAttribute("name", "Name")]
		string m_Name;

		[NodeAttribute("mimeType", "Mime Type")]
		string m_MimeType;


		[NodeAttribute("class", "Class")]
		string m_Class;
		
		public string Name {
			get {
				return m_Name;
			}
		}

		public string MimeType {
			get {
				return m_MimeType;
			}
		}

		public new IPasteFormatter CreateInstance ()
		{
			return (IPasteFormatter)Activator.CreateInstance(Addin.GetType(m_Class), m_MimeType);
		}
	}
}
