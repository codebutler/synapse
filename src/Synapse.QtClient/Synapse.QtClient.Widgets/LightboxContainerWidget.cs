//
// LightboxContainerWidget.cs
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
using Qyoto;

namespace Synapse.QtClient.Widgets
{	
	public class LightboxContainerWidget : QStackedWidget
	{
		QWidget m_LightboxWidget;
		QWidget m_LightboxChild;
		
		public LightboxContainerWidget(QWidget parent) : base (parent)
		{
			m_LightboxWidget = new QWidget(this);
			m_LightboxWidget.ObjectName = "lightboxWidget";
			new QVBoxLayout(m_LightboxWidget);
			base.AddWidget(m_LightboxWidget);

			((QStackedLayout)base.Layout()).stackingMode = QStackedLayout.StackingMode.StackAll;
			m_LightboxWidget.Hide();
		}
		
		public void ShowLightbox (QWidget widget)
		{
			if (m_LightboxChild != null)
				throw new InvalidOperationException("Lightbox is already visible");

			var layout = (QBoxLayout)m_LightboxWidget.Layout();
			m_LightboxChild = widget;
			widget.SetParent(m_LightboxWidget);
			layout.AddWidget(widget);
			widget.Show();

			base.Widget(1).Enabled = false;
			
			m_LightboxWidget.Show();
			base.CurrentIndex = 0;
		}
		
		public void HideLightbox ()
		{
			if (m_LightboxChild == null)
				throw new InvalidOperationException("Lightbox is already hidden");
		
			var layout = (QBoxLayout)m_LightboxWidget.Layout();
			layout.RemoveWidget(m_LightboxChild);
			
			m_LightboxChild.SetParent(null);
			m_LightboxChild.Dispose();
			m_LightboxChild = null;
			
			base.Widget(1).Enabled = true;
			
			m_LightboxWidget.Hide();
			base.CurrentIndex = 1;
		}
	}
}
