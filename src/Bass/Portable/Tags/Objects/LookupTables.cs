using System;
using System.Collections.Generic;

namespace ManagedBass
{
    static class LookupTables
    {
        public static readonly TagProperties<HashSet<string>> Ape = new TagProperties<HashSet<string>>
        {
            Title       = new HashSet<string>(StringComparer.Ordinal) { "title" },
            Artist      = new HashSet<string>(StringComparer.Ordinal) { "artist" },
            Album       = new HashSet<string>(StringComparer.Ordinal) { "album" },
            AlbumArtist = new HashSet<string>(StringComparer.Ordinal) { "album artist" },
            Track       = new HashSet<string>(StringComparer.Ordinal) { "track" },
            Year        = new HashSet<string>(StringComparer.Ordinal) { "year" },
            Genre       = new HashSet<string>(StringComparer.Ordinal) { "genre" },
            Copyright   = new HashSet<string>(StringComparer.Ordinal) { "copyright" },
            Encoder     = new HashSet<string>(StringComparer.Ordinal) { "encodedby" },
            Publisher   = new HashSet<string>(StringComparer.Ordinal) { "label" },
            Composer    = new HashSet<string>(StringComparer.Ordinal) { "composer" },
            Conductor   = new HashSet<string>(StringComparer.Ordinal) { "conductor" },
            Lyricist    = new HashSet<string>(StringComparer.Ordinal) { "lyricist" },
            Remixer     = new HashSet<string>(StringComparer.Ordinal) { "remixer" },
            Producer    = new HashSet<string>(StringComparer.Ordinal) { "producer" },
            Comment     = new HashSet<string>(StringComparer.Ordinal) { "comment" },
            Grouping    = new HashSet<string>(StringComparer.Ordinal) { "grouping" },
            Mood        = new HashSet<string>(StringComparer.Ordinal) { "mood" },
            Rating      = new HashSet<string>(StringComparer.Ordinal) { "rating" },
            ISRC        = new HashSet<string>(StringComparer.Ordinal) { "isrc" },
            BPM         = new HashSet<string>(StringComparer.Ordinal) { "bpm" }
        };

        public static readonly TagProperties<HashSet<string>> Ogg = new TagProperties<HashSet<string>>
        {
            Title       = new HashSet<string>(StringComparer.Ordinal) { "title" },
            Artist      = new HashSet<string>(StringComparer.Ordinal) { "artist" },
            Album       = new HashSet<string>(StringComparer.Ordinal) { "album" },
            AlbumArtist = new HashSet<string>(StringComparer.Ordinal) { "albumartist" },
            Track       = new HashSet<string>(StringComparer.Ordinal) { "tracknumber" },
            Year        = new HashSet<string>(StringComparer.Ordinal) { "date" },
            Genre       = new HashSet<string>(StringComparer.Ordinal) { "genre" },
            Copyright   = new HashSet<string>(StringComparer.Ordinal) { "copyright" },
            Encoder     = new HashSet<string>(StringComparer.Ordinal) { "encodedby" },
            Publisher   = new HashSet<string>(StringComparer.Ordinal) { "label" },
            Composer    = new HashSet<string>(StringComparer.Ordinal) { "composer" },
            Conductor   = new HashSet<string>(StringComparer.Ordinal) { "conductor" },
            Lyricist    = new HashSet<string>(StringComparer.Ordinal) { "lyricist" },
            Remixer     = new HashSet<string>(StringComparer.Ordinal) { "remixer" },
            Producer    = new HashSet<string>(StringComparer.Ordinal) { "producer" },
            Comment     = new HashSet<string>(StringComparer.Ordinal) { "comment" },
            Grouping    = new HashSet<string>(StringComparer.Ordinal) { "grouping" },
            Mood        = new HashSet<string>(StringComparer.Ordinal) { "mood" },
            Rating      = new HashSet<string>(StringComparer.Ordinal) { "rating" },
            ISRC        = new HashSet<string>(StringComparer.Ordinal) { "isrc" },
            BPM         = new HashSet<string>(StringComparer.Ordinal) { "bpm" }
        };

