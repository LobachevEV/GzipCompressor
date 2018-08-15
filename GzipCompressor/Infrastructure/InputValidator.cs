using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GzipCompressor.Infrastructure
{
    public class InputValidator
    {
        public static void Validate(string[] args)
        {
            CheckInputArguments(args);
            var sourceFilePath = args[1];
            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException(sourceFilePath);
        }

        private static void CheckInputArguments(IEnumerable<string> args)
        {
            var exceptions = new List<Exception>();
            const string message = "No required argument";
            var argsCount = args.Count();
            if (argsCount < 1)
                exceptions.Add(new ArgumentException(message, "Mode"));
            if (argsCount < 2)
                exceptions.Add(new ArgumentException(message, "Source file name"));
            if (argsCount < 3)
                exceptions.Add(new ArgumentException(message, "Target file name"));

            if (exceptions.Count != 0)
                throw new AggregateException(exceptions);
        }
    }
}