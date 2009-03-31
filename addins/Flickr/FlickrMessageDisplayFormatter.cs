//
// FlickrMessageDisplayFormatter.cs
//
// Copyright (C) 2008-2009 Eric Butler
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

namespace Synapse.Addins.Flickr
{	
	public class FlickrMessageDisplayFormatter : IMessageDisplayFormatter
	{		
		const string FLICKR_PAGE_LINK_PATTERN = @"(<a\s.*?href=\""(http://(www\.)?flickr.com/photos/(.*?)/(.*?)/(.*?))\"".*?>.*?</a>)";

		public bool SupportsMessage (string bodyHtml, Message message)
		{
			return Regex.IsMatch(bodyHtml, FLICKR_PAGE_LINK_PATTERN);
		}
		
		public string FormatMessage (string bodyHtml, Message message)
		{
			return Regex.Replace(bodyHtml, FLICKR_PAGE_LINK_PATTERN, delegate (Match match) {
				string url = match.Groups[2].Value;
				string photoId = match.Groups[5].Value;
				return match.Value + BuildPhotoHtml(url, photoId);
			}, RegexOptions.Singleline);
		}
		
		public bool StopAfter {
			get {
				return false;
			}
		}
		
		string BuildPhotoHtml (string linkUrl, string photoId)
		{
			Uri url = new Uri(Flickr.FLICKR_PHOTOS_GET_INFO_URL + "&api_key=" + Flickr.FLICKR_API_KEY + "&photo_id=" + HttpUtility.UrlEncode(photoId));
			
			try {
				HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
				request.Timeout = 2000;
				var response = request.GetResponse();
				string xml = null;
				using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
					xml = reader.ReadToEnd();
				}
				
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(xml);
				
				if (doc["rsp"] == null || doc["rsp"].GetAttribute("stat") != "ok")
					return String.Empty;
				
				var element = doc["rsp"]["photo"];
				
				string thumbUrl = String.Format("http://farm{0}.static.flickr.com/{1}/{2}_{3}_s.jpg",
				                                element.GetAttribute("farm"),
				                                element.GetAttribute("server"),
				                                element.GetAttribute("id"),
				                                element.GetAttribute("secret"));
				
				string title = element["title"].InnerText;
				
				return String.Format("<br/><a href=\"{0}\"><img width=\"75\" height=\"75\" src=\"{1}\" title=\"{2}\" /></a><br/>", linkUrl, thumbUrl, title);
				
			} catch (Exception ex) {
				Console.Error.WriteLine(ex);
			}
			return String.Empty;
		}
	}
}