        public static readonly TagProperties<HashSet<string>> RiffInfo = new TagProperties<HashSet<string>>
        {
            Title       = new HashSet<string>(StringComparer.Ordinal) { "inam" },
            Artist      = new HashSet<string>(StringComparer.Ordinal) { "iart" },
            Album       = new HashSet<string>(StringComparer.Ordinal) { "iprd" },
            AlbumArtist = new HashSet<string>(StringComparer.Ordinal) { "isbj" },
            Track       = new HashSet<string>(StringComparer.Ordinal) { "itrk", "iprt" },
            Year        = new HashSet<string>(StringComparer.Ordinal) { "icrd" },
            Genre       = new HashSet<string>(StringComparer.Ordinal) { "ignr" },
            Copyright   = new HashSet<string>(StringComparer.Ordinal) { "icop" },
            Encoder     = new HashSet<string>(StringComparer.Ordinal) { "isft" },
            Publisher   = new HashSet<string>(StringComparer.Ordinal) { "icms" },
            Composer    = new HashSet<string>(StringComparer.Ordinal) { "ieng" },
            Conductor   = new HashSet<string>(StringComparer.Ordinal) { "itch" },
            Lyricist    = new HashSet<string>(StringComparer.Ordinal) { "iwri" },
            Remixer     = new HashSet<string>(StringComparer.Ordinal) { "iedt" },
            Producer    = new HashSet<string>(StringComparer.Ordinal) { "ipro" },
            Comment     = new HashSet<string>(StringComparer.Ordinal) { "icmt" },
            Grouping    = new HashSet<string>(StringComparer.Ordinal) { "isrf" },
            Mood        = new HashSet<string>(StringComparer.Ordinal) { "ikey" },
            Rating      = new HashSet<string>(StringComparer.Ordinal) { "ishp" },
            ISRC        = new HashSet<string>(StringComparer.Ordinal) { "isrc" },
            BPM         = new HashSet<string>(StringComparer.Ordinal) { "ibpm" }
        };

        public static readonly TagProperties<HashSet<string>> Mp4 = new TagProperties<HashSet<string>>
        {
            Title       = new HashSet<string>(StringComparer.Ordinal) { "©nam" },
            Artist      = new HashSet<string>(StringComparer.Ordinal) { "©art" },
            Album       = new HashSet<string>(StringComparer.Ordinal) { "©alb" },
            AlbumArtist = new HashSet<string>(StringComparer.Ordinal) { "aart" },
            Track       = new HashSet<string>(StringComparer.Ordinal) { "trkn" },
            Year        = new HashSet<string>(StringComparer.Ordinal) { "©day" },
            Genre       = new HashSet<string>(StringComparer.Ordinal) { "©gen" },
            Copyright   = new HashSet<string>(StringComparer.Ordinal) { "cprt" },
            Encoder     = new HashSet<string>(StringComparer.Ordinal) { "©too" },
            Composer    = new HashSet<string>(StringComparer.Ordinal) { "©wrt" },
            Comment     = new HashSet<string>(StringComparer.Ordinal) { "©cmt" },
            Grouping    = new HashSet<string>(StringComparer.Ordinal) { "©grp" },
            Rating      = new HashSet<string>(StringComparer.Ordinal) { "rtng" },
        };

        public static readonly TagProperties<HashSet<string>> Id3v2 = new TagProperties<HashSet<string>>
        {
            Title       = new HashSet<string>(StringComparer.Ordinal) { "TIT2", "TT2" },
            Artist      = new HashSet<string>(StringComparer.Ordinal) { "TPE1", "TP1" },
            Album       = new HashSet<string>(StringComparer.Ordinal) { "TALB", "TAL" },
            AlbumArtist = new HashSet<string>(StringComparer.Ordinal) { "TPE2", "TP2" },
            Subtitle    = new HashSet<string>(StringComparer.Ordinal) { "TIT3", "TT3" },
            Track       = new HashSet<string>(StringComparer.Ordinal) { "TRK", "TRCK" },
            Year        = new HashSet<string>(StringComparer.Ordinal) { "TYER", "TYE" },
            Genre       = new HashSet<string>(StringComparer.Ordinal) { "TCON", "TCO" },
            Copyright   = new HashSet<string>(StringComparer.Ordinal) { "TCOP", "TCR" },
            Encoder     = new HashSet<string>(StringComparer.Ordinal) { "TENC", "TEN" },
            Publisher   = new HashSet<string>(StringComparer.Ordinal) { "TPUB", "TPB" },
            Composer    = new HashSet<string>(StringComparer.Ordinal) { "TCOM", "TCM" },
            Conductor   = new HashSet<string>(StringComparer.Ordinal) { "TPE3", "TP3" },
            Lyricist    = new HashSet<string>(StringComparer.Ordinal) { "TEXT", "TXT" },
            Remixer     = new HashSet<string>(StringComparer.Ordinal) { "TPE4", "TP4" },
            Producer    = new HashSet<string>(StringComparer.Ordinal) { "TIPL" },
            Comment     = new HashSet<string>(StringComparer.Ordinal) { "COMM", "COM" },
            Grouping    = new HashSet<string>(StringComparer.Ordinal) { "TIT1", "TT1" },
            Mood        = new HashSet<string>(StringComparer.Ordinal) { "TMOO" },
            Rating      = new HashSet<string>(StringComparer.Ordinal) { "POPM" },
            ISRC        = new HashSet<string>(StringComparer.Ordinal) { "TSCR" },
            BPM         = new HashSet<string>(StringComparer.Ordinal) { "TBPM", "TBP" }
        };
    }
}