using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#pragma warning disable 1573
namespace TrackMapGenerator.Parameters
{
    public abstract class Parameter {}

    public abstract class TypedParameter<T> : Parameter
    {
        /// <summary>
        /// The default value for this parameter.
        /// </summary>
        public readonly T Default;
        
        /// <summary>
        /// The name of this parameter.
        /// </summary>
        public readonly string Name;
        
        /// <summary>
        /// The aliases of this parameter.
        /// </summary>
        public readonly string[] Aliases;

        /// <summary>
        /// A description of this parameter's settings.
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// A combined list of this parameter's identifiers (e.g. `--input`, `-i`)
        /// </summary>
        protected readonly List<string> Identifiers;
        
        /// <summary>
        /// Creates a new Parameter declaration, used to scan for values in process arguments.
        /// </summary>
        /// <param name="default">The default value for this parameter.</param>
        /// <param name="name">The full name of this parameter. Must not include spaces.</param>
        /// <param name="aliases">The aliases of this parameter. Must not include spaces.</param>
        /// <param name="description">A short description of this parameter's actions.</param>
        protected TypedParameter(
            T @default,
            string name,
            string[] aliases,
            string description
        )
        {
            Default = @default;
            Name = name;
            Aliases = aliases;
            Description = description;

            Identifiers = new List<string> { "--" + name, "-" + name };
            foreach (string alias in aliases)
            {
                Identifiers.Add("-" + alias);
                Identifiers.Add("--" + alias);
            }
        }

        /// <summary>
        /// Get the value of this parameter from an array of arguments.
        /// </summary>
        /// <param name="identifier">The identifier used for this method.</param>
        /// <param name="args">The arguments passed to this parameter.</param>
        /// <returns></returns>
        public abstract T GetValue(string identifier, string[] args);
    }

    public class FileParameter : TypedParameter<string>
    {
        private readonly bool requireExists;
        
        /// <inheritdoc />
        /// <param name="requireExists">Determines if the file has to exist or not.</param>
        public FileParameter(
            string @default,
            string name,
            string[] aliases,
            string description,
            bool requireExists
        ) : base(@default, name, aliases, description)
        {
            this.requireExists = requireExists;
        }

        public override string GetValue(string identifier, string[] args)
        {
            if (args.Length == 0 || args[0].StartsWith("-"))
                throw new ArgumentException($"Unexpected value for {identifier} + \"{args[0]}\"");

            string path = Path.GetFullPath(args[0], Directory.GetCurrentDirectory());
            if (requireExists && !File.Exists(path))
            {
                throw new ArgumentException($"{identifier} file does not exist: \"{path}\"");
            }

            return path;
        }
    }

    public class StringParameter : TypedParameter<string>
    {
        private readonly string[] allowedValues;

        /// <inheritdoc />
        /// <param name="allowedValues">The allowed values for this parameter.</param>
        public StringParameter(
            string @default,
            string name,
            string[] aliases,
            string description,
            string[] allowedValues = null
        ) : base(@default, name, aliases, description)
        {
            this.allowedValues = allowedValues;
        }

        public override string GetValue(string identifier, string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException($"Expecting at least 1 value for {identifier}, got 0");
            string value = string.Join(' ', args);
            if (allowedValues != null && allowedValues.Contains(value))
                throw new ArgumentException($"Unexpected value for {identifier} + \"{value}\"");
            return args[0];
        }
    }

    public class IntegerParameter : TypedParameter<int>
    {
        /// <inheritdoc />
        public IntegerParameter(
            int @default,
            string name,
            string[] aliases,
            string description
        ) : base(@default, name, aliases, description) {}

        public override int GetValue(string identifier, string[] args)
        {
            if (args.Length != 1)
                throw new ArgumentException($"Expecting only 1 value for {identifier}, got 0");
            return int.Parse(args[0]);
        }
    }

    public class BooleanParameter : TypedParameter<bool>
    {
        /// <inheritdoc />
        public BooleanParameter(
            bool @default,
            string name,
            string[] aliases,
            string description
        ) : base(@default, name, aliases, description) {}

        public override bool GetValue(string identifier, string[] args)
        {
            if (args.Length == 0 || args[0].StartsWith("-")) return true;
            if (args.Length > 1)
                throw new ArgumentException($"Expecting only 1 value for {identifier}, got {args.Length}");
            switch (args[0].ToLower())
            {
                case "0":
                case "false":
                case "f":
                case "no":
                case "n":
                    return false;
                case "1":
                case "true":
                case "t":
                case "yes":
                case "y":
                    return true;
                default:
                    throw new ArgumentException(
                        $"Unexpected value for boolean parameter {identifier}: \"{args[0]}\""
                    );
            }
        }
    }
}