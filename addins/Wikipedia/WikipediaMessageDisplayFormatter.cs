using System;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Synapse.UI.Chat;
using jabber.protocol.client;

namespace Synapse.Addins.Wikipedia
{
	public class WikipediaMessageDisplayFormatter : IMessageDisplayFormatter
	{
		const string WIKIPEDIA_PAGE_LINK_PATTERN = @"<a href=""http://(www\.)?([a-zA-Z]{2})\.wikipedia\.org/wiki/(.*?)""";		
		
		public bool SupportsMessage(string bodyHtml, Message message)
		{
			return Regex.IsMatch(bodyHtml, WIKIPEDIA_PAGE_LINK_PATTERN);
		}

		public string FormatMessage(string bodyHtml, Message message)
		{
			Match match = Regex.Match(bodyHtml, WIKIPEDIA_PAGE_LINK_PATTERN);
			return bodyHtml + BuildHtmlPreview("http://" + match.Groups[2] + ".wikipedia.org/wiki/" + match.Groups[3]); // Groups[2] = Location , Groups[3] = Article
		}

		public bool StopAfter {
			get {
				return false;
			}
		}

		string BuildHtmlPreview(string linkUrl)
		{
			try {
				Uri url = new Uri(linkUrl);
				HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
				request.Timeout = 2000;
				WebResponse response = request.GetResponse();
				string sourceCode = null;
				using (StreamReader streamReader = new StreamReader(response.GetResponseStream())) {
					sourceCode = streamReader.ReadToEnd();
				}
				if (!String.IsNullOrEmpty(sourceCode)) {
					Regex regex = new Regex("<p>(.*)</p>");
					if (regex.IsMatch(sourceCode)) {
						Match match = regex.Match(sourceCode);
						string article = match.Groups[1].ToString();
						string temp = Regex.Replace(article, "<.*?>", "");
						temp = Regex.Replace(temp, @"\[\d*?\]", "");
						return String.Format("<br/><p style=\"background-color:white; color:black; border-width:1px; border-style:solid;\">{0}</p>", temp);
					}
				}
			} catch {
				Console.Error.WriteLine("WikipediaMessageDisplayFormatter.BuildHtmlPreview(string linkUrl) has an error (Maybe The Article doesn't exist?)");
			}
			return String.Empty;
		}
	}
}
