//
// Serialization.cs:
//
// Authors:
//   Eric Butler <eric@extremeboredom.net>
//
// (C) 2006 FileFind.net (http://filefind.net)
//

using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.IO;
using System;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;

namespace Synapse.Core
{
	public class XmlSerialization
	{
		public static string Serialize(object Obj) {
			StringBuilder sb = new StringBuilder();
			StringWriterWithEncoding HappyStringWriter = new StringWriterWithEncoding(sb, Encoding.UTF8);
			XmlSerializer x = new XmlSerializer(Obj.GetType());
			x.Serialize(HappyStringWriter, Obj);
			return HappyStringWriter.ToString();
		}	
		
		public static object DeSerialize(string Str, Type ObjectType)
		{
			byte[] b = System.Text.Encoding.UTF8.GetBytes(Str);
			using (MemoryStream ms = new MemoryStream()) {
				ms.Write(b, 0, b.Length);
				ms.Position = 0;
				XmlSerializer x = new XmlSerializer(ObjectType);
				return x.Deserialize(ms);
			}
		}
	}

	public class BinarySerialization
	{
		private static BinaryFormatter formatter = new BinaryFormatter();

		public static byte[] Serialize(object obj)
		{
			using (MemoryStream ms = new MemoryStream()) {
				formatter.Serialize(ms, obj);
				return ms.ToArray();
			}
		}

		public static object Deserialize(byte[] content)
		{
			using (MemoryStream ms = new MemoryStream(content)) {
				return formatter.Deserialize(ms);
			}
		}
	}
}
