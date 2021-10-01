using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TrackMapGenerator.Formats;
using TrackMapGenerator.Parameters;
using TrackMapGenerator.Scales;
using TrackMapGenerator.Util;

namespace TrackMapGenerator.Map
{
    internal class GeneratorOptions : Dictionary<string, string>
    {
        public new string this[string key] => !ContainsKey(key) ? null : base[key];
    } 
    public class MercatorGenerator : Generator
    {
        private readonly GeneratorOptions generatorOptions = new GeneratorOptions();
        
        public MercatorGenerator(string generatorOptions) : base(generatorOptions)
        {
            foreach (string option in generatorOptions.Split(","))
            {
                string[] splitOption = option.Split("=");
                this.generatorOptions.Add(
                    splitOption[0], 
                    splitOption.Length > 1 ? string.Join("=", splitOption[1..]) : ""
                );
            }
        }

        public static PixelLocation CoordinateToPixels(Coordinate coord, int width, int height)
        {
            return new PixelLocation(
                MathD.Remap(-180, 180, 0, width, coord.Longitude),
                // Invert Y with respect to the equator.
                MathD.Remap(90, -90, 0, height, coord.Latitude)
            );
        }

        /// <summary>
        /// Finds the focused area for the track map.
        /// </summary>
        /// <returns>
        ///     Two coordinates which form the top-left and bottom-right
        ///     corners of the area.
        /// </returns>
        public Tuple<Coordinate, Coordinate> FindFocus()
        {
            double paddingX = double.Parse(
                generatorOptions["padX"] ?? generatorOptions["pad"] ?? "5"
            ); 
            double paddingY = double.Parse(
                generatorOptions["padY"] ?? generatorOptions["pad"] ?? "5"
            );
            
            // Ratio in terms of (X / Y)
            double xyRatio = double.Parse(
                generatorOptions["ratioX"] ?? "1.618033988749894"
            ) / double.Parse(
                generatorOptions["ratioY"] ?? "1.0"
            );
            
            double minimumLatitude = generatorOptions.ContainsKey("ymin")
                ? double.Parse(generatorOptions["ymin"])
                : Options.Storms.MinimumLatitude - paddingY;
            double maximumLatitude = generatorOptions.ContainsKey("ymax")
                ? double.Parse(generatorOptions["ymax"])
                : Options.Storms.MaximumLatitude + paddingY;
            double minimumLongitude = generatorOptions.ContainsKey("xmin")
                ? double.Parse(generatorOptions["xmin"])
                : Options.Storms.MinimumLongitude - paddingX;
            double maximumLongitude = generatorOptions.ContainsKey("xmax")
                ? double.Parse(generatorOptions["xmax"])
                : Options.Storms.MaximumLongitude + paddingX;

            double minimumX = double.Parse(generatorOptions["mindim"] ?? "45");

            double latitudeRange = Math.Abs(maximumLatitude - minimumLatitude);
            double longitudeRange = Math.Abs(maximumLongitude - minimumLongitude);
            // LAT: 32.2, LON: 28.6
            if (latitudeRange < longitudeRange / 1) {
                double diff = longitudeRange / 1 - latitudeRange;
            
                maximumLatitude += diff / 2.0;
                minimumLatitude -= diff / 2.0;
            }
            
            if (longitudeRange < latitudeRange / xyRatio) {
                double diff = latitudeRange / xyRatio - longitudeRange;
                
                maximumLongitude += diff / 2.0;
                minimumLongitude -= diff / 2.0;
            }
            
            if (longitudeRange < minimumX)
            {
                double remaining = minimumX - longitudeRange;
            
                maximumLongitude += remaining / 2.0;
                minimumLongitude -= remaining / 2.0;
            }

            return Tuple.Create(
                new Coordinate(maximumLatitude, minimumLongitude),
                new Coordinate(minimumLatitude, maximumLongitude)
            );
        }

        /// <summary>
        /// Finds the dimensions of the target image.
        /// </summary>
        /// <returns>A tuple containing width and height.</returns>
        public static Tuple<int, int> FindDimensions(Tuple<Coordinate, Coordinate> focus)
        {
            int resolution = Options.Resolution;
            (Coordinate topLeft, Coordinate bottomRight) = focus;
            double minimumLatitude = bottomRight.Latitude;
            double maximumLatitude = topLeft.Latitude;
            double minimumLongitude = topLeft.Longitude;
            double maximumLongitude = bottomRight.Longitude;
            
            if (Math.Abs(maximumLongitude - minimumLongitude) < Math.Abs(maximumLatitude - minimumLatitude))
                // Portrait
                return Tuple.Create(
                    (int) Math.Round(
                        resolution * 
                        (maximumLongitude - minimumLongitude) /
                        (maximumLatitude - minimumLatitude) + 
                        0.5
                    ),
                    resolution
                );
            // Landscape
            return Tuple.Create(
                resolution,
                (int) Math.Round(
                    resolution * 
                    (maximumLatitude - minimumLatitude) /
                    (maximumLongitude - minimumLongitude) +
                    0.5
                )
            );
        }

