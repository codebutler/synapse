//
// StubbornWebView.cs: A QWebView that won't change location.
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
	public class StubbornWebView : QWebView
	{		
		public StubbornWebView (QWidget parent) : base (parent)
		{
			base.ContextMenuPolicy = ContextMenuPolicy.NoContextMenu;
			base.SetPage(new StubbornWebPage(this));
		}
		
		protected override void KeyPressEvent (Qyoto.QKeyEvent arg1)
		{
			arg1.Ignore();
		}
		
		protected override void DragEnterEvent (Qyoto.QDragEnterEvent arg1)
		{
			arg1.Ignore();
		}
	}
	
	public class StubbornWebPage : QWebPage		
	{
		public StubbornWebPage (QObject parent) : base (parent)
		{
		}
				
		protected override bool AcceptNavigationRequest (Qyoto.QWebFrame frame, Qyoto.QNetworkRequest request, Qyoto.QWebPage.NavigationType type)
		{
			if (request.Url().Scheme() != "resource")
				return false;
			else
				return base.AcceptNavigationRequest(frame, request, type);
		}
	}
}
