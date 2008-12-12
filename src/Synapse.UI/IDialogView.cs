//
// IDialogView.cs
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

namespace Synapse.UI
{
	public delegate DialogValidationResult DialogValidateEventHandler();

	public class DialogValidationResult
	{
		Dictionary<string, string> m_Errors = new Dictionary<string, string>();
		
		public Dictionary<string, string> Errors {
			get {
				return m_Errors;
			}
		}
		
		public bool IsValid {
			get {
				return m_Errors.Count == 0;
			}
		}
	}
	
	public interface IDialogView : IView
	{
		event EventHandler Accepted;
		event EventHandler Rejected;
		event DialogValidateEventHandler Validate;
	}
}
