//
// IAvatarProvider.cs
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

namespace Synapse.UI
{
	public delegate void AvatarCallback (object sender, AvatarInfo[] avatars);
	
	public interface IAvatarProvider
	{
		string Name {
			get;
		}
		
		void BeginGetAvatars (string searchText, AvatarCallback callback);
	}

	public class AvatarInfo
	{
		string m_Title;
		string m_ThumbnailUrl;
		string m_Url;
		
		public AvatarInfo (string title, string thumbnailUrl, string url)
		{
			m_Title        = title;
			m_ThumbnailUrl = thumbnailUrl;
			m_Url          = url;
		}

		public string Title {
			get {
				return m_Title;
			}		
		}

		public string ThumbnailUrl {
			get {
				return m_ThumbnailUrl;
			}
		}

		public string Url {
			get {
				return m_Url;
			}
		}
	}
}
