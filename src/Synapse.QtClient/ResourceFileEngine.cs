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
	public class ResourceFileEngineHandler : QAbstractFileEngineHandler
	{
		public override QAbstractFileEngine Create (string fileName)
		{
			if (!String.IsNullOrEmpty(fileName) && fileName.StartsWith("resource:/"))
				return new ResourceFileEngine(fileName);
			else {
				return null;
			}
		}
	}
	
	public class ResourceFileEngine : AbstractSimpleFileEngine
	{
		string m_FileName;
		long   m_Pos = 0;
		Stream m_Stream;
		
		public ResourceFileEngine (string uri)
		{
			// uri begins with "resource:/"
			m_FileName = uri.Substring(10);

			if (m_FileName.Contains("/")) {
				var parts = m_FileName.Split('/');
				var providerId = parts[0];
				m_FileName = parts[1];

				var providers = Mono.Addins.AddinManager.GetExtensionNodes("/Synapse/UI/ResourceProviders");
				foreach (ResourceProviderCodon provider in providers) {
					if (provider.Id == providerId) {
						m_Stream = provider.GetResource(m_FileName);
						break;
					}
				}
				if (m_Stream == null) {
					throw new Exception("Resource not found: " + m_FileName);
				}
			} else {
				var asm = Assembly.GetCallingAssembly();
				m_Stream = asm.GetManifestResourceStream(m_FileName);
				if (m_Stream == null)
					throw new Exception("Resource not found: " + m_FileName);
			}
		}

		public override long Size ()
		{
			return m_Stream.Length;
		}

		public override long Read (Qyoto.Pointer<sbyte> data, long len)
		{
			if (len > Size() - Pos())
				len = Size() - Pos();
			
			if (len <= 0)
			    return 0;
			
			int ilen = Convert.ToInt32(len);
			byte[] buffer = new byte[ilen];
			m_Stream.Seek(m_Pos, SeekOrigin.Begin);
			m_Stream.Read(buffer, 0, ilen);
			for (int i = 0; i < ilen; i++) {
				data[i] = (sbyte) buffer[i];
 			}
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
			return m_FileName;
		}
		
		public new bool AtEnd ()
		{
			return Pos() == Size();
		}

		public override bool Close ()
		{
			m_Stream.Close();
			return true;
		}
	}
}
