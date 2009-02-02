

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
				
				return String.Format("<a href=\"{0}\"><img width=\"75\" height=\"75\" src=\"{1}\" title=\"{2}\" /></a>", linkUrl, thumbUrl, title);				
				
			} catch (Exception ex) {
				Console.Error.WriteLine(ex);
			}
			return String.Empty;
		}
	}
}