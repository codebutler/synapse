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
		const string WIKIPEDIA_PAGE_LINK_PATTERN = @"(<a\s.*?href=""(http://(www\.)?([a-zA-Z]{2})\.wikipedia.(com|org)/wiki/(.*?))"".*?>.*?</a>)";

		const string STYLE_BEGIN = "<p style=\"min-height: 20px; margin-top: 0px; padding: 2px 2px 2px 22px; background: url(resource:/wikipedia/logo-16.png) 2px 2px no-repeat white; color: black; border: 1px solid #666;\">"; // Style to format the wikipediaPreview.
		const string STYLE_END = "</p>";
		
		public bool SupportsMessage(string bodyHtml, Message message)
		{
			return Regex.IsMatch(bodyHtml, WIKIPEDIA_PAGE_LINK_PATTERN); // If the message (incoming or outcoming) contains one or more wikipedia-urls it returns true
		}

		public string FormatMessage(string bodyHtml, Message message)
		{
			return Regex.Replace(bodyHtml, WIKIPEDIA_PAGE_LINK_PATTERN, delegate (Match match) {
				string linkUrl = match.Groups[2].Value;
				return match.Value + BuildHtmlPreview(linkUrl);
			}, RegexOptions.Singleline);
		}

		public bool StopAfter {
			get {
				return false;
			}
		}

		string BuildHtmlPreview(string linkUrl) // Create the WikipediaPreview
		{
			try { // Connect to Wikipedia and read the Article
				Uri url = new Uri(linkUrl);
				HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
				request.UserAgent = "Mozilla/5.0"; // Seriously wikipedia?
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
						string article = match.Groups[1].ToString(); // Get the first passage from the article
						article = Synapse.Core.HtmlSanitizer.ToPlainText(article); // Remove HTML
						article = Regex.Replace(article, @"\[\d*?\]", ""); // Remove source-tags

						if(article.Length > 160) // More than 160chars is too long.
						{
							article = String.Format("{0}...", article.Remove(160));
						}
						
						// FIXME:
						// article = article.Replace(articleName, "<b>" + articleName + "</b>");
						
						return String.Format("{0}{1}{2}", STYLE_BEGIN, article, STYLE_END); // Format the message with html/css and return.
					}
				}
			} catch (Exception ex) {
				Console.Error.WriteLine("WikipediaMessageDisplayFormatter.BuildHtmlPreview(string linkUrl) has an error (Maybe The Article doesn't exist?)");
				Console.Error.WriteLine(ex);
			}
			return String.Empty; // No Preview
		}
	}
}
