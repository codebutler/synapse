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
		const string WIKIPEDIA_PAGE_LINK_PATTERN = @"<a href=""http://(www\.)?([a-zA-Z]{2})\.wikipedia\.org/wiki/(.*?)""";	// Pattern to see if a string contains a wikipedia-url	

                const string STYLE_BEGIN = "<p style=\"background-color:white; color:black; border-width:1px; border-style:solid;\">"; // Style to format the wikipediaPreview.
                const string STYLE_END = "</p>";
		
		public bool SupportsMessage(string bodyHtml, Message message)
		{
			return Regex.IsMatch(bodyHtml, WIKIPEDIA_PAGE_LINK_PATTERN); // If the message (incoming or outcoming) contains one or more wikipedia-urls it returns true
		}

		public string FormatMessage(string bodyHtml, Message message)
		{
                        string chatMessage = bodyHtml;
                        MatchCollection matchCollection = Regex.Matches(bodyHtml, WIKIPEDIA_PAGE_LINK_PATTERN); // matchCollection contains (ia) the Wikipedia-Locations and the names of the articles

                        foreach(Match match in matchCollection) // Maybe more than one article was posted in a message
                        {
                                string linkUrl = String.Format("http://{0}.wikipedia.org/wiki/{1}",  match.Groups[2], match.Groups[3]); // Groups[2] = Location , Groups[3] = Article
                                chatMessage += BuildHtmlPreview(linkUrl); // Add the WikipediaPreview to the chatMessage                                
                        }
                        return chatMessage; // This will be displayed in the ChatWindow
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

						article = Regex.Replace(article, "(<.*?>)", delegate(Match tagmatch) { // Remove html-tags (except for <b> and </b>)
                                                          string tag = tagmatch.Groups[1].ToString();
                                                          if(tag.CompareTo("<b>") == 0 | tag.CompareTo("</b>") == 0)
                                                                  return tag;
                                                          else
                                                                  return "";
                                                        }); 
                                                article = Regex.Replace(article, @"\[\d*?\]", ""); // Remove source-tags

                                                if(article.Length > 160) // More than 160chars is too long.
                                                {
                                                        article = String.Format("{0}[...]", article.Remove(160));
                                                }
                                                return String.Format("{0}{1}{2}", STYLE_BEGIN, article, STYLE_END); // Format the message with html/css and return.
					}
				}
			} catch {
				Console.Error.WriteLine("WikipediaMessageDisplayFormatter.BuildHtmlPreview(string linkUrl) has an error (Maybe The Article doesn't exist?)");
			}
			return String.Empty; // No Preview
		}
	}
}
