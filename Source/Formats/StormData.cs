#region

using System;
using System.Collections.Generic;
using TrackMapGenerator.Parameters;

#endregion

namespace TrackMapGenerator.Formats
{
    public class StormData
    {
        public readonly string Data;
        public string Format;
        public StormDataAttributes Attributes;
        public readonly Dictionary<string, dynamic> MiscellaneousData =
            new Dictionary<string, dynamic>();
        public StormDataPoint[] Points;

        public StormData(
            string data, 
            string format, 
            StormDataAttributes attributes = 0
        )
        {
            Data = data;
            Format = format;
            Attributes = attributes;
        }
    }

    public enum StormType
    {
        TropicalCyclone,
        SubtropicalCyclone,
        ExtratropicalCyclone,
        Low
    }

    public struct StormDataPoint
    {
        public DateTime Time;
        public double Latitude;
        public double Longitude;
        public double Winds;
        public double Pressure;
        public StormType Type;
    }
}