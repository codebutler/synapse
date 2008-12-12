//
// Pair.cs
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

namespace Synapse.Core
{
	public class Pair<T1, T2>
	{
		public Pair(T1 first, T2 second)
		{
			First = first;
			Second = second;
		}

		public T1 First { 
			get; set; 
		}

		public T2 Second {
			get; set;
		}

		public override bool Equals (object obj)
		{
			var other = (Pair<T1, T2>)obj;
			return (First.Equals(other.First) && Second.Equals(other.Second));
		}
	}
}
