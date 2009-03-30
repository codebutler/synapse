/* These methods were added to QByteArray in Qyoto trunk r946979 */

using System;

namespace Qyoto
{	
	public static class QByteArrayConverter
	{
		public static QByteArray FromArray(byte[] array)
		{
			var qba = new QByteArray(array.Length, '\0');
			Pointer<sbyte> p = qba.Data();
			for (int i = 0; i < array.Length; i++) {
				p[i] = (sbyte) array[i];
			}
			return qba;
		}

		public static byte[] ToArray(QByteArray qba)
		{
			Pointer<sbyte> p = qba.Data();
			byte[] array = new byte[qba.Size()];
			for (int i = 0; i < qba.Size(); i++) {
				array[i] = (byte) p[i];
			}
			return array;
		}
	}
}