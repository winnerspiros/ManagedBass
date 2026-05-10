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

        // Per-handle reference count — allows O(1) check in Add() instead of an O(n) scan.
        static readonly ConcurrentDictionary<int, int> _handleCount =
            new ConcurrentDictionary<int, int>();

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
            Procedures[key] = proc;

            // Increment the per-handle ref count.  When it reaches 1 for the first time,
            // register the Free sync so we can clean up on channel release.
            // ConcurrentDictionary.AddOrUpdate is atomic per entry, so exactly one thread
            // will see newCount == 1 and register the sync.
            var newCount = _handleCount.AddOrUpdate(Handle, 1, static (_, old) => old + 1);
            if (newCount == 1)
                Bass.ChannelSetSync(Handle, SyncFlags.Free, 0, Freeproc);
        }

        /// <summary>
        /// Removes a Reference.
        /// </summary>
        public static void Remove(int Handle, int SpecialHandle)
        {
            if (!CrossPlatformHelper.IsDynamicCodeSupported)
                return;

            if (Procedures.TryRemove((Handle, SpecialHandle), out _))
                _handleCount.AddOrUpdate(Handle, 0, static (_, old) => old > 0 ? old - 1 : 0);
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

            _handleCount.TryRemove(Channel, out _);
        }
    }
}
