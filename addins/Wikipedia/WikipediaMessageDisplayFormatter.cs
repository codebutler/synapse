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

                const string STYLE_BEGIN = "<p style=\"background-color:white; color:black; border-width:1px; border-style:solid;\">";
                const string STYLE_END = "</p>";
		
		public bool SupportsMessage(string bodyHtml, Message message)
		{
			return Regex.IsMatch(bodyHtml, WIKIPEDIA_PAGE_LINK_PATTERN);
		}

		public string FormatMessage(string bodyHtml, Message message)
		{
                        string wikipediaPreviews = bodyHtml;
                        MatchCollection matchCollection = Regex.Matches(bodyHtml, WIKIPEDIA_PAGE_LINK_PATTERN);
                        foreach(Match match in matchCollection)
                        {
                               string linkUrl = "http://" + match.Groups[2] + ".wikipedia.org/wiki/" + match.Groups[3]; // Groups[2] = Location , Groups[3] = Article
                                wikipediaPreviews += BuildHtmlPreview(linkUrl);                                 
                        }
                        return wikipediaPreviews;
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
                                                if(temp.Length > 160)
                                                {
                                                        temp = String.Format("{0}[...]", temp.Remove(160));
                                                }
                                                return String.Format("{0}{1}{2}", STYLE_BEGIN, temp, STYLE_END);
					}
				}
			} catch {
				Console.Error.WriteLine("WikipediaMessageDisplayFormatter.BuildHtmlPreview(string linkUrl) has an error (Maybe The Article doesn't exist?)");
			}
			return String.Empty;
		}
	}
}
