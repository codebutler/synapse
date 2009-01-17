//
// TwitterXmlApi.cs
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
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using System.Web;
using System.Net;

namespace Twitter
{
	public class TwitterClient
	{
		static readonly string FriendsTimelineUrl = "http://twitter.com/statuses/friends_timeline.xml";
		static readonly string RepliesUrl = "http://twitter.com/statuses/replies.xml";
		static readonly string UpdateUrl = "http://twitter.com/statuses/update.xml";
		static readonly string DirectMessagesUrl = "http://twitter.com/direct_messages.xml";
		
		public TwitterClient ()
		{
			FriendsTimelineLastCheckedAt = DateTime.MinValue;
			RepliesLastCheckedAt = DateTime.MinValue;
			DirectMessagesLastChecked = DateTime.MinValue;
		}
		
		public DateTime FriendsTimelineLastCheckedAt {
			get;
			set;
		}

		public DateTime RepliesLastCheckedAt {
			get;
			set;
		}

		public DateTime DirectMessagesLastChecked {
			get;
			set;
		}

		public string Username {
			get;
			set;
		}

		public string Password {
			get;
			set;
		}

		public IEnumerable<AbstractTwitterItem> FriendsAndRepliesAndMessages(bool since)
		{
			foreach (var status in FriendsTimeline(since))
				yield return status;
			foreach (var status in Replies(since))
				yield return status;

			var messages = DirectMessages(since);
			if (messages != null) {
				foreach (var message in messages)
					yield return message;
			}
		}

		public Statuses FriendsTimeline()
		{
			return FriendsTimeline(false);
		}

		public Statuses FriendsTimeline(bool since)
		{
			var args = new Dictionary<string,string>();
			if (since && FriendsTimelineLastCheckedAt != DateTime.MinValue)
				args["since"] = FriendsTimelineLastCheckedAt.ToString("r");
			
			var statuses = Request<Statuses>(FriendsTimelineUrl, args);			
			FriendsTimelineLastCheckedAt = DateTime.Now.ToUniversalTime();
			return statuses;
		}

		public DirectMessages DirectMessages()
		{
			return DirectMessages(false);
		}

		public DirectMessages DirectMessages(bool since)
		{
			var args = new Dictionary<string,string>();
			if (since && DirectMessagesLastChecked != DateTime.MinValue)
				args["since"] = DirectMessagesLastChecked.ToString("r");
			
			try {
				var messages = Request<DirectMessages>(DirectMessagesUrl, args);
				DirectMessagesLastChecked = DateTime.Now.ToUniversalTime();				
				return messages;
			} catch (InvalidOperationException ex) {
				// Stupid twitter api inconsistency
				// http://markmail.org/message/yaammnseabyvdx3m
			}
			return null;
		}
		
		public Statuses Replies()
		{
			return Replies(false);
		}

		public Statuses Replies(bool since)
		{
			var args = new Dictionary<string,string>();
			if (since && RepliesLastCheckedAt != DateTime.MinValue)
				args["since"] = RepliesLastCheckedAt.ToString("r");			
			var statuses = Request<Statuses>(RepliesUrl, args);
			RepliesLastCheckedAt = DateTime.Now.ToUniversalTime();
			return statuses;
		}

		public Status Update (string status)
		{
			var args = new Dictionary<string, string>() { { "status", status } };
			return Request<Status>(UpdateUrl, args, true);
		}

		T Request<T> (string url, Dictionary<string,string> args)
		{
			return Request<T>(url, args, false);
		}
		
		T Request<T> (string url, Dictionary<string,string> args, bool post)
		{
			string argsString = String.Empty;
			if (args != null && args.Count > 0) {
				foreach (var arg in args)
					argsString += HttpUtility.UrlEncode(arg.Key) + "=" + HttpUtility.UrlEncode(arg.Value);
			}

			HttpWebRequest request = null;
			if (post) {
				request = (HttpWebRequest)HttpWebRequest.Create(url);
				request.ServicePoint.Expect100Continue = false;
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.UserAgent = "Synapse";
				using (StreamWriter writer = new StreamWriter(request.GetRequestStream())) {
					writer.Write(argsString);
				}
			} else {
				url += "?" + argsString;
				request = (HttpWebRequest)HttpWebRequest.Create(url);
			}
			
			request.Credentials = new NetworkCredential(Username, Password);
			var response = request.GetResponse();
			using (var stream = response.GetResponseStream()) {
				var serializer = new XmlSerializer(typeof(T));
				return (T)serializer.Deserialize(stream);
			}
		}
	}

	[XmlRoot("direct-messages")]
	public class DirectMessages : List<DirectMessage>
	{
		
	}

	[XmlType("direct_message")]
	public class DirectMessage : AbstractTwitterItem
	{
		[XmlElement("sender_id")]
		public int SenderID {
			get;
			set;
		}

		[XmlElement("recipient_id")]
		public int RecipientID {
			get;
			set;
		}

		[XmlElement("sender_screen_name")]
		public string SenderScreenName {
			get;
			set;
		}

		[XmlElement("recipient_screen_name")]
		public string RecipientScreenName {
			get;
			set;
		}
		
		[XmlElement("sender")]
		public User Sender {
			get;
			set;
		}

		[XmlElement("recipient")]
		public User Recipient {
			get;
			set;
		}
	}
	
	[XmlRoot("statuses")]
	public class Statuses : List<Status>
	{
	}
	
	[XmlType("status")]
	public class Status : AbstractTwitterItem
	{
		[XmlElement("source")]
		public string Source {
			get;
			set;
		}

		[XmlElement("truncated")]
		public bool Truncated {
			get;
			set;
		}
		
		[XmlElement("in_reply_to_status_id")]
		public string InReplyToStatusId {
			get;
			set;
		}

		[XmlElement("in_reply_to_user_id")]
		public string InReplyToUserId {
			get;
			set;
		}

		[XmlElement("favorited")]
		public bool Favorited {
			get;
			set;
		}

		[XmlElement("user")]
		public User User {
			get;
			set;
		}
	}

	public class User
	{
		[XmlElement("id")]
		public int ID {
			get;
			set;
		}
		
		[XmlElement("name")]
		public string Name {
			get;
			set;
		}

		[XmlElement("screen_name")]
		public string ScreenName {
			get;
			set;
		}

		[XmlElement("location")]
		public string Location {
			get;
			set;
		}

		[XmlElement("description")]
		public string Description {
			get;
			set;
		}

		[XmlElement("profile_image_url")]
		public string ProfileImageUrl {
			get;
			set;
		}

		[XmlElement("url")]
		public string Url {
			get;
			set;
		}

		[XmlElement("protected")]
		public bool Protected {
			get;
			set;
		}

		[XmlElement("followers_count")]
		public int FollowersCount {
			get;
			set;
		}
	}

	public abstract class AbstractTwitterItem
	{		
		[XmlElement("created_at")]
		public string CreatedAt {
			get;
			set;
		}

		public DateTime CreatedAtDT {
			get {
				return DateTime.ParseExact(CreatedAt, "ddd MMM dd HH:mm:ss zzzz yyyy", null);
			}
		}

		[XmlElement("id")]
		public int ID {
			get;
			set;
		}

		[XmlElement("text")]
		public string Text {
			get;
			set;
		}

	}
}
