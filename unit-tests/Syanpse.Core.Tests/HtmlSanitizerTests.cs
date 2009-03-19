//
// HtmlSanitizerTests.cs
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

using NUnit.Framework;

using Synapse.Core;

namespace Syanpse.Core
{	
	[TestFixture]
	public class HtmlSanitizerTests
	{
		[Test]
		public void Sanitize_Should_Remove_Script_Tags ()
		{
			string html = "<strong>hello!!</strong> <script type=\"text/javascript\">alert('pwnd!');</script> <em>world!</em>";
			html = HtmlSanitizer.Sanitize(html, false);
			Assert.AreEqual("<strong>hello!!</strong> alert('pwnd!'); <em>world!</em>", html);
		}
		
		[Test]
		public void Sanitize_Should_Add_Http_Links ()
		{
			string html = "hey check out www.google.com!";
			html = HtmlSanitizer.Sanitize(html, true);
			Assert.AreEqual("hey check out <a href=\"http://www.google.com\" title=\"http://www.google.com\">www.google.com</a>!", html);
		}
		
		[Test]
		public void Sanitize_Should_Add_Mailto_Links ()
		{
			string html = "my email is eric@extremeboredom.net";
			html = HtmlSanitizer.Sanitize(html, true);
			Assert.AreEqual("my email is <a href=\"mailto:eric@extremeboredom.net\" title=\"mailto:eric@extremeboredom.net\">eric@extremeboredom.net</a>!", html);
		}
		
		[Test]
		public void Sanitize_Should_Add_Title_To_Existing_Links ()
		{
			string html = "hey check out <a href=\"http://www.google.com\">www.google.com</a>!";
			html = HtmlSanitizer.Sanitize(html, true);
			Assert.AreEqual("hey check out <a href=\"http://www.google.com\" title=\"http://www.google.com\">www.google.com</a>!", html);
		}
		
		[Test]
		public void Sanitize_Should_Override_Existing_Title ()
		{
			string html = "hey check out <a href=\"http://www.google.com\" title=\"clickme!\">www.google.com</a>!";
			html = HtmlSanitizer.Sanitize(html, true);
			Assert.AreEqual("hey check out <a href=\"http://www.google.com\" title=\"http://www.google.com\">www.google.com</a>!", html);
		}
		
		[Test]
		public void Sanitize_Should_Remove_Bad_Attributes_From_Existing_Links ()
		{
			string html = "hey check out <a href=\"http://www.google.com\" evil=\"muahahaha\">www.google.com</a>!";
			html = HtmlSanitizer.Sanitize(html, true);
			Assert.AreEqual("hey check out <a href=\"http://www.google.com\" title=\"http://www.google.com\">www.google.com</a>!", html);
		}
		
		[Test]
		public void Sanitize_Should_Add_Href ()
		{
			string html = "hey check out <a>this</a>!";
			html = HtmlSanitizer.Sanitize(html, true);
			Assert.AreEqual("hey check out <a href=\"\" title=\"\">this</a>!", html);
		}
		
		[Test]
		public void Sanitize_Should_Remove_Bad_Img_Src ()
		{
			string html = "<img src=\"evil://foo\" />";
			html = HtmlSanitizer.Sanitize(html, true);
			Assert.AreEqual("<img src=\"\"/>", html);
		}
		
		[Test]
		public void Sanitize_Should_Not_Allow_Telnet_Urls ()
		{
			string html = "<a href=\"telnet://evildomain.com\">FREE KITTENS!</a>";
			html = HtmlSanitizer.Sanitize(html, true);
			Assert.AreEqual("<a href=\"\" title=\"\">FREE KITTENS!</a>", html);
		}
		
		[Test]
		public void Sanitize_Should_Remove_Links_Without_Scheme ()
		{
			string html = "hey check out <a href=\"www.google.com\">www.google.com</a>!";
			html = HtmlSanitizer.Sanitize(html, true);
			Assert.AreEqual("hey check out <a href=\"\" title=\"\">www.google.com</a>!", html);
		}
		
		[Test]
		public void ToPlainText_Should_Strip_Html ()
		{
			string html = "<b>Hello</b> world!";
			string plain = HtmlSanitizer.ToPlainText(html);
			Assert.AreEqual("Hello world!", plain);
		}
	}
}
