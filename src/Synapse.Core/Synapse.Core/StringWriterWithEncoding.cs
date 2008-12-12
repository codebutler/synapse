//
// StringWriterWithEncoding.cs:
//
// Authors:
//   Eric Butler <eric@extremeboredom.net>
//
// (C) 2006 FileFind.net (http://filefind.net)
//

using System.IO;
using System.Text;

namespace Synapse.Core
{
	public class StringWriterWithEncoding : StringWriter
	{
		private Encoding m_encoding;

		public StringWriterWithEncoding(StringBuilder sb, Encoding encoding) : base (sb)
		{
			m_encoding = encoding;
		}

		public override Encoding Encoding {
			get {
				return m_encoding;
			}
		}
	}
}
