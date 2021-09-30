using System;
using System.Collections.Generic;

namespace TrackMapGenerator.Map
{
    public static class Generators
    {
        public static readonly string DefaultGenerator = "mercator";
        
        public static readonly Dictionary<string, Func<string, Generator>> GeneratorList = 
            new Dictionary<string, Func<string, Generator>>
            {
                { "mercator", s => new MercatorGenerator(s) }
            };
    }
    
    public abstract class Generator
    {
        protected Generator(string generatorOptions) {}

        public abstract void Draw();
    }
}