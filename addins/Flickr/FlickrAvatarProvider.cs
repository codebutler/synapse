//
// Main.cs
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
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using System.Collections.Generic;
using Synapse.UI;

namespace Synapse.Addins.Flickr
{
	public class FlickrAvatarProvider : IAvatarProvider
	{
		static readonly string FLICKR_API_KEY           = "a31c83238d37147b4df54c7e117a8add";
		static readonly string FLICKR_PHOTOS_SEARCH_URL = "http://api.flickr.com/services/rest/?method=flickr.photos.search";

		public string Name {
			get {
				return "Flickr";
			}
		}
		
		public void BeginGetAvatars (string searchText, AvatarCallback callback)
		{
			Uri url = new Uri(FLICKR_PHOTOS_SEARCH_URL + "&sort=relevance&api_key=" + FLICKR_API_KEY + "&text=" + HttpUtility.UrlEncode(searchText));
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
			request.BeginGetResponse(delegate (IAsyncResult result) {
				var response = request.EndGetResponse(result);
				string xml = null;
				using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
					xml = reader.ReadToEnd();					
				}

				var doc = new XmlDocument();
				doc.LoadXml(xml);


				List<AvatarInfo> infos = new List<AvatarInfo>();
				
				foreach (XmlElement element in doc.SelectNodes("/rsp/photos/photo")) {
					string photoTitle = element.GetAttribute("title");


					string photoThumbnailUrl = String.Format("http://farm{0}.static.flickr.com/{1}/{2}_{3}_s.jpg",
					                                         element.GetAttribute("farm"), 
					                                         element.GetAttribute("server"),
					                                         element.GetAttribute("id"),
					                                         element.GetAttribute("secret"));
					
					string photoUrl = String.Format("http://farm{0}.static.flickr.com/{1}/{2}_{3}_t.jpg",
					                                element.GetAttribute("farm"),
					                                element.GetAttribute("server"),
					                                element.GetAttribute("id"),
					                                element.GetAttribute("secret"));
					
					infos.Add(new AvatarInfo(photoTitle, photoThumbnailUrl, photoUrl));
				}

				callback(this, infos.ToArray());
			}, null);
		}
	}
}
