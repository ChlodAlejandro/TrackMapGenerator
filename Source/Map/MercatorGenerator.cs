using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TrackMapGenerator.Formats;
using TrackMapGenerator.Parameters;

namespace TrackMapGenerator.Map
{
    public class MercatorGenerator : Generator
    {
        public MercatorGenerator(string generatorOptions) : base(generatorOptions)
        {
            
        }

        public static PixelLocation CoordinateToPixels(Coordinate coord, int width, int height)
        {
            return new PixelLocation(
                (coord.Longitude + 180) / 360d * width,
                (coord.Latitude + 90) / 180d * height
            );
        }

        // Mutations are not asynchronous.
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public override void Draw()
        {
            if (!File.Exists(MapHandler.MapLocation))
                MapHandler.Download().Wait();

            Image background = Image.Load(MapHandler.MapLocation);
            
            // Generate the overlay (dots, lines, etc.)
            Image overlay = DrawOverlay(background.Width, background.Height);
            
            // Create the target image.
            Image target = new Image<Rgba32>(background.Width, background.Height);

            // Copy the background + overlay, given an offset and size
            target.Mutate(context =>
            {
                GraphicsOptions options = new GraphicsOptions
                {
                    ColorBlendingMode = PixelColorBlendingMode.Normal
                };
                context.DrawImage(background, options);
                context.DrawImage(overlay, options);
            });
            
            // Write image to file.
            target.Save(Options.Output);
            
            // Dispose.
            background.Dispose();
            overlay.Dispose();
            target.Dispose();
        }

        private Image DrawOverlay(int width, int height)
        {
            Image overlay = new Image<Rgba32>(width, height, Color.Transparent);
            
            overlay.Mutate(context =>
            {
                foreach (StormData storm in Options.Storms)
                {
                    foreach (StormDataPoint point in storm.Points)
                    {
                        PixelLocation center = CoordinateToPixels(
                            new Coordinate(point.Latitude, point.Longitude),
                            width, height
                        );
                        context.Fill(
                            Color.Blue,
                            new EllipsePolygon((float) center.X, (float) center.Y, 10)
                        );
                    }
                }
            });
            
            return overlay;
        }
    }
}