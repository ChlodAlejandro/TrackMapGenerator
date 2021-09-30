using System;
using System.Diagnostics.CodeAnalysis;

namespace TrackMapGenerator.Parameters
{
    // These are acronyms.
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum StormDataFormat
    {
        HURDAT,
        ATCF,
        TCR,
        MD,
        TAB
    }

    [Flags]
    public enum StormDataAttributes
    {
        NegativeX,
        NegativeY
    }
}