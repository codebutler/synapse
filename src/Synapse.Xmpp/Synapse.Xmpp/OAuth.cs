//
// OAuth.cs
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
using System.Xml;

using jabber.protocol;

using OAuth;

namespace Synapse.Xmpp
{	
	public class OAuth : Element
	{
		public OAuth (Consumer consumer, AccessToken token, XmlDocument doc) 
			: base ("oauth", Namespace.OAuth, doc)
		{
			this.ConsumerKey     = consumer.Key;
			this.Token           = token.Token;
			this.Version         = consumer.OAuthVersion;
			this.Nonce           = global::OAuth.Helper.GenerateNonce();
			this.Timestamp       = global::OAuth.Helper.GenerateTimestamp();
			this.SignatureMethod = consumer.SignatureMethod;
		}
		
		public string ConsumerKey {
			get {
				return GetElem("oauth_consumer_key");
			}
			set {
				SetElem("oauth_consumer_key", value);
			}
		}
		
		public string Nonce {
			get {
				return GetElem("oauth_nonce");
			}
			set {
				SetElem("oauth_nonce", value);
			}
		}
		
		public string Signature {
			get {
				return GetElem("oauth_signature");
			}
			set {
				SetElem("oauth_signature", value);
			}
		}
		
		public string SignatureMethod {
			get {
				return GetElem("oauth_signature_method");
			}
			set {
				SetElem("oauth_signature_method", value);
			}
		}
		
		public string Timestamp {
			get {
				return GetElem("oauth_timestamp");
			}
			set {
				SetElem("oauth_timestamp", value);
			}
		}
		
		public string Token {
			get {
				return GetElem("oauth_token");
			}
			set {
				SetElem("oauth_token", value);
			}
		}
		
		public string Version {
			get {
				return GetElem("oauth_version");
			}
			set {
				SetElem("oauth_version", value);
			}
		}
	}
}
