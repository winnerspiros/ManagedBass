using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Cd
{
    /// <summary>
	/// Used with <see cref="BassCd.GetTOC(int,TOCMode, out TOC)" /> to retrieve the TOC from a CD.
	/// </summary>
	/// <remarks>
	/// If <see cref="TOCMode.Index"/> was used in the <see cref="BassCd.GetTOC(int,TOCMode, out TOC)" /> call, first and last will be index numbers rather than track numbers.
	/// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct TOC
    {
        short size;
        byte first;
        byte last;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        TOCTrack[] tracks;

        /// <summary>
        /// The first track number (or index number if <see cref="TOCMode.Index"/> is used).
        /// </summary>
        public int First => first;

        /// <summary>
        /// The last track number (or index number if <see cref="TOCMode.Index"/> is used).
        /// </summary>
        public int Last => last;

        /// <summary>
        /// The list of tracks retrieved (see <see cref="TOCTrack" />, up to 100 tracks).
        /// </summary>
        /// <remarks>Returns an <see cref="ArraySegment{T}"/> view over the internal fixed-size array — no allocation.</remarks>
        public ArraySegment<TOCTrack> Tracks
        {
            get
            {
                var n = size / BassMarshal.SizeOf<TOC>();
                return new ArraySegment<TOCTrack>(tracks, 0, Math.Min(n, tracks?.Length ?? 0));
            }
        }
    }
}