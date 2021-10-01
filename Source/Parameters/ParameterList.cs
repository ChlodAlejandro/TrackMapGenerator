using System.Linq;
using TrackMapGenerator.Map;
using TrackMapGenerator.Scales;

namespace TrackMapGenerator.Parameters
{
    public static class ParameterList
    {
        
        public static readonly FileParameter Input = new FileParameter(
            null,
            "input",
            new[] { "i" },
            "An input text file to be used in creating a track map. The format can be controlled with --format.",
            true
        );

        public static readonly StringParameter Format = new StringParameter(
            "hurdat",
            "format",
            new[] { "f" },
            "The format of the succeeding input files. If this is placed after all --inputs, this will set the format of all previous input files.",
            new [] { "hurdat", "atcf", "tcr", "md", "tab" }
        );

        public static readonly BooleanParameter NegativeX = new BooleanParameter(
            false,
            "negative-x",
            new [] { "negx", "nx" },
            "Enabled if the latitudes are to the west of the prime meridian."
        );
        
        public static readonly BooleanParameter NegativeY = new BooleanParameter(
            false,
            "negative-y",
            new [] { "negy", "ny" },
            "Enabled if the longitudes are to the south of the equator."
        );

        public static readonly StringParameter Scale = new StringParameter(
            IntensityScale.DefaultScale,
            "scale",
            new[] { "s" },
            "The storm intensity scale to use.",
            IntensityScale.Scales.Keys.ToArray()
        );
        
        public static readonly StringParameter Generator = new StringParameter(
            Generators.DefaultGenerator,
            "generator",
            new [] { "g" },
            "The generator to use.",
            Generators.GeneratorList.Keys.ToArray()
        );
        
        public static readonly StringParameter GeneratorOptions = new StringParameter(
            "",
            "generator-options",
            new [] { "go" },
            "Options to be passed to the map generator."
        );
        
        public static readonly IntegerParameter Resolution = new IntegerParameter(
            2700,
            "resolution",
            new [] { "res" },
            "The width in pixels of the image generated (default: 2700px)"
        );

        public static readonly FileParameter Background = new FileParameter(
            null,
            "background",
            new[] { "bg", "back" },
            "The background map file to use. This will depend on the used projection.",
            true
        );
        
        public static readonly FileParameter Output = new FileParameter(
            "track.png",
            "output",
            new[] { "o" },
            "The output file where the track will be saved.",
            true
        );
        
        public static readonly Parameter[] Parameters = {
            Input, Format, NegativeX, NegativeY, 
            Generator, GeneratorOptions,
            Resolution, Background, Output
        };
        
    }
}