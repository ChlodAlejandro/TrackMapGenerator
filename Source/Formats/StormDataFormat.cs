#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TrackMapGenerator.Parameters;

#endregion

namespace TrackMapGenerator.Formats
{
    public abstract class StormDataFormat
    {
        public static readonly string DefaultFormat = "atcf"; 
        public static readonly Dictionary<string, StormDataFormat> Formats =
            new Dictionary<string, StormDataFormat>
            {
                { "atcf", new ATCFFormat() },
            };

        public void Read(StormData storm, string data)
        {
            Read(storm, new MemoryStream(Encoding.UTF8.GetBytes(data)));
        }
        
        public abstract void Read(StormData storm, Stream data);

    }
    
}