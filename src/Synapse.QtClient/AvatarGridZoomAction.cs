//
// AvatarGridZoomAction.cs
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

using Synapse.QtClient.Widgets;

namespace Synapse.QtClient
{
	public class AvatarGridZoomAction<T> : QWidgetAction
	{
		AvatarGrid<T> m_Grid;
		QWidget m_SliderContainer;
		
		public AvatarGridZoomAction(AvatarGrid<T> grid) : base (grid)
		{
			m_Grid = grid;
			m_SliderContainer = new QWidget();
			m_SliderContainer.SetLayout(new QHBoxLayout());
			m_SliderContainer.Layout().AddWidget(new QLabel("Zoom:", m_SliderContainer));
			var zoomSlider = new QSlider(Orientation.Horizontal, m_SliderContainer);
			zoomSlider.Minimum = 16;
			zoomSlider.Maximum = 60;
			zoomSlider.Value = m_Grid.IconSize;
			QObject.Connect<int>(zoomSlider, Qt.SIGNAL("valueChanged(int)"), HandleZoomSliderValueChanged);
			m_SliderContainer.Layout().AddWidget(zoomSlider);
			
			base.SetDefaultWidget(m_SliderContainer);
		}
		
		void HandleZoomSliderValueChanged (int value)
		{
			m_Grid.IconSize = value;
		}
	}
}
