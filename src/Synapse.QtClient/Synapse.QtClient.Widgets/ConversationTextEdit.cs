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
			
			var document = base.Document();
			
			bool isFirstBlock = true;
			var block = document.Begin();
			while (block.IsValid()) {			
				if (!isFirstBlock)
					builder.Append("<br/>");
				isFirstBlock = false;
				
				QTextBlock.iterator it;
				for (it = block.Begin(); !it.AtEnd(); it = it++) {
					var fragment = it.Fragment();
					var format = fragment.CharFormat();
					if (format.IsImageFormat()) {					
						var imageFormat = format.ToImageFormat();
						var name = imageFormat.Name();
						var data = document.Resource((int)QTextDocument.ResourceType.ImageResource, new QUrl(name));
						if (data.type() == QVariant.TypeOf.Pixmap) {
							var pixmap = (QPixmap)data;
							var tempArray = new QByteArray();
							var tempBuffer = new QBuffer(tempArray);
							pixmap.Save(tempBuffer, "PNG");
						
							string imageString = tempArray.ToBase64().ConstData();
							builder.AppendFormat("<img alt=\"[embeded image]\" src=\"data:image/png;base64,{0}\" />", imageString);
						}						
					} else {
						var link = format.AnchorHref();
						var bold = (format.FontWeight() == (int)QFont.Weight.Bold);
						var underline = format.FontUnderline();
						var italic = format.FontItalic();
						var strike = format.FontStrikeOut();
						
						if (!String.IsNullOrEmpty(link))
							builder.AppendFormat("<a href=\"{0}\" title=\"{0}\">", link);

						if (bold) builder.Append("<b>");						
						if (underline) builder.Append("<u>");
						if (italic) builder.Append("<i>");
						if (strike) builder.Append("<s>");
							
						var text = fragment.Text();

						text = text.Replace("  ", "&#160;");
						text = text.Replace("\t", " &#160;&#160;&#160;");
						text = text.Replace("\r\n", "<br/>");
						text = text.Replace("\r", "<br/>");
						text = text.Replace("\n", "<br/>");
						text = text.Replace("\u2028", "<br/>");

						builder.Append(text);
						
						if (bold) builder.Append("</b>");
						if (underline) builder.Append("</u>");
						if (italic) builder.Append("</i>");
						if (strike) builder.Append("</s>");

						if (!String.IsNullOrEmpty(link))
							builder.Append("</a>");
					}
				}			
				
				block = block.Next();
			}
			
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
				var image = QPixmap.FromImage((QImage)source.ImageData());
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
