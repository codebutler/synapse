//
// Mono.Unix/Magic.cs
//
// Authors:
//   Milosz Tanski (mtanski@gmail.com)
//
// (C) 2004-2006 Milosz Tanski
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.IO;
using System.Runtime.InteropServices;


namespace Synapse.Core {

    public class MagicException : Exception
    {
        string _text;
        
        public MagicException(string text) : base()
        {
            _text = text;
        }

        override public string ToString()
        {
            return _text;
        }
    }
    
    public class Magic : IDisposable
    {
        IntPtr  _magic = IntPtr.Zero;
        bool    _followSymlinks = false;

        // open the magic database with default database files
        public Magic(bool ReturnMime)
        {
            _magic = magic_open((ReturnMime) ? DefaultFlags | MAGIC_FLAGS.MAGIC_MIME : DefaultFlags);
            if (_magic == IntPtr.Zero)
                throw new MagicException("Unable to open the magic database");

            if (magic_load(_magic, null) != 0)
                throw new MagicException(this.Error);
                
        }

        // open the magic database with a custom database file list
        public Magic(bool ReturnMime, string[] dblist)
        {
            _magic = magic_open((ReturnMime) ? DefaultFlags | MAGIC_FLAGS.MAGIC_MIME : DefaultFlags);
            if (_magic == IntPtr.Zero)
                throw new MagicException("Unable to open the magic database");

            foreach(string file in dblist) {
                if (magic_load(_magic, file) != 0)
                    throw new MagicException(this.Error);
            }
            
        }

        ~Magic()
        {
            if (_magic != IntPtr.Zero) {
                magic_close(_magic);
                 _magic = IntPtr.Zero;
            }
        }

        public void Dispose ()
        {
            if (_magic != IntPtr.Zero) {
                magic_close(_magic);
                _magic = IntPtr.Zero;
            }

            GC.SuppressFinalize(this);
        }

        public void AddDefaultDbFiles()
        {
            if (magic_load(_magic, null) != 0)
                throw new MagicException(this.Error); 
        }

        public void AddDbFile(string file)
        {
            if (magic_load(_magic, file) != 0)
                throw new MagicException(this.Error); 
        }

        public bool FollowSymlinks
        {
            get {
                return _followSymlinks;
            }

            set {
                magic_setflags(_magic, (value) ? DefaultFlags | MAGIC_FLAGS.MAGIC_MIME : DefaultFlags);
                _followSymlinks = value;
            }
            
        }

        private string Error
        {
            get {
                return Marshal.PtrToStringAuto(magic_error(_magic));
            }
        }

        public string Lookup(string filename)
        {
            string text;

            text = Marshal.PtrToStringAuto(magic_file(_magic, filename));
            if (text == null)
                throw new MagicException(this.Error);

            return text;
        }

        public string Lookup(FileInfo fi)
        {
            return Lookup(fi.FullName);
        }

        public string Lookup(byte[] data)
        {
            string text;

            text = Marshal.PtrToStringAuto(magic_buffer(_magic, data, data.Length));
            if (text == null)
                throw new MagicException(this.Error);

            return text;   
        }

        static public string Mime(string filename)
        {
            string mime;
            Magic m = new Magic(true);

            mime = Marshal.PtrToStringAuto(magic_file(m._magic, filename)); 
            if (mime == null) {
                throw new MagicException(m.Error);
            }

            return mime;
        }

        static public string Descrition(string filename)
        {
            string desc;
            Magic m = new Magic(false);

            magic_setflags(m._magic, DefaultFlags);
            desc = Marshal.PtrToStringAuto(magic_file(m._magic, filename)); 

            if (desc == null) {
                throw new MagicException(m.Error);
            }

            return desc;
        }

        private static MAGIC_FLAGS DefaultFlags{
            get {
                return MAGIC_FLAGS.MAGIC_NONE | MAGIC_FLAGS.MAGIC_PRESERVE_ATIME;
            }
        }

        [Flags]
        internal enum MAGIC_FLAGS
        {
            MAGIC_NONE              = 0,
            MAGIC_DEBUG             = 1,
            MAGIC_SYMLINK           = 1 << 1,
            MAGIC_COMPRESS          = 1 << 2,
            MAGIC_DEVICES           = 1 << 3,
            MAGIC_MIME              = 1 << 4,
            MAGIC_CONTINUE          = 1 << 5,
            MAGIC_CHECK             = 1 << 6,
            MAGIC_PRESERVE_ATIME    = 1 << 7,
            MAGIC_RAW               = 1 << 8,
            MAGIC_ERROR             = 1 << 9
        }
        
        [DllImport("libmagic.so.1")]
        private extern static IntPtr magic_open(MAGIC_FLAGS flags);

        [DllImport("libmagic.so.1")]
        private extern static void magic_close(IntPtr magic_cookie);

        [DllImport("libmagic.so.1")]
        private extern static int magic_setflags(IntPtr magic_cookie, MAGIC_FLAGS flags);

        [DllImport("libmagic.so.1")]
        private extern static IntPtr magic_file(IntPtr magic_cookie, string filename);

        [DllImport("libmagic.so.1")]
        private extern static IntPtr magic_buffer(IntPtr magic_cookie, Byte[] data, int len);

        [DllImport("libmagic.so.1")]
        private extern static IntPtr magic_error(IntPtr magic_cookie);

        [DllImport("libmagic.so.1")]
        private extern static int magic_load(IntPtr magic_cookie, string filename);
        
    }
}
