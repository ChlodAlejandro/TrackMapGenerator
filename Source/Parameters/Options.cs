using System;
using System.Collections.Generic;
using System.IO;
using TrackMapGenerator.Formats;

namespace TrackMapGenerator.Parameters
{
    public static class Options
    {
        public static readonly List<StormData> Storms = new List<StormData>();
        public static string Generator = ParameterList.Generator.Default;
        public static string GeneratorOptions = ParameterList.GeneratorOptions.Default;
        public static int Resolution = ParameterList.Resolution.Default;
        public static string Background = ParameterList.Background.Default;
        public static string Output = ParameterList.Output.Default;

        internal static void ReadParameters(Tuple<Parameter, string, string[]>[] parameters)
        {
            ReadOrderedParameters(parameters);
            ReadUnorderedParameters(parameters);
        }

        internal static void ReadUnorderedParameters(Tuple<Parameter, string, string[]>[] parameters)
        {
            foreach ((dynamic parameter, string identifier, string[] args) in parameters)
            {
                switch (parameter.Name)
                {
                    case "generator":
                    {
                        Generator = parameter.GetValue(identifier, args);
                        break;
                    }
                    case "generator-options":
                    {
                        GeneratorOptions = parameter.GetValue(identifier, args);
                        break;
                    }
                    case "resolution":
                    {
                        Resolution = parameter.GetValue(identifier, args);
                        break;
                    }
                    case "background":
                    {
                        Background = parameter.GetValue(identifier, args);
                        break;
                    }
                    case "output":
                    {
                        Output = parameter.GetValue(identifier, args);
                        break;
                    }
                }
            }
        }

        internal static void ReadOrderedParameters(Tuple<Parameter, string, string[]>[] parameters)
        {
            int formatChanges = 0;
            bool inputAfterFormat = false;
            StormDataAttributes defaultAttributes = 0;
            StormDataFormat currentFormat = StormDataFormat.HURDAT;
            StormData lastStorm = null;
            foreach ((dynamic parameter, string identifier, string[] args) in parameters)
            {
                switch (parameter.Name)
                {
                    case "input":
                    {
                        string input = parameter.GetValue(identifier, args);
                        string data = input == "-" ? 
                            new StreamReader(Console.OpenStandardInput()).ReadToEnd() :
                            File.ReadAllText(input);
                        
                        Storms.Add(lastStorm = new StormData(
                            data,
                            currentFormat,
                            defaultAttributes
                        ));
                        inputAfterFormat = true;
                        break;
                    }
                    case "format":
                    {
                        currentFormat = parameter.GetValue(identifier, args) switch
                        {
                            "hurdat" => StormDataFormat.HURDAT,
                            "atcf" => StormDataFormat.ATCF,
                            "tcr" => StormDataFormat.TCR,
                            "md" => StormDataFormat.MD,
                            "tab" => StormDataFormat.TAB,
                            _ => currentFormat
                        };
                        formatChanges++;
                        inputAfterFormat = false;
                        break;
                    }
                    case "negative-x":
                    {
                        if (lastStorm == null)
                            defaultAttributes |= StormDataAttributes.NegativeX;
                        else
                            lastStorm.Attributes |= StormDataAttributes.NegativeX;
                        break;
                    }
                    case "negative-y":
                    {
                        if (lastStorm == null)
                            defaultAttributes |= StormDataAttributes.NegativeY;
                        else
                            lastStorm.Attributes |= StormDataAttributes.NegativeY;
                        break;
                    }
                }
            }

            if (formatChanges == 1 && !inputAfterFormat)
                // Retroactively change format of previous storms.
                foreach (StormData storm in Storms)
                {
                    storm.Format = currentFormat;
                }

            foreach (StormData storm in Storms)
            {
                StormDataFormatter.Formatters[storm.Format]().Read(storm, storm.Data);
            }
        }
    }
}