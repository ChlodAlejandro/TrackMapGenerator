﻿using System;
using System.Collections.Generic;
using TrackMapGenerator.Parameters;

namespace TrackMapGenerator.Formats
{
    public class StormData
    {
        public readonly string Data;
        public StormDataFormat Format;
        public StormDataAttributes Attributes;
        public readonly Dictionary<string, dynamic> MiscellaneousData =
            new Dictionary<string, dynamic>();
        public StormDataPoint[] Points;

        public StormData(
            string data, 
            StormDataFormat format, 
            StormDataAttributes attributes = 0
        )
        {
            Data = data;
            Format = format;
            Attributes = attributes;
        }
    }

    public struct StormDataPoint
    {
        public DateTime Time;
        public double Latitude;
        public double Longitude;
        public double Winds;
        public double Pressure;
    }
}