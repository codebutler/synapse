//
// ResourceFileEngine.cs
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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Synapse.UI;
using Qyoto;

namespace Synapse.QtClient
{
	public class AvatarFileEngineHandler : QAbstractFileEngineHandler
	{
		public override QAbstractFileEngine Create (string fileName)
		{
			if (!String.IsNullOrEmpty(fileName) && fileName.StartsWith("avatar:/"))
				return new AvatarFileEngine(fileName);
			else {
				return null;
			}
		}
	}
	
	public class AvatarFileEngine : AbstractSimpleFileEngine
	{
		string m_AvatarHash;
		long   m_Pos = 0;
		byte[] m_Buffer;
		
		public AvatarFileEngine (string uri)
		{
			// uri begins with "avatar:/"
			m_AvatarHash = uri.Substring(8);

			QPixmap pixmap = (QPixmap) Synapse.Xmpp.AvatarManager.GetAvatar(m_AvatarHash);

			// FIXME: This doesn't seem very efficient...
			QBuffer buffer = new QBuffer();
			buffer.Open((uint)QIODevice.OpenModeFlag.WriteOnly);
			pixmap.Save(buffer, "PNG");
			buffer.Close();

			m_Buffer = new byte[buffer.Size()];
			Marshal.Copy(buffer.Data().Data().ToIntPtr(), m_Buffer, 0, m_Buffer.Length);
		}

		public override long Size ()
		{
			return m_Buffer.Length;
		}

		public override long Read (Qyoto.Pointer<sbyte> data, long len)
		{
			if (len > Size() - Pos())
				len = Size() - Pos();
			
			if (len <= 0)
			    return 0;
			
			int ilen = Convert.ToInt32(len);
			int ipos = Convert.ToInt32(m_Pos);
			Marshal.Copy(m_Buffer, ipos, data.ToIntPtr(), ilen);
			
			m_Pos += ilen;
			return ilen;
		}

		public override bool Seek (long pos)
		{
			m_Pos = pos;
			return true;
		}

		public override long Pos ()
		{
			return m_Pos;
		}
		
		public override string fileName (QAbstractFileEngine.FileName file)
		{
			return m_AvatarHash;
		}
		
		public new bool AtEnd ()
		{
			return Pos() == Size();
		}

		public override bool Close ()
		{
			m_Buffer = null;
			return true;
		}
	}
}
