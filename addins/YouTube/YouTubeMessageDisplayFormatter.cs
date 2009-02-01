//
// YouTubeMessageDisplayFormatter.cs
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
using System.Net;
using System.Web;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using Synapse.ServiceStack;
using Synapse.UI.Chat;
using jabber.protocol.client;

namespace Synapse.Addins.YouTube
{
	public class YouTubeMessageDisplayFormatter : IMessageDisplayFormatter
	{
		const string YOUTUBE_LINK_PATTERN = @"(<a\s.*?href=\""http://(www\.)?youtube.com/watch\?v=(.*?)\"".*?>.*?</a>)";
		
		public bool SupportsMessage (string bodyHtml, Message message)
		{
			return Regex.IsMatch(bodyHtml, YOUTUBE_LINK_PATTERN);
		}
		
		public string FormatMessage (string bodyHtml, Message message)
		{
			return Regex.Replace(bodyHtml, YOUTUBE_LINK_PATTERN, delegate (Match match) {
				string youtubeId = match.Groups[3].Value;
				return match.Value + BuildPreviewHtml(youtubeId);
			}, RegexOptions.Singleline);
		}

		public bool StopAfter {
			get {
				return false;
			}
		}

		string BuildPreviewHtml (string videoId)
		{
			string url = "http://gdata.youtube.com/feeds/api/videos/" + videoId;

			string title = String.Empty;

			try {
				var request = (HttpWebRequest) HttpWebRequest.Create(url);
				request.Timeout = 2000;
				var response = request.GetResponse();
				string xml = null;
				using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
					xml = reader.ReadToEnd();
				}

				XmlDocument document = new XmlDocument();
				document.LoadXml(xml);
				title = document["entry"]["title"].InnerText;
			} catch (Exception ex) {
				Console.Error.WriteLine(ex);
				title = "Failed to get title";
			}

			return String.Format("<div onclick=\"showVideo(this, '{0}')\" class=\"youtube\" style=\"background-image: url('http://i.ytimg.com/vi/{0}/0.jpg')\">" + 
			                     "<div class=\"play\"></div>" +
			                     "<div class=\"desc\"><div>{1}</div></div>" +
			                     "</div>", videoId, title);
		}
	}
}
