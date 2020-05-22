// Copyright 2019 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;

namespace Nuke.Common.OutputSinks
{
    [UsedImplicitly]
    [ExcludeFromCodeCoverage]
    internal class SystemColorOutputSink : OutputSink
    {
        internal override void WriteNormal(string text)
        {
            Console.WriteLine(text);
        }

        internal override void WriteSuccess(string text)
        {
            WriteLineWithColors(text, ConsoleColor.Green);
        }

        internal override void WriteTrace(string text)
        {
            WriteLineWithColors(text, ConsoleColor.Gray);
        }

        internal override void WriteInformation(string text)
        {
            WriteLineWithColors(text, ConsoleColor.Cyan);
        }

        protected override void WriteWarning(string text, string details = null)
        {
            WriteLineWithColors(text, ConsoleColor.Yellow);
            if (details != null)
                WriteLineWithColors(details, ConsoleColor.Yellow);
        }

        protected override void WriteError(string text, string details = null)
        {
            WriteLineWithColors(text, ConsoleColor.Red);
            if (details != null)
                WriteLineWithColors(details, ConsoleColor.Red);
        }

        internal override void WriteToolInvocation(string toolPath, IArguments arguments)
        {
            var tokens = new List<(string text, ConsoleColor color)>();
            tokens.Add(("> ", ConsoleColor.Cyan));

            var fullPath = Path.GetFullPath(toolPath);
            void QuotePath()
            {
                if (fullPath.IsDoubleQuoteNeeded())
                    tokens.Add(("\"", ConsoleColor.DarkGray));
            }

            QuotePath();
            tokens.Add((Path.GetDirectoryName(fullPath) + Path.DirectorySeparatorChar, ConsoleColor.DarkGray));
            tokens.Add((Path.GetFileName(fullPath), ConsoleColor.White));
            QuotePath();

            tokens.Add((Arguments.Space.ToString(), ConsoleColor.DarkGray));

            var argPairs = arguments.GetArguments();
            foreach (var argumentPair in argPairs)
            {
                var argumentNameOnly = string.Format(argumentPair.Key, string.Empty);
                foreach (var argument in argumentPair.Value)
                {
                    var renderedArgument = string.Format(argumentPair.Key + Arguments.Space, argument);
                    var indexOfValue = renderedArgument.IndexOf(argument);
                    if (indexOfValue <= 0)
                        tokens.Add((renderedArgument, argumentNameOnly.Length > 0 ? ConsoleColor.Magenta : ConsoleColor.Red));
                    else // if (indexOfValue + argument.Length == renderedArgument.Length)
                    {
                        tokens.Add((renderedArgument.Substring(0, indexOfValue), ConsoleColor.Green));
                        tokens.Add((renderedArgument.Substring(indexOfValue), ConsoleColor.Red));
                        tokens.Add((renderedArgument.Substring(indexOfValue + argument.Length), ConsoleColor.Green));
                    }
                }
            }

            tokens.ForEach(x => WriteWithColors(x.text, x.color));
            Console.WriteLine();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void WriteWithColors(string text, ConsoleColor foregroundColor)
        {
            var previousForegroundColor = Console.ForegroundColor;

            using (DelegateDisposable.CreateBracket(
                () => Console.ForegroundColor = foregroundColor,
                () => Console.ForegroundColor = previousForegroundColor))
            {
                Console.Write(text);
            }
        }

        private void WriteLineWithColors(string text, ConsoleColor foregroundColor)
        {
            WriteWithColors(text, foregroundColor);
            Console.WriteLine();
        }
    }
}
