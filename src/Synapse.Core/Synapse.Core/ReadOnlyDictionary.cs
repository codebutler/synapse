//
// ReadOnlyDictionary.cs: A read-only wrapper for IDictionary<TKey, TValue>
//                        because Microsoft forgot about this.
// Authors:
//   Davy Brion
//
// (C) 2007 Davy Brion
//
// Code taken from http://ralinx.wordpress.com/2007/09/23/read-only-generic-dictionary/
// Thank you very much for making this available and saving me the time!
//   - Eric Butler <eric@extremeboredom.net>

using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;

namespace Synapse.Core
{
	[Serializable]
	public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, ISerializable,
	                                                IDeserializationCallback
	{
		private const string _readOnlyExceptionMessage = "This Dictionary is read-only!";
 
		private readonly IDictionary<TKey, TValue> _wrappedDictionary;
 
		public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionaryToWrap)
		{
			_wrappedDictionary = dictionaryToWrap;
		}
 
		#region IDeserializationCallback Members
 
		public void OnDeserialization(object sender)
		{
			((IDeserializationCallback)_wrappedDictionary).OnDeserialization(sender);
		}
 
		#endregion
 
		#region IDictionary Members
 
		public void Add(object key, object value)
		{
			throw new Exception(_readOnlyExceptionMessage);
		}
 
		public bool Contains(object key)
		{
			return ((IDictionary)_wrappedDictionary).Contains(key);
		}
 
		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return ((IDictionary)_wrappedDictionary).GetEnumerator();
		}
 
		public bool IsFixedSize
		{
			get { return ((IDictionary)_wrappedDictionary).IsFixedSize; }
		}
 
		ICollection IDictionary.Keys
		{
			get { return ((IDictionary)_wrappedDictionary).Keys; }
		}
 
		public void Remove(object key)
		{
			throw new Exception(_readOnlyExceptionMessage);
		}
 
		ICollection IDictionary.Values
		{
			get { return ((IDictionary)_wrappedDictionary).Values; }
		}
 
		public object this[object key]
		{
			get { return ((IDictionary)_wrappedDictionary)[key]; }
			set { throw new Exception(_readOnlyExceptionMessage); }
		}
 
		public void CopyTo(Array array, int index)
		{
			((IDictionary)_wrappedDictionary).CopyTo(array, index);
		}
 
		public bool IsSynchronized
		{
			get { return ((IDictionary)_wrappedDictionary).IsSynchronized; }
		}
 
		public object SyncRoot
		{
			get { return ((IDictionary)_wrappedDictionary).SyncRoot; }
		}
 
		#endregion
 
		#region IDictionary<TKey,TValue> Members
 
		public void Add(TKey key, TValue value)
		{
			throw new Exception(_readOnlyExceptionMessage);
		}
 
		public bool ContainsKey(TKey key)
		{
			return _wrappedDictionary.ContainsKey(key);
		}
 
		public ICollection<TKey> Keys
		{
			get { return new List<TKey>(_wrappedDictionary.Keys).AsReadOnly(); }
		}
 
		public bool Remove(TKey key)
		{
			throw new Exception(_readOnlyExceptionMessage);
		}
 
		public bool TryGetValue(TKey key, out TValue value)
		{
			return _wrappedDictionary.TryGetValue(key, out value);
		}
 
		public ICollection<TValue> Values
		{
			get { return new List<TValue>(_wrappedDictionary.Values).AsReadOnly(); }
		}
 
		public TValue this[TKey key]
		{
			get { return _wrappedDictionary[key]; }
			set { throw new Exception(_readOnlyExceptionMessage); }
		}
 
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			throw new Exception(_readOnlyExceptionMessage);
		}
 
		public void Clear()
		{
			throw new Exception(_readOnlyExceptionMessage);
		}
 
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return _wrappedDictionary.Contains(item);
		}
 
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			_wrappedDictionary.CopyTo(array, arrayIndex);
		}
 
		public int Count
		{
			get { return _wrappedDictionary.Count; }
		}
 
		public bool IsReadOnly
		{
			get { return true; }
		}
 
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			throw new Exception(_readOnlyExceptionMessage);
		}
 
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return _wrappedDictionary.GetEnumerator();
		}
 
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IDictionary)_wrappedDictionary).GetEnumerator();
		}
 
		#endregion
 
		#region ISerializable Members
 
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			((ISerializable)_wrappedDictionary).GetObjectData(info, context);
		}
 
		#endregion
	}
}
