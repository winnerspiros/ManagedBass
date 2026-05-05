using System;
using System.Collections.Concurrent;

namespace ManagedBass
{
    /// <summary>
    /// Holds References to Channel Items like <see cref="SyncProcedure"/> and <see cref="FileProcedures"/>.
    /// </summary>
    public static class ChannelReferences
    {
        // (int,int) ValueTuple is a struct — zero heap allocation for the dictionary key,
        // unlike the previous Tuple<int,int> which caused a heap object per Add/Remove.
        static readonly ConcurrentDictionary<(int Handle, int SpecificHandle), object> Procedures =
            new ConcurrentDictionary<(int, int), object>();

        static readonly SyncProcedure Freeproc = Callback;

        /// <summary>
        /// Adds a Reference.
        /// </summary>
        public static void Add(int Handle, int SpecificHandle, object proc)
        {
            if (!CrossPlatformHelper.IsDynamicCodeSupported)
                return;

            if (proc == null)
                return;

            if (proc.Equals(Freeproc))
                return;

            var key = (Handle, SpecificHandle);

            // Check (without LINQ) whether any existing key belongs to this Handle.
            // If none found, register the Free sync so we can clean up on channel release.
            bool hasHandle = false;
            foreach (var pair in Procedures)
            {
                if (pair.Key.Handle == Handle)
                {
                    hasHandle = true;
                    break;
                }
            }

            if (!hasHandle)
                Bass.ChannelSetSync(Handle, SyncFlags.Free, 0, Freeproc);

            Procedures[key] = proc;
        }

        /// <summary>
        /// Removes a Reference.
        /// </summary>
        public static void Remove(int Handle, int SpecialHandle)
        {
            if (!CrossPlatformHelper.IsDynamicCodeSupported)
                return;

            Procedures.TryRemove((Handle, SpecialHandle), out _);
        }

        static void Callback(int Handle, int Channel, int Data, IntPtr User)
        {
            // ConcurrentDictionary enumeration is safe without a snapshot here because:
            // (a) TryRemove never throws during enumeration, and
            // (b) this callback is invoked only once per channel Free event, so no new entries
            //     for this channel can be added after the Free fires — meaning nothing is missed.
            foreach (var pair in Procedures)
            {
                if (pair.Key.Handle == Channel)
                    Procedures.TryRemove(pair.Key, out _);
            }
        }
    }
}
