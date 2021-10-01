using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TrackMapGenerator.Parameters;

namespace TrackMapGenerator.Formats
{
    // ReSharper disable once InconsistentNaming
    public class ATCFFormat : StormDataFormatter
    {
        public override void Read(StormData storm, Stream data)
        {
            StreamReader reader = new StreamReader(data);
            List<StormDataPoint> dataPoints = new List<StormDataPoint>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Trim().Length == 0)
                    continue;
                string[] columns = line.Split(", ");
                
                // X, X, DateTime, X, BEST, X, X, LAT, LON, WIN, PRS, ...
                if (columns[4] != "BEST")
                    // Not a b-deck line.
                    continue;

                dataPoints.Add(new StormDataPoint
                {
                    Time = DateTime.Parse(
                        columns[2][..4] + "-" +
                        columns[2][4..6] + "-" +
                        columns[2][6..8] + " " +
                        columns[2][8..10] + ":00"
                    ),
                    Latitude = double.Parse(columns[6][..^1]) / 10 * 
                               (columns[6].Last() == 'N' ? 1 : -1),
                    Longitude = double.Parse(columns[7][..^1]) / 10 * 
                                (columns[7].Last() == 'E' ? 1 : -1),
                    Winds = double.Parse(columns[8]),
                    Pressure = double.Parse(columns[9]),
                    Type = columns[10] switch {
                         var x when
                             x == "TD" ||
                             x == "TS" ||
                             x == "ST" ||
                             x == "TC" ||
                             x == "HU" ||
                             x == "TY" => StormType.TropicalCyclone,
                         var x when
                             x == "SD" ||
                             x == "SS" => StormType.SubtropicalCyclone,
                         "EX" => StormType.ExtratropicalCyclone,
                         var x when
                             x == "DB" ||
                             x == "LO" ||
                             x == "WV" ||
                             x == "MD" => StormType.Low,
                         _ => dataPoints.Last().Type
                    }
                });
            }

            storm.Points = dataPoints.ToArray();
        }
    }
}