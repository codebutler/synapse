//
// WebIdentityConfiguratorCodon.cs
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

using Synapse.Xmpp;

using Mono.Addins;

namespace Synapse.QtClient.ExtensionNodes
{	
	public class WebIdentityConfiguratorCodon : TypeExtensionNode
	{	
		[NodeAttribute("name", true, "Name of website")]
		string m_Name;
		
		[NodeAttribute("description", true, "Description explaining what this addin does")]
		string m_Description;

		[NodeAttribute("icon", true, "Website icon")]
		string m_IconUri;
		
		public WebIdentityConfiguratorCodon()
		{
			
		}
		
		public string Name {
			get {
				return m_Name;
			}
		}
		
		public string Description {
			get {
				return m_Description;
			}
		}
		
		public string IconUri {
			get {
				return m_IconUri;
			}
		}
		
		public IWebIdentityConfigurator CreateInstance (Account account)
		{
			return (IWebIdentityConfigurator)Activator.CreateInstance(base.Type, account);
		}
	}
}
