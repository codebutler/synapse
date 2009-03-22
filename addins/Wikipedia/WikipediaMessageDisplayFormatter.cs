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
			return bodyHtml +  buildHtmlPreview("http://" + match.Groups[2] + ".wikipedia.org/wiki/" + match.Groups[3]); // Groups[2] = Location , Groups[3] = Article
        }
		public bool StopAfter {
			get {
				return false;
			}
		}
		string buildHtmlPreview(string linkUrl)
        {			
			try {
				Uri url = new Uri(linkUrl);
				HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
				request.Timeout = 2000;
				WebResponse response = request.GetResponse();
				StreamReader streamReader = new StreamReader(response.GetResponseStream());
				string sourceCode = streamReader.ReadToEnd();
				Regex regex = new Regex("<p>(.*)</p>");
				if(regex.IsMatch(sourceCode))
				{
					Match match = regex.Match(sourceCode);
					string article = match.Groups[1].ToString();
					string temp = Regex.Replace(article, "<.*?>", "");
					return "<br>" +
						   "<p style=\"background-color:white; color:black; border-width:1px; border-style:solid;\">" + 
							Regex.Replace(temp, @"\[\d*?\]", "")
							+ "</p>";
				}
            }
            catch{
							Console.Error.WriteLine("WikipediaMessageDisplayFormatter.buildHtmlPreview(string linkUrl) has an error (Maybe The Article doesn't exist?)");
			}
			return "";
        }
	}
}
