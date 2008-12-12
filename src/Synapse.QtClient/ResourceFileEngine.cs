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
	
	public class ResourceFileEngine : QAbstractFileEngine
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
					}
				}
				throw new Exception("Resource not found: " + m_FileName);
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

		public override bool Open (uint openMode)
		{
			if ((openMode & (uint)QIODevice.OpenModeFlag.WriteOnly) == (uint)QIODevice.OpenModeFlag.WriteOnly)
				return false;
			return true;
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
			Marshal.Copy(buffer, 0, data.ToIntPtr(), ilen);
			
			m_Pos += ilen;
			return ilen;
		}

		public override long Write (string data, long len)
		{
			throw new InvalidOperationException();
		}

		public override bool SupportsExtension (QAbstractFileEngine.Extension extension)
		{
			return false;
		}

		public override bool SetSize (long size)
		{
			throw new InvalidOperationException();
		}

		public override bool SetPermissions (uint perms)
		{
			return false;
		}

		public override void SetFileName (string file)
		{
			throw new InvalidOperationException();
		}

		public override bool Seek (long pos)
		{
			m_Pos = pos;
			return true;
		}

		public override bool Rmdir (string dirName, bool recurseParentDirectories)
		{
			return false;
		}

		public override bool Rename (string newName)
		{
			return false;
		}

		public override bool Remove ()
		{
			return false;
		}

		public override long Pos ()
		{
			return m_Pos;
		}

		public override uint OwnerId (QAbstractFileEngine.FileOwner arg1)
		{
			unchecked {
				return (uint)-2;
			}
		}

		public override string Owner (QAbstractFileEngine.FileOwner arg1)
		{
			return String.Empty;
		}

		public override bool Mkdir (string dirName, bool createParentDirectories)
		{
			return false;
		}

		public override bool Link (string newName)
		{
			return false;
		}

		public override bool IsSequential ()
		{
			return false;
		}

		public override bool IsRelativePath ()
		{
			return false;
		}

		public override bool Flush ()
		{
			return false;
		}

		public override QDateTime fileTime (QAbstractFileEngine.FileTime time)
		{
			return new QDateTime();
		}
		
		public override string fileName (QAbstractFileEngine.FileName file)
		{
			return m_FileName;
		}

		public override uint FileFlags ()
		{
			throw new InvalidOperationException();
		}
		
		public new bool AtEnd ()
		{
			return Pos() == Size();
		}

        const uint PermsMask  = 0x0000FFFF;
        const uint TypesMask  = 0x000F0000;
        const uint FlagsMask  = 0x0FF00000;
		
		public override uint FileFlags (uint type)
		{
			QAbstractFileEngine.FileFlag ret = 0;
			
			if ((type & PermsMask) == PermsMask) {
				ret |= (QAbstractFileEngine.FileFlag.ReadOwnerPerm | 
					QAbstractFileEngine.FileFlag.ReadUserPerm | 
					QAbstractFileEngine.FileFlag.ReadGroupPerm |
					QAbstractFileEngine.FileFlag.ReadOtherPerm);
			}
			
			if ((type & TypesMask) == TypesMask) {
				ret |= QAbstractFileEngine.FileFlag.FileType;
			}
			
			if ((type & FlagsMask) == FlagsMask) {
					ret |= Qyoto.QAbstractFileEngine.FileFlag.ExistsFlag;
			}

			return (uint) ret;
		}

		public override bool extension (QAbstractFileEngine.Extension extension)
		{
			throw new InvalidOperationException();
		}

		public override System.Collections.Generic.List<string> EntryList (uint filters, System.Collections.Generic.List<string> filterNames)
		{
			throw new InvalidOperationException();
		}

		public override QAbstractFileEngineIterator EndEntryList ()
		{
			throw new InvalidOperationException();
		}

		public override bool Copy (string newName)
		{
			throw new InvalidOperationException();
		}

		public override bool Close ()
		{
			m_Stream.Close();
			return true;
		}

		public override bool CaseSensitive ()
		{
			return true;
		}

		public override QAbstractFileEngineIterator BeginEntryList (uint filters, System.Collections.Generic.List<string> filterNames)
		{
			throw new InvalidOperationException();
		}	
		
	}
}