        /// <summary>
        /// Returns the specific position and size of a rectangle on a Mercator map
        /// which represents the area found by <see cref="FindFocus"/>.
        /// </summary>
        /// <param name="focus">The focus of the crop.</param>
        /// <param name="width">The map width.</param>
        /// <param name="height">The map height.</param>
        /// <returns>
        /// A tuple containing three values: the top-left corner of the bounding box,
        /// the width of the bounding box, and the height of the bounding box.
        /// </returns>
        public Tuple<PixelLocation, int, int> FindCropDimensions(
            Tuple<Coordinate, Coordinate> focus, int width, int height
        )
        {
            (Coordinate topLeft, Coordinate bottomRight) = focus;
            
            // Invert Y
            PixelLocation topLeftPixel = CoordinateToPixels(
                topLeft, width, height
            );
            PixelLocation bottomRightPixel = CoordinateToPixels(
                bottomRight, width, height
            );
            double minimumX = topLeftPixel.X;
            double maximumX = bottomRightPixel.X;
            double minimumY = bottomRightPixel.Y;
            double maximumY = topLeftPixel.Y;

            return Tuple.Create(
                new PixelLocation((int) Math.Ceiling(minimumX), (int) Math.Ceiling(maximumY)),
                (int) Math.Round(Math.Abs(maximumX - minimumX)),
                (int) Math.Round(Math.Abs(maximumY - minimumY))
            );
        }

        // Mutations are not asynchronous.
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public override void Draw()
        {
            if (!File.Exists(MapHandler.MapLocation))
                MapHandler.Download().Wait();

            Tuple<Coordinate, Coordinate> focus = FindFocus();
            (int width, int height) = FindDimensions(focus);
            Console.WriteLine(width + ":" + height);
            
            // Create the target image.
            Image target = new Image<Rgba32>(width, height);

            // Load the background
            Image background = Image.Load(MapHandler.MapLocation);
            
            // Generate the overlay (dots, lines, etc.)
            Image overlay = DrawOverlay(background.Width, background.Height);
            
            // TODO: Move this operation to the target to avoid the shapes from being scaled.
            background.Mutate(context =>
            {
                context.DrawImage(overlay, new GraphicsOptions());
            });

            // Crop the background to the portion we want.
            (PixelLocation cropPoint, int cropWidth, int cropHeight) = 
                FindCropDimensions(focus, background.Width, background.Height);
            target.Mutate(context =>
            {
                context.DrawImage(
                    background,
                    new Point(
                        0 - (int) cropPoint.X,
                        0 - (int) cropPoint.Y
                    ),
                    new GraphicsOptions()
                );
                ResizeOptions resizeOptions = new ResizeOptions
                {
                    CenterCoordinates = new PointF(0, 0),
                    Size = new Size(
                        (int) Math.Round((double) width / cropWidth * width),
                        (int) Math.Round((double) height / cropHeight * height)
                    )
                };
                context.Resize(resizeOptions);
                context.Crop(width, height);
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
            IntensityScale scale = IntensityScale.Scales[Options.Scale];
            
            overlay.Mutate(context =>
            {
                Options.Storms.IteratePoints((_, pointA, pointB) =>
                {
                    PixelLocation centerA = CoordinateToPixels(
                        new Coordinate(pointA.Latitude, pointA.Longitude),
                        width, height
                    );
                    PixelLocation centerB = CoordinateToPixels(
                        new Coordinate(pointB.Latitude, pointB.Longitude),
                        width, height
                    );
                    context.DrawLines(
                        Color.White,
                        3,
                        new PointF((int) centerA.X, (int) centerA.Y),
                        new PointF((int) centerB.X, (int) centerB.Y)
                    );
                });
                Options.Storms.IteratePoints((_, point) =>
                {
                    PixelLocation center = CoordinateToPixels(
                        new Coordinate(point.Latitude, point.Longitude),
                        width, height
                    );
                    context.Fill(
                        scale.GetColor(point),
                        scale.GetShape(point, center, 8)
                    );
                });
            });
            
            return overlay;
        }
    }
}