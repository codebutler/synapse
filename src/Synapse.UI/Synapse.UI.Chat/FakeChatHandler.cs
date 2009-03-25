//
// FakeChatHandler.cs
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

using jabber;
using jabber.protocol.client;

using Synapse.Xmpp;
using Synapse.Xmpp.Services;

namespace Synapse.UI.Chat
{
	public class FakeChatHandler : AbstractChatHandler
	{		
		JID m_OtherJid = new JID("BritneySpears14@example.com");
		
		public FakeChatHandler () : base (new Account(new AccountInfo("bloodninja", "example.com", "pass", "synapse")))
		{
		}
		
		public void Go ()
		{
			AppendMessage("Baby, I been havin a tough night so treat me nice aight?");
			AppendInMessage("Aight.");
			AppendMessage("Slip out of those pants baby, yeah.");
			AppendInMessage("I slip out of my pants, just for you, bloodninja.");
			AppendMessage("Oh yeah, aight. Aight, I put on my robe and wizard hat.");
			AppendInMessage("Oh, I like to play dress up.");
			AppendMessage("Me too baby.");
			AppendInMessage("I kiss you softly on your chest.");
			AppendMessage("I cast Lvl. 3 Eroticism. You turn into a real beautiful woman.");
			AppendInMessage("Hey...");
			AppendMessage("I meditate to regain my mana, before casting Lvl. 8 chicken of the Infinite.");
			AppendInMessage("Funny I still don't see it.");
			AppendMessage("I spend my mana reserves to cast Mighty F*ck of the Beyondness.");
			AppendInMessage("You are the worst cyber partner ever. This is ridiculous.");
			base.FireQueued();
		}
		
		public override void Send (string html)
		{
		}
		
		public override void Send (System.Xml.XmlElement element)
		{
		}
		
		public override void Dispose ()
		{
			
		}
		
		void AppendMessage (string body)
		{
			AppendMessage(false, body);
		}
		
		void AppendInMessage (string body)
		{
			AppendMessage(true, body);
		}
		
		void AppendMessage (bool incoming, string body)
		{
			Message msg = new Message(base.Account.Client.Document);
			msg.Body = body;
			msg.From = incoming ? m_OtherJid : base.Account.Jid;
			msg.To = incoming ? base.Account.Jid : m_OtherJid;
			base.AppendMessage(incoming, msg);
		}
	}
}
