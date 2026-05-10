using System;
using System.IO;
using System.Runtime.InteropServices;
using static System.Text.Encoding;

namespace ManagedBass
{
    /// <summary>
    /// Writes Wave data to a .wav file
    /// </summary>
    public class WaveFileWriter : IDisposable
    {
        #region Fields
        Stream _ofstream;
        BinaryWriter _writer;
        readonly long _dataSizePos, _factSampleCountPos;
        readonly object _locker = new object();
        readonly bool _leaveOpen;

        // Static pre-encoded header constants — avoids UTF8.GetBytes("RIFF") allocation on every construction.
        static readonly byte[] _riff    = new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' };
        static readonly byte[] _wavefmt = new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E',
                                                        (byte)'f', (byte)'m', (byte)'t', (byte)' ' };
        static readonly byte[] _fact    = new byte[] { (byte)'f', (byte)'a', (byte)'c', (byte)'t' };
        static readonly byte[] _data    = new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' };
        
        /// <summary>
        /// Number of bytes of audio
        /// </summary>
        public long Length { get; set; }

        readonly WaveFormat _waveFormat;
        #endregion

        #region Factory
        /// <summary>
        /// Creates a <see cref="WaveFileWriter"/> that writes to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="outStream">The stream to write to.</param>
        /// <param name="format">The wave format.</param>
        public WaveFileWriter(Stream outStream, WaveFormat format)
            : this(outStream, format, leaveOpen: false) { }

        /// <summary>
        /// Creates a <see cref="WaveFileWriter"/> that writes to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="outStream">The stream to write to.</param>
        /// <param name="format">The wave format.</param>
        /// <param name="leaveOpen">
        /// <see langword="true"/> to leave <paramref name="outStream"/> open after this
        /// <see cref="WaveFileWriter"/> is disposed; <see langword="false"/> to close it.
        /// </param>
        public WaveFileWriter(Stream outStream, WaveFormat format, bool leaveOpen)
        {
            _leaveOpen = leaveOpen;
            _ofstream = outStream;
            _writer = new BinaryWriter(outStream, UTF8);

            _writer.Write(_riff);
            _writer.Write(0); // placeholder
            _writer.Write(_wavefmt);
            _waveFormat = format;

            _writer.Write(18 + format.ExtraSize); // wave format Length
            format.Serialize(_writer);

            // CreateFactChunk
            if (format.Encoding != WaveFormatTag.Pcm)
            {
                _writer.Write(_fact);
                _writer.Write(4);
                _factSampleCountPos = outStream.Position;
                _writer.Write(0); // number of samples
            }

            // WriteDataChunkHeader
            _writer.Write(_data);
            _dataSizePos = outStream.Position;
            _writer.Write(0); // placeholder

            Length = 0;
        }
        #endregion

        #region Write
        /// <summary>
        /// Writes bytes to the WaveFile
        /// </summary>
        /// <param name="Data">the Buffer containing the wave data</param>
        /// <param name="Length">the number of bytes to write</param>
        public bool Write(byte[] Data, int Length)
        {
            try
            {
                lock (_locker)
                {
                    _writer.Write(Data, 0, Length);
                    this.Length += Length;
                }

                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Writes 16 bit samples to the Wave file
        /// </summary>
        /// <param name="Data">The Buffer containing the wave data</param>
        /// <param name="Length">The number of bytes to write</param>
        public bool Write(short[] Data, int Length)
        {
            try
            {
                lock (_locker)
                {
#if NET5_0_OR_GREATER
                    // Reinterpret the short[] as raw bytes and write in one shot — no per-element loop.
                    _writer.Write(MemoryMarshal.AsBytes(new ReadOnlySpan<short>(Data, 0, Length / 2)));
#else
                    var n = Length / 2;
                    for (var i = 0; i < n; i++)
                        _writer.Write(Data[i]);
#endif
                    this.Length += Length;
                }

                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Writes 32 bit float samples to the Wave file
        /// </summary>
        /// <param name="Data">The Buffer containing the wave data</param>
        /// <param name="Length">The number of bytes to write</param>
        public bool Write(float[] Data, int Length)
        {
            try
            {
                lock (_locker)
                {
#if NET5_0_OR_GREATER
                    // Reinterpret the float[] as raw bytes and write in one shot — no per-element loop.
                    _writer.Write(MemoryMarshal.AsBytes(new ReadOnlySpan<float>(Data, 0, Length / 4)));
#else
                    var n = Length / 4;
                    for (var i = 0; i < n; i++)
                        _writer.Write(Data[i]);
#endif
                    this.Length += Length;
                }

                return true;
            }
            catch { return false; }
        }
        #endregion
        
        #region IDisposable Members
        /// <summary>
        /// Closes this WaveFile
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        /// <summary>
        /// Actually performs the close, making sure the header contains the correct data
        /// </summary>
        /// <param name="Disposing">True if called from <see>Dispose</see></param>
        void Dispose(bool Disposing)
        {
            if (!Disposing || _writer == null)
                return;

            // Hold the lock for the entire finalisation sequence so that no concurrent
            // Write() call can sneak in between header-patching and writer disposal.
            lock (_locker)
            {
                try
                {
                    _writer.Flush();

                    _writer.Seek(4, SeekOrigin.Begin);
                    _writer.Write((int) (_ofstream.Length - 8));

                    if (_waveFormat.Encoding != WaveFormatTag.Pcm)
                    {
                        _writer.Seek((int) _factSampleCountPos, SeekOrigin.Begin);
                        _writer.Write((int) (Length * 8 / _waveFormat.BitsPerSample));
                    }

                    _writer.Seek((int) _dataSizePos, SeekOrigin.Begin);
                    _writer.Write((int) Length);
                }
                finally
                {
                    _writer.Dispose();
                    _writer = null;
                }
            }

            if (!_leaveOpen)
                _ofstream.Dispose(); // close the underlying base stream only when not asked to leave it open
            _ofstream = null;
        }

        /// <summary>
        /// Finaliser - should only be called if the User forgot to close this WaveFileWriter
        /// </summary>
        ~WaveFileWriter() { Dispose(false); }
        #endregion
    }
}
