using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TrackMapGenerator.Parameters;

namespace TrackMapGenerator
{
    internal static class TrackMapGenerator
    {
        public static readonly AssemblyName AssemblyName = Assembly.GetExecutingAssembly().GetName();
        public static readonly string Header = $"{AssemblyName.Name} v{AssemblyName.Version}";
        public static readonly string ProcessName =
            Process.GetCurrentProcess().MainModule?.ModuleName ?? "TrackMapGenerator";
        
        private static void Main(string[] args)
        {
            if (args.Length == 0 || args.Any(arg => new[] { "--help", "-h", "-?" }.Contains(arg)))
            {
                GenerateHelp();
            }
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
                
                Console.WriteLine();
            }
            
            Console.WriteLine(
                "Licensed under the Apache License, Version 2.0 (the \"License\");\n" + 
                "you may not use this file except in compliance with the License.\n" +
                "You may obtain a copy of the License at\n\n" +
                "    https://www.apache.org/licenses/LICENSE-2.0"
            );
        }
    }
}