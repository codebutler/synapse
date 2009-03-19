//
// HtmlSanitizer.cs
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace Synapse.Core
{	
	public static class HtmlSanitizer 
	{
		static readonly Dictionary<string, string[]> s_WhiteList = new Dictionary<string, string[]> {
			{ "abbr",       null },
			{ "acronym",    null },
			{ "address",    null },
			{ "blockquote", null },
			{ "br",         null },
			{ "cite",       null },
			{ "code",       null },
			{ "dfn",        null },
			{ "div",        null },
			{ "em",         null },
			{ "h1",         null },
			{ "h2",         null },
			{ "h3",         null },
			{ "h4",         null },
			{ "h5",         null },
			{ "h6",         null },
			{ "kbd",        null },
			{ "p",          null },
			{ "pre",        null },
			{ "q",          null },
			{ "samp",       null },
			{ "span",       null },
			{ "strong",     null },
			{ "var",        null },
			
			{ "a",          new [] { "href", "title" } },
			
			{ "dl",         null },
			{ "dt",         null },
			{ "dd",         null },
			{ "ol",         null },
			{ "ul",         null },
			{ "li",         null },
			
			{ "img",        new [] { "width", "height", "src" } },

			{ "b",          null },
			{ "i",          null },
			{ "u",          null },
			{ "small",      null },
			{ "big",        null },
			{ "hr",         null },
			{ "strike",     null },
			{ "sup",        null },
			{ "sub",        null },
			{ "s",          null }
		};
		
		public static string ToPlainText (string html)
		{
			var builder = new StringBuilder();
			
			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			
			foreach (var node in doc.DocumentNode.ChildNodes)
				ExtractPlainText(node, builder);
			
			return builder.ToString();
		}
		
		static void ExtractPlainText (HtmlNode node, StringBuilder builder)
		{
			if (node is HtmlTextNode) {
				builder.Append(node.InnerText);
			} else if (!(node is HtmlCommentNode)) {
				foreach (var childNode in node.ChildNodes)
					ExtractPlainText(childNode, builder);
			}
		}
		
		public static string Sanitize (string html, bool linkify)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			
			var builder = new StringBuilder();
			
			foreach (var node in doc.DocumentNode.ChildNodes)
				SanitizeNode(node, builder, linkify);
			
			return builder.ToString();
		}
		
		static void SanitizeNode (HtmlNode node, StringBuilder builder, bool linkify)
		{
			if (node is HtmlTextNode) {
				if (linkify && HasParent(node, "a") == false)
					builder.Append(Util.Linkify(node.InnerText));
				else
					builder.Append(node.InnerText);
			} else if ((node is HtmlCommentNode) == false) {
				string name = node.Name.ToLower();
				if (s_WhiteList.ContainsKey(name)) {
					var attributeString = BuildAttributeString(node);
					if (String.IsNullOrEmpty(node.InnerHtml)) {
						builder.AppendFormat("<{0}{1}/>", name, attributeString);
					} else {
						builder.AppendFormat("<{0}{1}>", name, attributeString);

						foreach (var childNode in node.ChildNodes)
							SanitizeNode(childNode, builder, linkify);
												
						builder.AppendFormat("</{0}>", name);
					}
				} else {
					foreach (var childNode in node.ChildNodes)
						SanitizeNode(childNode, builder, linkify);
				}
			}
		}
		
		static string BuildAttributeString (HtmlNode node)
		{
			string name = node.Name.ToLower();

			if (name == "a") {
				node.SetAttributeValue("href", SanitizeAHref(node.GetAttributeValue("href", String.Empty)));

				// Don't allow custom titles (tooltips), and make sure one is always set.
				node.SetAttributeValue("title", node.GetAttributeValue("href", String.Empty));
				
				Console.WriteLine("SET ATTRIBUTE VALUE !! " + node.GetAttributeValue("title", "frak"));
			}
			
			if (name == "img") {
				node.SetAttributeValue("src", SanitizeImgSrc(node.GetAttributeValue("src", String.Empty)));
				
				// FIXME: 
				
				// Show something if the image fails to load for some reason.
				//node.SetAttributeValue("alt", "[IMAGE]");
				
				// Force width
				//node.SetAttributeValue("width", "100%");
				
				// Remove height
				//node.SetAttributeValue("height", String.Empty);
			}

			var attributeStringBuilder = new StringBuilder();
			foreach (var attr in node.Attributes) {
				string attrName = attr.Name.ToLower();
				string attrVal = attr.Value;
				Console.WriteLine(attrName);
				if (s_WhiteList[name] != null && s_WhiteList[name].Contains(attrName)) {
					if (attributeStringBuilder.Length > 0)
						attributeStringBuilder.Append(" ");
					attributeStringBuilder.AppendFormat("{0}=\"{1}\"", attrName, attrVal);
				} else if (attrName == "style") {
					// FIXME: Special handling for css					
				}
			}
			if (attributeStringBuilder.Length > 0)
				attributeStringBuilder.Insert(0, " ");
			return attributeStringBuilder.ToString();
		}
		
		static string SanitizeAHref (string href)
		{
			// FIXME: Allow xmpp: URIs.
			var pattern = @"(https?|ftp)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+";
			if (Regex.IsMatch(href, pattern))
				return href;
			else
				return String.Empty; // FIXME: Might want to remove the <a> tag entirelly...
		}

		static string SanitizeImgSrc (string src)
		{
			var pattern = @"data:image/png;base64,[0-9a-zA-Z\+/=]+|https?://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+";
			if (Regex.IsMatch(src, pattern))
				return src;
			else
				return String.Empty; // FIXME: Show standard browser "missing" image?
		}
		
		static bool HasParent (HtmlNode node, string parentName)
		{
			HtmlNode parent = node;
			while ((parent = parent.ParentNode) != null) {
				if (parent.Name.ToLower() == parentName.ToLower())
					return true;
			}
			return false;
		}
	}
}
