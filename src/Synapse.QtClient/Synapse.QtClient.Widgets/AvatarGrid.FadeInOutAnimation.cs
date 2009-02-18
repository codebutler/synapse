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
			
			IFadableItem m_Item;
			
			public FadeInOutAnimation () : base ()
			{
			}
			
			public FadeInOutAnimation (QObject parent) : base (parent)
			{
			}

			public new void SetItem (QGraphicsItem item)
			{			
				base.SetItem(item);
				m_Item = (IFadableItem)item;
			}
			
			public bool FadeIn {
				get {
					return m_FadeIn;
				}
				set {
					m_FadeIn = value;
					if (m_Item != null) {
						m_Item.Opacity = m_FadeIn ? 0 : 1;
						m_Item.SetVisible(m_FadeIn);
						m_Item.Update();
					}
				}
			}
			
			protected override void AfterAnimationStep (double step)
			{
				base.AfterAnimationStep (step);
				
				if (m_Item != null) {
					var opacity = m_FadeIn ? step : 1 - step;

					m_Item.Opacity = opacity;
				
					if (step == 1 && !m_FadeIn) {
						m_Item.SetVisible(false);
					}
				}
			}
		}

		interface IFadableItem : IQGraphicsItem
		{
			double Opacity {
				get;
				set;
			}
		}
	}
}
