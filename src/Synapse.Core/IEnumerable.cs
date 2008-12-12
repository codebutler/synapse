//
// IEnumerable.cs: Extension methods for things that really should be in .NET.
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
using System.Linq;

namespace Synapse.Core.ExtensionMethods
{	
	public static class IEnumerableExtensions
	{
		// Who the hell decided to call this "Select"!?!?
		public static IEnumerable<TResult> Map<TSource, TResult>(
    		this IEnumerable<TSource> source,
    		Func<TSource, TResult> selector
    	)
    	{
			return source.Select(selector);
		}
	}
	
	public static class IListExtensions
	{
		public static void RemoveIf<T>(this IList<T> source, Func<T, bool> block)
		{
			List<T> toDelete = new List<T>();
						
			foreach (var x in source)
				if (block(x)) toDelete.Add(x);
				
			foreach (var x in toDelete)
				source.Remove(x);
		}
	}
	
	public static class IDictionaryExtensions
	{
		public static void RemoveIf<TKey, TValue>(this IDictionary<TKey, TValue> source, Func<TKey, TValue, bool> block)
		{
			List<TKey> toDelete = new List<TKey>();
			
			foreach (var x in source) {
				if (block(x.Key, x.Value)) toDelete.Add(x.Key);
			}
				
			foreach (var x in toDelete)
				source.Remove(x);
		}
	}
}
