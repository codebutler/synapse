
using System;

using Synapse.Xmpp;

using Monobjc;
using Monobjc.Cocoa;

namespace Synapse.CocoaClient
{
	public class AccountStatusViewController : NSViewController
	{
        static readonly Class MyClass = Class.GetClassFromType(typeof(AccountStatusViewController)); 
		
		[ObjectiveCField] public NSTextField nameLabel;
		[ObjectiveCField] public NSTextField statusLabel;
		[ObjectiveCField] public NSImageView avatarView;
		
		Account m_Account;
		
        [ObjectiveCMessage("init")]
        public override Id Init()
        {
			Console.WriteLine("Aaargh");
			
            this.NativePointer = this.SendMessageSuper<IntPtr>(MyClass, "initWithNibName:bundle:", "AccountStatusView", null);
			
			//base.InitWithNibNameBundle("AccountStatusView", null);
			
            // ...
            // Do additional initialization
            // ...
            return this;
        }

		
		public AccountStatusViewController (Account account)
		{	
			m_Account = account;
			
			//nameLabel.StringValue = account.UserDisplayName;
		}
	}
}
