#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TrackMapGenerator.Parameters;

#endregion

namespace TrackMapGenerator.Formats
{
    public abstract class StormDataFormatter
    {
        public static Dictionary<StormDataFormat, Func<StormDataFormatter>> Formatters =
            new Dictionary<StormDataFormat, Func<StormDataFormatter>>
            {
                { StormDataFormat.ATCF, () => new ATCFFormat() }
            };

        public void Read(StormData storm, string data)
        {
            Read(storm, new MemoryStream(Encoding.UTF8.GetBytes(data)));
        }
        
        public abstract void Read(StormData storm, Stream data);

    }
    
}