//
// Client.cs
//
// Authors:
//   Aaron Bockover <abockover@novell.com>
//   Eric Butler <eric@extremeboredom.net>
//
// Copyright (C) 2008 Novell, Inc.
//
// This file was copied from Banshee - http://banshee-project.org/
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Threading;
using Synapse.Core;
using Synapse.Services;

namespace Synapse.ServiceStack
{	
    public interface IClient
    {
       	event Action<IClient> Started;
        
        string ClientId {
            get; 
        }

        bool IsStarted {
            get;
        }
		
		object CreateImage (byte[] data);
		
		object CreateImage (string fileName);
		
		object CreateImageFromResource (string resourceName);

		void ShowErrorWindow (string title, string errorMessage, string errorDetail);
		
		void ShowErrorWindow (string errorTitle, Exception error);

		void DesktopNotify (ActivityFeedItemTemplate template, IActivityFeedItem item, string text);
		
		void Exit ();
    }
}
