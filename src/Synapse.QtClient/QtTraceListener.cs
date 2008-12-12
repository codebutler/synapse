//
// QtTraceListener.cs
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
using System.Diagnostics;
using Synapse.ServiceStack;
using System.Threading;
using Qyoto;

namespace Synapse.QtClient
{
	public class QtTraceListener : DefaultTraceListener
	{
		public override void Fail (string message, string detailMessage)
		{
			StackTrace stack = new StackTrace();
			ManualResetEvent mutex = new ManualResetEvent(false);			
			Application.Invoke(delegate {
				string msg = String.Format("{0}{1}{2}{1}{1}{3}", message, Environment.NewLine, detailMessage, stack);

				Console.Error.WriteLine("---");
				Console.Error.WriteLine("ASSERTION FAILED");
				Console.Error.WriteLine(msg);
				
				var result = QMessageBox.Critical(null, "Assertion Failed", msg,
				                                  (uint)QMessageBox.StandardButton.Abort |
				                                  (uint)QMessageBox.StandardButton.Ignore);
				
				if (result == Qyoto.QMessageBox.StandardButton.Abort) {
					Thread.CurrentThread.Abort();
				}
				
				mutex.Set();
			});
			mutex.WaitOne();
		}
	}
}
