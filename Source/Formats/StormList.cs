#region

using System;
using System.Collections.Generic;

#endregion

namespace TrackMapGenerator.Formats
{
    public class StormList : List<StormData>
    {
        public double MinimumLatitude { get; private set; } = double.NaN;
        public double MaximumLatitude { get; private set; } = double.NaN;
        public double MinimumLongitude { get; private set; } = double.NaN;
        public double MaximumLongitude { get; private set; } = double.NaN;

        public new void Add(StormData item)
        {
            base.Add(item);
            UpdateBounds();
        }

        public void UpdateBounds()
        {
            foreach (StormData storm in this)
            {
                if (storm.Points == null) continue;
                foreach (StormDataPoint point in storm.Points)
                {
                    if (double.IsNaN(MinimumLatitude) || point.Latitude < MinimumLatitude)
                        MinimumLatitude = point.Latitude;
                    if (double.IsNaN(MaximumLatitude) || point.Latitude > MaximumLatitude)
                        MaximumLatitude = point.Latitude;
                    if (double.IsNaN(MinimumLongitude) || point.Longitude < MinimumLongitude)
                        MinimumLongitude = point.Longitude;
                    if (double.IsNaN(MaximumLongitude) || point.Longitude > MaximumLongitude)
                        MaximumLongitude = point.Longitude;
                }
            }
        }

        public void IteratePoints(Action<StormData, StormDataPoint> callback)
        {
            foreach (StormData storm in this)
            {
                foreach (StormDataPoint point in storm.Points)
                {
                    callback(storm, point);
                }
            }
        }
        
        public void IteratePoints(Action<StormData, StormDataPoint, StormDataPoint> callback)
        {
            foreach (StormData storm in this)
            {
                StormDataPoint? lastPoint = null;
                foreach (StormDataPoint point in storm.Points)
                {
                    if (lastPoint != null)
                        callback(storm, (StormDataPoint) lastPoint, point);
                    lastPoint = point;
                }
            }
        }
    }
}