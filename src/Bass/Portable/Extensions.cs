using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ManagedBass
{
    /// <summary>
    /// Contains Helper and Extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Clips a value between a Minimum and a Maximum.
        /// </summary>
        public static T Clip<T>(this T Item, T MinValue, T MaxValue)
            where T : IComparable<T>
        {
            if (Item.CompareTo(MaxValue) > 0)
                return MaxValue;

            return Item.CompareTo(MinValue) < 0 ? MinValue : Item;
        }
        
        /// <summary>
        /// Converts <see cref="Resolution"/> to <see cref="BassFlags"/>
        /// </summary>
        public static BassFlags ToBassFlag(this Resolution Resolution) => Resolution switch
        {
            Resolution.Byte  => BassFlags.Byte,
            Resolution.Float => BassFlags.Float,
            _                => BassFlags.Default,
        };
        
        /// <summary>
        /// Returns the <param name="N">n'th (max 15)</param> pair of Speaker Assignment Flags
        /// </summary>
        public static BassFlags SpeakerN(int N) => (BassFlags)(N << 24);

        static bool? _floatable;

        /// <summary>
        /// Check whether Floating point streams are supported in the Current Environment.
        /// </summary>
        public static bool SupportsFloatingPoint
        {
            get
            {
                if (_floatable.HasValue) 
                    return _floatable.Value;

                // try creating a floating-point stream
                var hStream = Bass.CreateStream(44100, 1, BassFlags.Float, StreamProcedureType.Dummy);

                _floatable = hStream != 0;

                // floating-point channels are supported! (free the test stream)
                if (_floatable.Value) 
                    Bass.StreamFree(hStream);

                return _floatable.Value;
            }
        }

        /// <summary>
        /// Gets a <see cref="Version"/> object for a version number returned by BASS.
        /// </summary>
        public static Version GetVersion(int Num)
        {
            return new Version(Num >> 24 & 0xff,
                               Num >> 16 & 0xff,
                               Num >> 8 & 0xff,
                               Num & 0xff);
        }
        
        /// <summary>
        /// Returns a string representation for given number of channels.
        /// </summary>
        public static string ChannelCountToString(int Channels) => Channels switch
        {
            1 => "Mono",
            2 => "Stereo",
            3 => "2.1",
            4 => "Quad",
            5 => "4.1",
            6 => "5.1",
            7 => "6.1",
            _ when Channels < 1 => throw new ArgumentException("Channels must be greater than Zero."),
            _ => Channels + " Channels",
        };

        /// <summary>
        /// Extract an array of strings from a pointer to ANSI null-terminated string ending with a double null.
        /// </summary>
        public static unsafe string[] ExtractMultiStringAnsi(IntPtr Ptr)
        {
            if (Ptr == IntPtr.Zero)
                return Array.Empty<string>();

            // Pass 1: count entries by walking byte-by-byte — no string allocation.
            var count = 0;
            var p = (byte*)Ptr;
            while (*p != 0)
            {
                while (*p != 0) p++; // advance past the string
                p++;                 // skip null terminator
                count++;
            }

            if (count == 0)
                return Array.Empty<string>();

            // Pass 2: fill an exactly-sized array — no List<string> overhead.
            var result = new string[count];
            p = (byte*)Ptr;
            for (var i = 0; i < count; i++)
            {
                result[i] = Marshal.PtrToStringAnsi((IntPtr)p);
                while (*p != 0) p++;
                p++; // skip null terminator
            }

            return result;
        }

        /// <summary>
        /// Extract an array of strings from a pointer to UTF-8 null-terminated string ending with a double null.
        /// </summary>
        public static string[] ExtractMultiStringUtf8(IntPtr Ptr)
        {
            if (Ptr == IntPtr.Zero)
                return Array.Empty<string>();

            // Pass 1: count entries to allocate an exact-size array — no List<string> overhead.
            var count = 0;
            var p = Ptr;
            while (true)
            {
                var s = PtrToStringUtf8(p, out var size);
                if (string.IsNullOrEmpty(s)) break;
                count++;
                p += size + 1;
            }

            if (count == 0)
                return Array.Empty<string>();

            // Pass 2: fill the exact-sized array.
            var result = new string[count];
            p = Ptr;
            for (var i = 0; i < count; i++)
            {
                result[i] = PtrToStringUtf8(p, out var size);
                p += size + 1;
            }

            return result;
        }

        static unsafe string PtrToStringUtf8(IntPtr Ptr, out int Size)
        {
            Size = 0;

            if (Ptr == IntPtr.Zero)
                return null;

            var bytes = (byte*)Ptr.ToPointer();

            if (bytes[0] == 0)
                return null;
            
            while (bytes[Size] != 0)
                ++Size;

#if NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER
            // .NET Core 2.1+ / .NET 5+: decode directly from the raw pointer — no intermediate byte[] copy.
            return Encoding.UTF8.GetString(bytes, Size);
#elif NETSTANDARD2_1_OR_GREATER
            // On .NET Standard 2.1+ (e.g. Mono/Xamarin) use ReadOnlySpan to decode without byte[] allocation.
            return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(bytes, Size));
#else
            // netstandard2.0 / .NET Framework: use stackalloc for small strings (≤ 256 bytes).
            // For larger strings fall back to a pooled array to avoid large stack frames.
            if (Size <= 256)
            {
                var buf = stackalloc byte[Size];
                for (var i = 0; i < Size; i++) buf[i] = bytes[i];
                return Encoding.UTF8.GetString(buf, Size);
            }
            else
            {
                var buffer = ArrayPool<byte>.Shared.Rent(Size);
                try
                {
                    Marshal.Copy(Ptr, buffer, 0, Size);
                    return Encoding.UTF8.GetString(buffer, 0, Size);
                }
                finally { ArrayPool<byte>.Shared.Return(buffer); }
            }
#endif
        }

        /// <summary>
        /// Returns a Unicode string from a pointer to a Utf-8 string.
        /// </summary>
        public static string PtrToStringUtf8(IntPtr Ptr)
        {
            return PtrToStringUtf8(Ptr, out int _);
        }

        /// <summary>
        /// Returns a <see cref="StreamProcedure"/> which can be used to Play Silence on a Device (Useful during Wasapi Loopback Capture).
        /// </summary>
        public static StreamProcedure SilenceStreamProcedure { get; } = static (Handle, Buffer, Length, User) =>
        {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            // Zero the entire output buffer with a single intrinsic rather than a byte-by-byte Marshal.WriteByte loop.
            unsafe { new System.Span<byte>((void*)Buffer, Length).Clear(); }
#else
            for (var i = 0; i < Length; ++i)
                Marshal.WriteByte(Buffer, i, 0);
#endif
            return Length;
        };

        /// <summary>
        /// Returns an instance of <see cref="FileProcedures"/> wrapped around a <see cref="Stream"/>.
        /// </summary>
        /// <param name="InputStream">The <see cref="Stream"/> to use with BASS.</param>
        public static FileProcedures StreamFileProcedures(Stream InputStream)
        {
            return new StreamFileProcedures(InputStream);
        }

        /// <summary>
        /// Applies the Effect on a <see cref="MediaPlayer"/>.
        /// </summary>
        /// <param name="Effect">The Effect to Apply.</param>
        /// <param name="Player">The <see cref="MediaPlayer"/> to apply the Effect on.</param>
        /// <param name="Priority">Priority of the Effect in DSP chain.</param>
        public static void ApplyOn<T>(this Effect<T> Effect, MediaPlayer Player, int Priority = 0)
            where T : class, IEffectParameter, new()
        {
            Effect.ApplyOn(Player.Handle, Priority);

            Player.MediaLoaded += NewHandle =>
            {
                Effect.Dispose();

                Effect.ApplyOn(NewHandle, Priority);
            };
        }
    }
}
