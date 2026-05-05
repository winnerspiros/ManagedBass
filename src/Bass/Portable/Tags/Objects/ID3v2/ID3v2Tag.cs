// Adopted from the great article: http://www.codeproject.com/Articles/17890/Do-Anything-With-ID

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ManagedBass
{
    /// <summary>
    /// Reads ID3v2 Tags.
    /// </summary>
    public class ID3v2Tag
    {
        enum TextEncodings
        {
            Ascii,
            Utf16,
            Utf16Be,
            Utf8
        }

        IntPtr _ptr;
        readonly Version _versionInfo;

        // Cached static encoding to avoid Encoding.GetEncoding("UTF-16BE") allocation on every call.
        static readonly Encoding _utf16Be = Encoding.BigEndianUnicode;

        /// <summary>
        /// Reads tags from an <see cref="IntPtr"/> to an ID3v2 block.
        /// </summary>
        public ID3v2Tag(IntPtr Pointer)
        {
            _ptr = Pointer;

            if (ReadText(3, TextEncodings.Ascii) != "ID3") // If don't contain ID3v2 tag
                throw new DataMisalignedException("ID3v2 info not found");

            _versionInfo = new Version(2, ReadByte(), ReadByte()); // Read ID3v2 version           
            _ptr += 1; // Flags are skipped

            ReadAllFrames(ReadSize());
        }
        
        /// <summary>
        /// Reads tags from a Channel.
        /// </summary>
        public ID3v2Tag(int Channel) : this(Bass.ChannelGetTags(Channel, TagType.ID3v2)) { }

        void ReadAllFrames(int Length)
        {
            // If ID3v2 is ID3v2.2 FrameID, FrameLength of Frames is 3 byte
            // otherwise it's 4 character
            var frameIdLen = _versionInfo.Minor == 2 ? 3 : 4;

            // Minimum frame size is 10 because frame header is 10 byte
            while (Length > 10)
            {
                // check for padding( 00 bytes )
                var buf = ReadByte();

                if (buf == 0)
                {
                    Length--;
                    continue;
                }

                // if readed byte is not zero. it must read as FrameID
                _ptr -= 1;

                // ---------- Read Frame Header -----------------------
                var frameId = ReadText(frameIdLen, TextEncodings.Ascii);
                var frameLength = Convert.ToInt32(ReadUInt(frameIdLen));

                if (frameIdLen == 4)
                    ReadUInt(2);

                //Issue 78 Fix: Dbond (2020-07-16)
                //add saftey check to make sure we can't read past the overall header length
                //protecting against an invalid frame value.
                frameLength = Math.Min(frameLength, Length - 10);  //header space left is length - header size.. (10 bytes)

                var added = AddFrame(frameId, frameLength);

                // if don't read this frame, we must go forward to read next frame
                if (!added)
                    _ptr += frameLength;

                Length -= frameLength + 10;
            }
        }

        bool AddFrame(string FrameID, int Length)
        {
            if (FrameID == null || !IsValidFrameID(FrameID))
                return false;

            if (FrameID[0] == 'T' || FrameID[0] == 'W') // Is Text Frame
            {
                var isUrl = FrameID[0] == 'W';

                TextEncodings textEncoding;

                if (isUrl) textEncoding = TextEncodings.Ascii;
                else
                {
                    textEncoding = (TextEncodings)ReadByte();
                    Length--;

                    // Range check instead of Enum.IsDefined (which boxes the enum value).
                    if ((uint)textEncoding > 3)
                        return false;
                }

                var text = ReadText(Length, textEncoding);

                AddTextFrame(FrameID, text);
                return true;
            }

            switch (FrameID)
            {
                case "POPM":
                    ReadText(Length, TextEncodings.Ascii, ref Length, true); // Skip Email Address

                    var rating = ReadByte().ToString(); // Read Rating
                        
                    if (--Length > 8)
                        return false;
            
                    _ptr += Length; // Skip Counter value
                        
                    AddTextFrame("POPM", rating);                        
                    return true;

                case "COM":
                case "COMM":
                    var TextEncoding = (TextEncodings)ReadByte();
                        
                    Length--;

                    // Range check instead of Enum.IsDefined.
                    if ((uint)TextEncoding > 3)
                        return false;

                    _ptr += 3;
                        
                    Length -= 3;

                    ReadText(Length, TextEncoding, ref Length, true); // Skip Description

                    AddTextFrame("COMM", ReadText(Length, TextEncoding));
                    return true;

                case "APIC":
                    var textEncoding = (TextEncodings)ReadByte();
                        
                    Length--;

                    // Range check instead of Enum.IsDefined.
                    if ((uint)textEncoding > 3)
                        return false;

                    var mimeType = ReadText(Length, TextEncodings.Ascii, ref Length, true);

                    var pictureType = (PictureTypes)ReadByte();

                    Length--;

                    ReadText(Length, textEncoding, ref Length, true); // Skip Description

                    var data = new byte[Length];

                    Read(data, 0, Length);

                    PictureFrames.Add(new PictureTag
                    {
                        Data = data,
                        MimeType = mimeType,
                        PictureType = pictureType
                    });
                    return true;
            }

            return false;
        }

        // Replaced LINQ Cast<char>().All(...) with a plain loop — avoids IEnumerator allocation.
        static bool IsValidFrameID(string FrameID)
        {
            if (FrameID == null) return false;
            var len = FrameID.Length;
            if (len != 3 && len != 4) return false;

            for (var i = 0; i < len; i++)
            {
                var c = FrameID[i];
                if (!char.IsUpper(c) && !char.IsDigit(c))
                    return false;
            }
            return true;
        }

        // Single-lookup for duplicate text frame keys.
        void AddTextFrame(string Key, string Value)
        {
            if (TextFrames.TryGetValue(Key, out var existing))
                TextFrames[Key] = existing + ";" + Value;
            else
                TextFrames.Add(Key, Value);
        }

        /// <summary>
        /// Dictionary of Text frames.
        /// </summary>
        public Dictionary<string, string> TextFrames { get; } = new Dictionary<string, string>();

        /// <summary>
        /// List of Picture tags.
        /// </summary>
        public List<PictureTag> PictureFrames { get; } = new List<PictureTag>();

        #region Streaming
        string ReadText(int MaxLength, TextEncodings TEncoding)
        {
            var i = 0;
            return ReadText(MaxLength, TEncoding, ref i, false);
        }
        
        string ReadText(int MaxLength, TextEncodings TEncoding, ref int ReadedLength, bool DetectEncoding)
        {
            if (MaxLength <= 0)
                return "";

            var bytesRead = 0;

            // Use an ArrayPool-rented buffer instead of MemoryStream to avoid heap allocations.
            var rentedBuf = ArrayPool<byte>.Shared.Rent(MaxLength);
            var bufPos = 0;

            try
            {
                if (DetectEncoding && MaxLength >= 3)
                {
                    // Read 3 BOM bytes individually via Marshal.ReadByte instead of allocating byte[3].
                    var b0 = Marshal.ReadByte(_ptr);
                    var b1 = Marshal.ReadByte(_ptr + 1);
                    var b2 = Marshal.ReadByte(_ptr + 2);
                    _ptr += 3; // advance as if Read(buffer, 0, 3) was called

                    if (b0 == 0xFF && b1 == 0xFE)
                    {
                        // FF FE
                        TEncoding = TextEncodings.Utf16; // UTF-16 (LE)
                        _ptr -= 1;
                        bytesRead += 1;
                        MaxLength -= 2;
                    }
                    else if (b0 == 0xFE && b1 == 0xFF)
                    {
                        // FE FF
                        TEncoding = TextEncodings.Utf16Be;
                        _ptr -= 1;
                        bytesRead += 1;
                        MaxLength -= 2;
                    }
                    else if (b0 == 0xEF && b1 == 0xBB && b2 == 0xBF)
                    {
                        // EF BB BF
                        TEncoding = TextEncodings.Utf8;
                        MaxLength -= 3;
                    }
                    {
                        _ptr -= 3;
                        bytesRead += 3;
                    }
                }

                var is2ByteSeprator = TEncoding == TextEncodings.Utf16 || TEncoding == TextEncodings.Utf16Be;

                while (MaxLength > 0)
                {
                    var buf = ReadByte();

                    if (buf != 0) // if it's data byte
                        rentedBuf[bufPos++] = buf;

                    else // if Buf == 0
                    {
                        if (is2ByteSeprator)
                        {
                            var temp = ReadByte();

                            if (temp == 0)
                                break;

                            rentedBuf[bufPos++] = buf;   // the null byte is part of the 2-byte sequence
                            rentedBuf[bufPos++] = temp;
                            MaxLength--;
                        }
                        else break;
                    }

                    MaxLength--;
                }

                if (MaxLength < 0)
                    _ptr += MaxLength;

                ReadedLength -= bytesRead;

                return GetEncoding(TEncoding).GetString(rentedBuf, 0, bufPos);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedBuf);
            }
        }

        // Reads a single byte from the current pointer position — no byte[1] allocation.
        byte ReadByte()
        {
            var b = Marshal.ReadByte(_ptr);
            _ptr += 1;
            return b;
        }

        // Decodes a big-endian uint of 1-4 bytes without heap allocation.
        uint ReadUInt(int Length)
        {
            if (Length < 1 || Length > 4)
                throw new ArgumentOutOfRangeException(nameof(Length), "ReadUInt method can read 1-4 byte(s)");

            uint result = 0;
            for (var i = 0; i < Length; i++)
                result = (result << 8) | ReadByte();
            return result;
        }

        int ReadSize()
        {
            /* ID3 Size is like:
             * 0XXXXXXXb 0XXXXXXXb 0XXXXXXXb 0XXXXXXXb (b means binary)
             * the zero bytes must ignore, so we have 28 bits number = 0x1000 0000 (maximum)
             * it's equal to 256MB
             */
            return ReadByte() * 0x200000 + ReadByte() * 0x4000 + ReadByte() * 0x80 + ReadByte();
        }

        static Encoding GetEncoding(TextEncodings TEncoding) => TEncoding switch
        {
            TextEncodings.Utf16   => Encoding.Unicode,
            TextEncodings.Utf16Be => _utf16Be,
            _                     => Encoding.UTF8,
        };

        void Read(byte[] Buffer, int Offset, int Count)
        {
            Marshal.Copy(_ptr, Buffer, Offset, Count);
            _ptr += Count;
        }
        #endregion
    }
}
