//
// ActionCodon.cs
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

using Synapse.ServiceStack;
using Mono.Addins;

namespace Synapse.UI.Actions.ExtensionNodes
{	
	public class ActionCodon : TypeExtensionNode
	{
		[NodeAttribute("_label", true, "Label", Localizable=true)]
		string m_Label;

		[NodeAttribute("icon", "blahblah")]
		string m_Icon;
		
		public override object CreateInstance ()
		{
			return new ActionTemplate(Id, m_Label, m_Icon);
		}
	}	

	public class ActionTemplate
	{
		string m_Id, m_Label, m_Icon;
		
		public ActionTemplate (string id, string label, string icon)
		{
			m_Id    = id;
			m_Label = label;
			m_Icon  = icon;
		}
		
		public string Id { 
			get {
				return m_Id;
			}
		}
		
		public string Label { 
			get {
				return m_Label;
			}
		}
		
		public string Icon { 
			get {
				return m_Icon;
			}
		}
	}
}