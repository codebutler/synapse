// 
// ActionService.cs
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
using System.Reflection;
using System.Collections.Generic;

using Synapse.ServiceStack;
using Synapse.UI.Actions.ExtensionNodes;

using Mono.Addins;

namespace Synapse.UI.Services
{
	public class ActionService : IService, IRequiredService, IInitializeService
	{
		Dictionary<string, List<EventHandler>> m_ActionHandlers;
		Dictionary<string, ActionTemplate> m_ActionTemplates = new Dictionary<string, ActionTemplate>();
		object m_SeparatorAction;
			
		public void Initialize ()
		{
			m_ActionHandlers = new Dictionary<string, List<EventHandler>>();

			m_SeparatorAction = Application.CreateAction(null, null, null, null);
			
			foreach (ActionHandlerCodon node in AddinManager.GetExtensionNodes("/Synapse/UI/ActionHandlers")) {
				Type type = node.HandlerType;
				object handler = node.CreateInstance();
				
				MethodInfo[] methods = node.HandlerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
				foreach (MethodInfo method in methods) {
					foreach (object attr in method.GetCustomAttributes(true)) {
						if (attr is ActionHandlerAttribute) {
							var actionHandlerAttr = (ActionHandlerAttribute)attr;
							string actionId = actionHandlerAttr.ActionId;
							if (!m_ActionHandlers.ContainsKey(actionId)) {
								m_ActionHandlers[actionId] = new List<EventHandler>();
							}
							EventHandler handlerMethod = (EventHandler) Delegate.CreateDelegate(typeof(EventHandler), handler, method);
							m_ActionHandlers[actionId].Add(handlerMethod);
						}
					}
				}
			}

			foreach (ActionCodon node in AddinManager.GetExtensionNodes("/Synapse/UI/Actions")) {
				if (!m_ActionHandlers.ContainsKey(node.Id))
					throw new Exception("No action handler(s) for: " + node.Id);
				m_ActionTemplates.Add(node.Id, (ActionTemplate)node.CreateInstance());
			}
		}

		public object CreateAction (string id, object parent)
		{
			if (!m_ActionTemplates.ContainsKey(id))
				throw new Exception("Action not found: " + id);

			var template = m_ActionTemplates[id];
			return Application.CreateAction(template.Id, template.Label, template.Icon, parent);
		}

		public object GetSeparatorAction ()
		{
			return m_SeparatorAction;	
		}

		public void TriggerAction (string id, object action)
		{
			if (!m_ActionHandlers.ContainsKey(id))
				throw new Exception("Action not found: " + id);

			foreach (EventHandler handler in m_ActionHandlers[id])
				handler(action, EventArgs.Empty);
		}
		
		public string ServiceName {
			get {
				return "ActionService";
			}
		}
	}
}