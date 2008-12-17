//
// AvatarGrid.FadeInOutAnimation.cs
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
	public partial class AvatarGrid<T> : QGraphicsView
	{		
		class FadeInOutAnimation : QGraphicsItemAnimation
		{
			bool m_FadeIn;
			
			public FadeInOutAnimation (bool fadeIn, QObject parent) : base (parent)
			{
				m_FadeIn = fadeIn;
			}

			public new void SetItem (QGraphicsItem item)
			{
				var fadeItem = (IFadeable)item;
				if (!fadeItem.IsVisible() && m_FadeIn) {
					fadeItem.Opacity = 0;
					fadeItem.SetVisible(true);
					fadeItem.Update();
				}
				
				base.SetItem(item);
			}
			
			protected override void AfterAnimationStep (double step)
			{
				base.AfterAnimationStep (step);
				
				var opacity = m_FadeIn ? step : 1 - step;

				var fadeItem = (IFadeable)base.Item();
				fadeItem.Opacity = opacity;
				if (step == 1 && !m_FadeIn) {
					base.Item().SetVisible(false);
				}
			}
		}

		interface IFadeable : IQGraphicsItem
		{
			double Opacity {
				get;
				set;
			}
			
		}
	}
}
