#region

using System;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using TrackMapGenerator.Formats;
using TrackMapGenerator.Map;

#endregion

namespace TrackMapGenerator.Scales
{
    // ReSharper disable once InconsistentNaming
    public class SSHWSScale : IntensityScale
    {
        public override Rgba32 GetColor(StormDataPoint point)
        {
            return 
                point.Winds > 136 ? Rgba32.ParseHex("FF6060") :
                point.Winds > 112 ? Rgba32.ParseHex("FF8F20") :
                point.Winds > 95 ? Rgba32.ParseHex("FFC140") :
                point.Winds > 82 ? Rgba32.ParseHex("FFE775") :
                point.Winds > 63 ? Rgba32.ParseHex("FFFFCC") : 
                Rgba32.ParseHex(
                    point.Winds > 33 ? "00FAF4" : "5EBAFF"
                );
        }

        public override IPath GetShape(StormDataPoint point, PixelLocation center, int size)
        {
            return point.Type switch
            {
                StormType.TropicalCyclone =>
                    new EllipsePolygon(
                        (float) center.X, (float) center.Y, size
                    ),
                StormType.SubtropicalCyclone =>
                    new RectangularPolygon(
                        (float) center.X, (float) center.Y, size, size
                    ),
                StormType.ExtratropicalCyclone =>
                    new RegularPolygon(
                        (float) center.X, (float) center.Y, 3, (int)(size * 1.2), (float) Math.PI
                    ),
                StormType.Low =>
                    new RegularPolygon(
                        (float) center.X, (float) center.Y, 3, (int)(size * 1.2), (float) Math.PI
                    ),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}