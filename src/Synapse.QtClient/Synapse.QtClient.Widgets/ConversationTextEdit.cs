// 
// ConversationTextEdit.cs
//  
// Copyright (C) 2009 Eric Butler
// 
// Authors:
//   Eric Butler <eric@extremeboredom.net>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Text;

using Synapse.Core;

using Qyoto;

namespace Synapse.QtClient.Widgets
{
	public class ConversationTextEdit : QTextEdit
	{
		public ConversationTextEdit(QWidget parent) : base (parent)
		{
		}
		
		public new string ToHtml ()
		{
			var builder = new StringBuilder();
			
			builder.Append("<body xmlns='http://www.w3.org/1999/xhtml'>");
			
			var document = base.Document();
			
			var block = document.Begin();
			while (block.IsValid()) {			
				builder.Append("<p>");
				
				/*			
				QTextBlock.iterator it;
				for (it = block.Begin(); !it.AtEnd(); it = it.Next()) {
					QTextFragment fragment = it.Fragment();
					
				}			
				*/
				
				builder.Append("</b>");
				
				block = block.Next();
			}
			
			builder.Append("</body>");
			
			return builder.ToString();
		}
		
		protected override bool CanInsertFromMimeData (Qyoto.QMimeData source)
		{
			if (source.HasImage())
				return true;
			else
				return base.CanInsertFromMimeData (source);
		}
		
		protected override void InsertFromMimeData (Qyoto.QMimeData source)
		{
			var cursor = base.TextCursor();
			
			if (source.HasImage()) {
				var image = (QImage)source.ImageData();
				var document = base.Document();
				var imageName = Guid.NewGuid().ToString();
				document.AddResource((int)QTextDocument.ResourceType.ImageResource, new QUrl(imageName), image);
				cursor.InsertImage(imageName);
			} else if (source.HasUrls()) {				
				var magic = new Magic(true);				
				foreach (var url in source.Urls()) {
					if (url.Scheme() == "file") {
						string fileName = url.Path();
						if (File.Exists(fileName)) {
							string mimeType = magic.Lookup(url.Path());
							if (mimeType.StartsWith("image/")) {
								cursor.InsertHtml(String.Format("<img src=\"{0}\" />", fileName));
							} else {
								// FIXME: Generate and insert an image representing a file.
								Console.WriteLine("File Transfer: " + fileName);
							}
						} else {
							// FIXME: Support "sending" directories?
							cursor.InsertText(url.ToString());
						}
					} else if (url.Scheme() == "http" || url.Scheme() == "https") {
						cursor.InsertHtml(String.Format("<a href=\"{0}\">{0}</a>", url.ToString()));
					} else {
						cursor.InsertText(url.ToString());
					}
				}
			} else {
				base.InsertFromMimeData(source);
			}
		}
	}
}
