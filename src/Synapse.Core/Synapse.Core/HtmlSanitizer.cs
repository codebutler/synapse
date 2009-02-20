//
// HtmlSanitizer.cs
//
// Code from http://refactormycode.com/codes/333-sanitize-html

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Synapse.Core
{
	public static class HtmlSanitizer 
	{
		private static SpecialTag[] _specialTags;
		private static Regex _tags = new Regex("<[^>]*(>|$)", RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		private static Regex _whitelist = new Regex(@"
			^</?(a|b(lockquote)?|code|em|h(1|2|3)|i|li|ol|p(re)?|s(ub|up|trong|trike)?|ul)>$
			|^<(b|h)r\s?/?>$
			|^<a[^>]+>$
			|^<img[^>]+/?>$",
			RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace |
			RegexOptions.ExplicitCapture | RegexOptions.Compiled);

		static HtmlSanitizer ()
		{
			_specialTags = new [] {
				// FIXME: allow xmpp: uris!
				new SpecialTag("a", "href", @"href\s*=\s*""(https?|ftp)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+""",
				       "style", "id", "target", "class", "type", "title", "tabindex", "name"),

				new SpecialTag("img", "src", @"src\s*=\s*""(data:image/png;base64,[0-9a-zA-Z\+/=]+|https?://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+)""",
				 	"hspace", "height", "border", "align", "width", "vspace", "style", "class", "longdesc", "title", "id", "alt")
			};
		}
		
		/// <summary>
		/// sanitize any potentially dangerous tags from the provided raw HTML input using 
		/// a whitelist based approach, leaving the "safe" HTML tags
		/// </summary>
		public static string Sanitize(string html)
		{
			var tags = _tags.Matches(html);
		
			// iterate through all HTML tags in the input
			for (int i = tags.Count-1; i > -1; i--)
			{
				var tag = tags[i];
				var tagname = tag.Value.ToLower();
		
				if (!_whitelist.IsMatch(tagname))
				{
					// Not on our whitelist? I SAY GOOD DAY TO YOU, SIR. GOOD DAY!
					html = html.Remove(tag.Index, tag.Length);
					continue;
				}

				foreach (SpecialTag specialTag in _specialTags)
				{
					if (tagname.StartsWith("<" + specialTag.Tag))
					{
						html = specialTag.Parse(html, tag);
						break;
					}
				}
			}
		
			return html;
		}
				
		/// <summary>
		/// Utility function to match a regex pattern: case, whitespace, and line insensitive
		/// </summary>
		private static bool IsMatch(string s, string pattern)
		{
			return Regex.IsMatch(s, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase |
				RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
		}		

		class SpecialTag
		{
			private string _tag;
			private string _requiredAttribute;
			private List<string> _whitelist;
			private Regex _regex;
			private string _requiredAttribRegex;

			public string Tag
			{
				get { return _tag; }
			}

			public SpecialTag(string tag, string requiredAttribute, string requiredAttribRegex, params string[] list)
			{
				_tag = tag;
				_requiredAttribute = requiredAttribute;
				_requiredAttribRegex = requiredAttribRegex;
				_whitelist = new List<string>(list);

				// make sure the main required attribute is included in the whitelist
				if (!_whitelist.Contains(_requiredAttribute))
					_whitelist.Add(_requiredAttribute);

				_regex = new Regex(@"
					<" + _tag + @"
					(?<attrib>\s
						(?<name>[a-z]+)
						\s*=\s*
						""(?<value>[^""]*)""
					)+\s?/?>",
					RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
			}

			public string Parse(string html, Match tag)
			{
				MatchCollection attribs = _regex.Matches(tag.Value.ToLower());
				// verify general structure
				if (attribs.Count == 0)
				{
					html = html.Remove(tag.Index, tag.Length);
					return html;
				}

				// there will only be 1 capture
				GroupCollection attribGroups = attribs[0].Groups;

				// handle required attribute (href, img) first
				int hrefIndex = -1;
				for (int j = 0; j < attribGroups["name"].Captures.Count; j++)
				{
					if (attribGroups["name"].Captures[j].Value == _requiredAttribute)
					{
						hrefIndex = j;
						break;
					}
				}
				// if no required attribute or if it doesn't match the regex, kill the whole tag
				if (hrefIndex < 0 || !HtmlSanitizer.IsMatch(attribGroups["attrib"].Captures[hrefIndex].Value.Trim(), _requiredAttribRegex))
				{
					html = html.Remove(tag.Index, tag.Length);
					return html;
				}

				for (int k = attribGroups["attrib"].Captures.Count - 1; k > -1; k--)
				{
					Capture c = attribGroups["attrib"].Captures[k];

					string attrib = c.Value;
					string aName = attribGroups["name"].Captures[k].Value;
					string aValue = attribGroups["value"].Captures[k].Value;

					if (!_whitelist.Contains(aName))
						html = html.Remove(tag.Index + c.Index, c.Length);
				}

				return html;
			}
		}
	}
}
