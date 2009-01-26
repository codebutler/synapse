//
// ActivityFeedService.cs
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
using System.Collections.Generic;
using System.Text;
using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Services;
using Notifications;
using Mono.Addins;

namespace Synapse.Services
{
	public class ActivityFeedService : IService, IRequiredService, IInitializeService
	{
		public delegate void ActivityFeedItemEventHandler (IActivityFeedItem item);

		Queue<IActivityFeedItem> m_Queue = new Queue<IActivityFeedItem>();
		
		public event ActivityFeedItemEventHandler NewItem;
		
		public void Initialize ()
		{
			AddTemplate("synapse", "{0}", "{0}");
			
			Application.Client.Started +=  delegate {
			//	PostItem(null, null, "synapse", "Welcome to Synapse!", null);
			};
		}

		Dictionary<string, ActivityFeedItemTemplate> m_Templates = new Dictionary<string, ActivityFeedItemTemplate>();
		
		public IDictionary<string, ActivityFeedItemTemplate> Templates {
			get {
				return new ReadOnlyDictionary<string, ActivityFeedItemTemplate>(m_Templates);
			}
		}

		public void AddTemplate (string name, string singularText, string pluralText, params NotificationAction[] actions)
		{
			AddTemplate(name, singularText, pluralText, new Dictionary<string,object>(), actions);
		}

		public void AddTemplate (string name, string singularText, string pluralText, Dictionary<string, object> options, params NotificationAction[] actions)
		{
			bool desktopNotify    = (options.ContainsKey("DesktopNotify") && ((bool)options["DesktopNotify"]) == true);
			bool showInMainWindow = (options.ContainsKey("ShowInMainWindow") && ((bool)options["ShowInMainWindow"]) == true);
			string iconUrl        = (options.ContainsKey("IconUrl") ? (string)options["IconUrl"] : null);

			m_Templates.Add(name, new ActivityFeedItemTemplate(name, singularText, pluralText, desktopNotify, showInMainWindow, iconUrl, actions));
		}

		public void PostItem (IActivityFeedItem item)
		{
			if (NewItem == null) {
				lock (m_Queue)
					m_Queue.Enqueue(item);
			} else {
				NewItem(item);
			}
			
			var template = m_Templates[item.Type];
			if (template.DesktopNotify) {
				var text = new StringBuilder();
				text.Append(item.FromName);
				text.Append(" ");
				text.AppendFormat(template.SingularText, item.ActionItem);	
				if (String.IsNullOrEmpty(item.Content)) {
					text.Append(".");
				} else {
					text.Append(":");
				}			
				DesktopNotify(template, item, text.ToString());
			}
		}		
			
		public void FireQueued () 
		{
			if (NewItem == null) {
				throw new InvalidOperationException("No event handler for NewItem");
			}
			lock (m_Queue) {
				while (m_Queue.Count > 0)
					NewItem(m_Queue.Dequeue());
			}
		}

		public string ServiceName {
			get { return "ActivityFeedService"; }
		}

		void DesktopNotify (ActivityFeedItemTemplate template, IActivityFeedItem item, string text)
		{
			Notification notif = new Notification(text, item.Content);
			foreach (var action in template.Actions) {
				notif.AddAction(action.Name, action.Label, delegate {
					item.TriggerAction(action.Name);
				});
			}			
			notif.Show ();
		}
	}

	public class ActivityFeedItemTemplate
	{
		string m_Name;
		string m_SingularText;
		string m_PluralText;
		bool   m_DesktopNotify;
		bool   m_ShowInMainWindow;
		string m_IconUrl;
		NotificationAction[] m_Actions;

		public ActivityFeedItemTemplate (string name, string singularText, string pluralText, bool desktopNotify,
		                                 bool showInMainWindow, string iconUrl, params NotificationAction[] actions)
		{
			m_Name = name;
			m_SingularText = singularText;
			m_PluralText = pluralText;
			m_DesktopNotify = desktopNotify;
			m_ShowInMainWindow = showInMainWindow;
			m_IconUrl = iconUrl;
			m_Actions = actions;
		}

		public string Name {
			get {
				return m_Name;
			}
		}

		public string SingularText {
			get {
				return m_SingularText;
			}
		}
		
		public string PluralText {
			get {
				return m_PluralText;
			}
		}
		
		public bool DesktopNotify {
			get {
				return m_DesktopNotify;
			}
		}

		public bool ShowInMainWindow {
			get {
				return m_ShowInMainWindow;
			}
		}

		public string IconUrl {
			get {
				return m_IconUrl;
			}
		}
		
		public NotificationAction[] Actions {
			get {
				return m_Actions;
			}
		}
	}

	public abstract class AbstractActivityFeedItem : IActivityFeedItem
	{
		object m_Data;

		protected AbstractActivityFeedItem ()
		{
		}
		
		protected AbstractActivityFeedItem (object data)
		{
			m_Data = data;
		}
		
		public event NotificationActionCallback ActionTriggered;
		
		public virtual void TriggerAction (string actionName)
		{
			var template = ServiceManager.Get<ActivityFeedService>().Templates[Type];
			foreach (var action in template.Actions) {
				if (action.Name == actionName) {
					action.Callback(this, action);

					if (ActionTriggered != null)
						ActionTriggered(this, action);
					
					return;
				}
			}
			throw new Exception("Action not found: " + actionName);
		}

		public object Data {
			get {
				return m_Data;
			}
		}
		
		public abstract string FromName {
			get;
		}
		
		public abstract string FromUrl {
			get;
		}
		
		public abstract string AvatarUrl {
			get;
		}
		
		public abstract string Type {
			get;
		}
		
		public abstract string ActionItem {
			get;
		}
		
		public abstract string Content {
			get;
		}
		
		public abstract Uri ContentUrl {
			get;
		}		
	}
	
	public interface IActivityFeedItem
	{
		event NotificationActionCallback ActionTriggered;
		
		string FromName {
			get;
		}

		string FromUrl {
			get;
		}

		string AvatarUrl {
			get;
		}

		string Type {
			get;
		}

		string ActionItem {
			get;
		}

		string Content {
			get;
		}

		Uri ContentUrl {
			get;
		}

		object Data {
			get;
		}
		
		void TriggerAction (string actionName);
	}

	public delegate void NotificationActionCallback (IActivityFeedItem item, NotificationAction action);
	
	public class NotificationAction
	{
		public string Name { get; set; }
		public string Label { get; set; }
		public NotificationActionCallback Callback { get; set; }
	}
}
