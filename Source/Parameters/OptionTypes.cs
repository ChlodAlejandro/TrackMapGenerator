#region

using System;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace TrackMapGenerator.Parameters
{
    [Flags]
    public enum StormDataAttributes
    {
        NegativeX,
        NegativeY
    }
}