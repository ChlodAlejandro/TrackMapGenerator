#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using TrackMapGenerator.Map;
using TrackMapGenerator.Parameters;

#endregion

namespace TrackMapGenerator
{
    public static class Program
    {
        public static readonly AssemblyName AssemblyName = Assembly.GetExecutingAssembly().GetName();
        public static readonly string Header = $"{AssemblyName.Name} v{AssemblyName.Version}";
        public static readonly string ProcessName =
            Process.GetCurrentProcess().MainModule?.ModuleName ?? "TrackMapGenerator";
        public static readonly string DataFolder = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TrackMapGenerator"
        );
        
        private static int Main(string[] args)
        {
            if (!Directory.Exists(DataFolder))
                Directory.CreateDirectory(DataFolder);
            
            if (args.Length == 0 || args.Any(arg => new[] { "--help", "-h", "-?" }.Contains(arg)))
            {
                GenerateHelp();
            } else if (args.Any(arg => new[] { "--version", "-v" }.Contains(arg))) {
                Console.WriteLine(AssemblyName.Version);
            }
            else
            {
                try
                {
                    Options.ReadParameters(ReadArguments(args));
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Could not parse arguments: {e.Message}");
                    Console.Error.WriteLine(e.StackTrace);
                    return 1;
                }
                if (Options.Storms.Count == 0)
                {
                    Console.Error.WriteLine("No storms provided.");
                    return 1;
                }
                try
                {
                    if (!File.Exists(Options.Background))
                    {
                        Console.WriteLine("Cannot find background. Will download.");
                        MapHandler.Download().Wait();
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Could not download track map: {e.Message}");
                    Console.Error.WriteLine(e.StackTrace);
                    return 3;
                }
                try
                {
                    Console.WriteLine("Generating track map...");
                    Image target = 
                        Generators.GeneratorList[Options.Generator](Options.GeneratorOptions).Draw();
                    Console.WriteLine("Saving...");
                    target.Save(Options.Output);
                    target.Dispose();
                }
                catch (Exception e)
                {
                    if (e is ExternalException)
                    {
                        Console.Error.WriteLine(
                            $"External exception code: {e.InnerException?.Message ?? "UNKNOWN"}"
                        );
                    }
                    Console.Error.WriteLine($"Could not generate track map: {e.Message}");
                    Console.Error.WriteLine(e.StackTrace);
                    return 1;
                }
                Console.WriteLine("Done.");
                return 0;
            }

            return 0;
        }

        private static Tuple<Parameter, string, string[]>[] ReadArguments(IEnumerable<string> args)
        {
            List<Tuple<Parameter, string, string[]>> parameters = new List<Tuple<Parameter, string, string[]>>();
            string param = null;
            List<string> paramOptions = new List<string>(); 
            foreach (string arg in args)
            {
                if (param == null && !arg.StartsWith("--"))
                    throw new ArgumentException($"Unexpected \"{arg}\"");
                
                if (!arg.StartsWith("--"))
                {
                    paramOptions.Add(arg);
                }
                else
                {
                    if (param != null)
                    {
                        foreach (dynamic parameter in ParameterList.Parameters)
                            if (((List<string>)parameter.Identifiers).Contains(param))
                                parameters.Add(Tuple.Create<Parameter, string, string[]>(
                                    parameter, param, paramOptions.ToArray()
                                ));
                        paramOptions.Clear();
                    }
                    param = arg;
                }
            }
            foreach (dynamic parameter in ParameterList.Parameters)
                if (((List<string>)parameter.Identifiers).Contains(param))
                    parameters.Add(Tuple.Create<Parameter, string, string[]>(
                        parameter, param, paramOptions.ToArray()
                    ));
            return parameters.ToArray();
        }

        private static void GenerateHelp()
        {
            Console.WriteLine(Header);
            Console.WriteLine("https://github.com/ChlodAlejandro/TrackMapGenerator");
            Console.WriteLine($"Usage: {ProcessName} [options] [-o <file>]");
            Console.WriteLine("Creates cyclone track maps using track data from meteorological agencies.");
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            foreach (Parameter untypedParameter in ParameterList.Parameters)
            {
                dynamic parameter = untypedParameter;
                if (parameter == null) continue;
                Console.WriteLine(
                    "    --" + parameter.Name +
                    (
                        parameter.Aliases != null && parameter.Aliases.Length > 0
                        ? ", -" + string.Join(
                            ", -", parameter.Aliases
                        ) : ""
                    )
                );

                string choppedDescription = "        ";
                foreach (string word in parameter.Description.Split(" "))
                {
                    choppedDescription += word + " ";
                    if (choppedDescription.Length <= 80) continue;
                    Console.WriteLine(choppedDescription);
                    choppedDescription = "        ";
                }
                if (choppedDescription.Length > 8) Console.WriteLine(choppedDescription);

                if (parameter is StringParameter && parameter.AllowedValues != null)
                {
                    Console.WriteLine(
                        "        Allowed values: [" + string.Join(", ", parameter.AllowedValues) + "]"
                    );
                }
                
                Console.WriteLine();
            }
            
            Console.WriteLine(
                "Licensed under the Apache License, Version 2.0 (the \"License\");\n" + 
                "you may not use this program except in compliance with the License.\n" +
                "You may obtain a copy of the License at\n\n" +
                "    https://www.apache.org/licenses/LICENSE-2.0"
            );
        }
    }
}