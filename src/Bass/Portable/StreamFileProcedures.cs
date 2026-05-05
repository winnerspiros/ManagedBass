using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    /// <summary>
    /// <see cref="FileProcedures"/> for use with .Net <see cref="Stream"/>.
    /// </summary>
    class StreamFileProcedures : FileProcedures
    {
        readonly Stream _stream;

        /// <summary>
        /// Creates a new instance of <see cref="StreamFileProcedures"/>.
        /// </summary>
        /// <param name="InputStream">The <see cref="Stream"/> to wrap.</param>
        public StreamFileProcedures(Stream InputStream)
        {
            _stream = InputStream;

            if (!_stream.CanRead)
                throw new ArgumentException("Provide a readable stream", nameof(InputStream));

            Close = M => _stream.Dispose();
            Length = M => _stream.Length;
            Read = ReadProc;
            Seek = SeekProc;
        }

        int ReadProc(IntPtr Buffer, int Length, IntPtr User)
        {
            if (Length <= 0)
                return 0;

            // Rent a temporary buffer from the shared pool to avoid per-call heap allocation.
            var buf = ArrayPool<byte>.Shared.Rent(Length);
            try
            {
                var read = _stream.Read(buf, 0, Length);
                if (read > 0)
                    Marshal.Copy(buf, 0, Buffer, read);
                return read;
            }
            catch { return 0; }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf);
            }
        }

        bool SeekProc(long Offset, IntPtr User)
        {
            if (!_stream.CanSeek)
                return false;

            try
            {
                // BASS FILESEEKPROC receives an absolute byte offset from the start of the file
                // (un4seen docs: "The new position, in bytes, from the start of the file").
                _stream.Seek(Offset, SeekOrigin.Begin);
                return true;
            }
            catch { return false; }
        }
    }
}
