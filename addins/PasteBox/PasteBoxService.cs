//
// PasteBoxService.cs
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
using System.Text;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.UI.Chat;
using Wilco.SyntaxHighlighting;

namespace Synapse.Addins.PasteBox
{	
	public class PasteBoxService : IExtensionService
	{
		public static readonly string CODE_NS = "http://synapse.im/protocol/code";
		
		public void Initialize ()
		{
			var htmlParser = new HtmlParser();
			foreach (var highlighter in Highlighters)
				highlighter.Parser = htmlParser;			
		}

		public IEnumerable<HighlighterBase> Highlighters {
			get {
				foreach (var highlighter in Register.Instance.Highlighters) {
					yield return (HighlighterBase)highlighter;
				}
			}
		}
		
		public string GeneratePreview (string language, string text)
		{
			var builder = new StringBuilder();
			builder.Append("<html><head>");
			builder.Append(Util.ReadResource("ConversationHeader.html"));
			builder.Append("</head><body>");
			builder.Append(FormatAsHtml(language, text));
			builder.Append("</body></html>");
			return builder.ToString();
		}

		public string FormatMessageBody (XmlElement bodyElement)
		{
			string language = bodyElement.GetAttribute("language", CODE_NS);
			return FormatAsHtml(language, bodyElement.InnerText);
		}

		public void SendMessage (IChatHandler handler, string language, string text)
		{			
			var bodyElement = handler.Account.Client.Document.CreateElement("body");
			bodyElement.SetAttribute("xmlns:code", CODE_NS);
			bodyElement.SetAttribute("language", CODE_NS, language);
			bodyElement.InnerText = text;

			handler.Send(bodyElement);
		}

		public string ServiceName {
			get {
				return "PasteBoxService";
			}
		}

		public void Dispose ()
		{

		}
		
		string FormatAsHtml (string language, string text)
		{
			if (String.IsNullOrEmpty("language"))
				throw new ArgumentNullException("language");
			
			var highlighter = this.Highlighters.FirstOrDefault(h => h.Name == language);
			if (highlighter == null) {
				Console.WriteLine("Unsupported language: " + language);
				return null;
			}
				
			var builder = new StringBuilder();
			builder.AppendFormat("<div class=\"code\">");
			
			text = highlighter.Parse(text);
			text = text.Replace("  ", " &nbsp;");
			text = text.Replace("\t", " &nbsp;&nbsp;&nbsp;");
			text = text.Replace("\r\n", "<br/>");
			text = text.Replace("\n", "<br/>");
			builder.Append(text);
			
			builder.AppendFormat("</div>");
			return builder.ToString();
		}
	}
}
