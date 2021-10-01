using System.Collections.Generic;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using TrackMapGenerator.Formats;
using TrackMapGenerator.Map;

namespace TrackMapGenerator.Scales
{
    public abstract class IntensityScale
    {
        public const string DefaultScale = "sshws";
        public static readonly Dictionary<string, IntensityScale> Scales = new Dictionary<string, IntensityScale>()
        {
            { "sshws", new SSHWSScale() }
        };
        
        public abstract Rgba32 GetColor(StormDataPoint point);
        public abstract IPath GetShape(StormDataPoint point, PixelLocation center, int size);
    }
}