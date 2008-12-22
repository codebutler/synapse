//
// AvatarManager.cs
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
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Drawing;
using System.Reflection;
using Synapse.Core;
using Synapse.ServiceStack;
using jabber;
using jabber.protocol.client;
using jabber.protocol.iq;

namespace Synapse.Xmpp
{
	public class AvatarManager
	{
		static string s_AvatarPath;
		
		// Bare Jid => Hash
		static Dictionary<string, string> s_HashCache = new Dictionary<string, string>();

		// Hash => Native Image Object
		static Dictionary<string, object> s_AvatarCache = new Dictionary<string, object>();

		static object s_DefaultAvatarImage;
		
		Account m_Account;
		
		static AvatarManager ()
		{
			s_AvatarPath = Path.Combine(Paths.ApplicationData, "avatars");
			if (!Directory.Exists(s_AvatarPath))
				Directory.CreateDirectory(s_AvatarPath);

			s_DefaultAvatarImage = Application.CreateImage("resource:/default-avatar.png");
			if (s_DefaultAvatarImage == null)
				throw new Exception("Unable to load default avatar!");

			s_AvatarCache.Add("octy", Application.CreateImage("resource:/octy-32.png"));
		}
		
		public AvatarManager(Account account)
		{
			account.Client.OnPresence += HandleOnPresence;
			m_Account = account;	
		}

		public static string GetAvatarHash (JID jid)
		{
			if (jid != null) {
				lock (s_HashCache) {
					string bare = jid.Bare;
					if (s_HashCache.ContainsKey(bare)) {
						return s_HashCache[bare];
					}
				}
			}
			return "default";
		}

		public static object GetAvatar (JID jid)
		{
			string hash = GetAvatarHash(jid);
			return GetAvatar(hash);
		}

		public static object GetAvatar (string hash)
		{
			lock (s_AvatarCache) {
				if (hash != null) {
					if (s_AvatarCache.ContainsKey(hash)) {
						return s_AvatarCache[hash];
					} else {
						if (AvatarExists(hash)) {
							object obj = Application.CreateImage(AvatarFileName(hash));
							s_AvatarCache[hash] = obj;
							return obj;
						}
					}
				}
			}
			return s_DefaultAvatarImage;
		}
		
		void HandleOnPresence(object sender, Presence pres)
		{
			foreach (XmlElement x in pres.GetElementsByTagName("x")) {
				if (x.Attributes["xmlns"] != null && x.Attributes["xmlns"].Value == "vcard-temp:x:update") {
					var photos = x.GetElementsByTagName("photo");
					if (photos.Count > 0) {
						string bareJid = pres.From.Bare;
						string hash = photos[0].InnerText;
						lock (s_HashCache) {
							if (!s_HashCache.ContainsKey(bareJid) || s_HashCache[bareJid] != hash) {
								s_HashCache[bareJid] = hash;
								if (!AvatarExists(hash))
									UpdateAvatar(bareJid, hash);
							}
						}						
						break;
					}
				}
			}
		}

		static bool AvatarExists(string hash)
		{
			return File.Exists(AvatarFileName(hash));
		}

		static string AvatarFileName(string hash)
		{
			return Path.Combine(s_AvatarPath, String.Format("{0}.png", hash));
		}
		
		void UpdateAvatar(string jid, string expectedHash)
		{
			// XXX: Abstract this into a VCardManager
			VCardIQ iq = new VCardIQ(m_Account.Client.Document);
			iq.Type = IQType.get;
			iq.To = jid;
			iq.AddChild(new VCard(m_Account.Client.Document));
			
			m_Account.Client.Tracker.BeginIQ(iq, HandleReceivedAvatar, expectedHash); 
		}

		void HandleReceivedAvatar (object o, IQ i, object data)
		{
			string expectedHash = (string)data;

			VCard vcard = (VCard)i.FirstChild;
			if (vcard.Photo != null) {
				byte[] imageData = vcard.Photo.BinVal;
				string hash = Util.SHA1(imageData);

				if (hash == expectedHash) {
					vcard.Photo.Image.Save(AvatarFileName(hash), System.Drawing.Imaging.ImageFormat.Png);
				}
			}
		}
	}
}