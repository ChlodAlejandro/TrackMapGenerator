namespace TrackMapGenerator.Map
{
    public readonly struct Coordinate
    {
        /// <summary>
        /// Latitude in degrees.
        /// </summary>
        public readonly double Latitude;
        /// <summary>
        /// Longitude in degrees.
        /// </summary>
        public readonly double Longitude;

        public Coordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
    
    public readonly struct PixelLocation
    {
        public readonly double X;
        public readonly double Y;
        
        public PixelLocation(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}