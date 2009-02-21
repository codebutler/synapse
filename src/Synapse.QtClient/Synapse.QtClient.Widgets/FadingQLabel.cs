//
// FadingQLabel.cs
// 
// Copyright (C) 2008 Eric Butler
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
using Qyoto;

namespace Synapse.QtClient.Widgets
{
	public class FadingQLabel : QLabel
	{
		public FadingQLabel (QWidget parent) : base (parent)
		{
		}
		
		protected override void PaintEvent (Qyoto.QPaintEvent arg1)
		{
			using (QPainter p = new QPainter(this)) {
				var rect = ContentsRect();
				//var fm = new QFontMetrics(Font);
				
				//if (fm.Width(Text) > rect.Width()) {
					var gradient = new QLinearGradient(rect.TopLeft(), rect.TopRight());
					gradient.SetColorAt(0.8, Palette.Color(QPalette.ColorRole.WindowText));
					gradient.SetColorAt(1.0, new QColor(Qt.GlobalColor.transparent));
					var pen = new QPen();
					pen.SetBrush(new QBrush(gradient));
					p.SetPen(pen);
				//}
				
				p.DrawText(rect, (int)Qt.TextFlag.TextSingleLine, Text);
			}
		}

		public override QSize MinimumSizeHint ()
		{
			var s = base.SizeHint();
			s.SetWidth(-1);
			return s;
		}

		public override QSize SizeHint ()
		{
			var s = base.SizeHint();
			s.SetWidth(-1);
			return s;
		}
	}
}
