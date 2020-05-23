// Copyright 2019 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Colorful;
using JetBrains.Annotations;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;
using Console = Colorful.Console;

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
            WriteLineWithColors(text, Color.Green);
        }

        internal override void WriteTrace(string text)
        {
            WriteLineWithColors(text, Color.Gray);
        }

        internal override void WriteInformation(string text)
        {
            WriteLineWithColors(text, Color.Cyan);
        }

        protected override void WriteWarning(string text, string details = null)
        {
            WriteLineWithColors(text, Color.Yellow);
            if (details != null)
                WriteLineWithColors(details, Color.Yellow);
        }

        protected override void WriteError(string text, string details = null)
        {
            WriteLineWithColors(text, Color.Red);
            if (details != null)
                WriteLineWithColors(details, Color.Red);
        }

        internal override void WriteToolInvocation(string toolPath, Arguments arguments)
        {
            var previousForegroundColor = Console.ForegroundColor;

            using (DelegateDisposable.CreateBracket(
                null,
                () => Console.ForegroundColor = previousForegroundColor))
            {
                WriteWithColors("> ", Color.Cyan);

                var fullPath = Path.GetFullPath(toolPath);
                void QuotePath()
                {
                    if (fullPath.IsDoubleQuoteNeeded())
                        WriteWithColors("\"", Color.DarkGray);
                }

                QuotePath();
                Console.WriteFormatted("{0}{1}", Color.White, new[]
                {
                new Formatter(Path.GetDirectoryName(fullPath) + Path.DirectorySeparatorChar, Color.DarkGray),
                new Formatter(Path.GetFileName(fullPath) + Arguments.Space, Color.White)
            });
                QuotePath();

                var argPairs = arguments.GetArguments();
                foreach (var argumentPair in argPairs)
                {
                    var argumentNameOnly = string.Format(argumentPair.Key, string.Empty);
                    foreach (var argument in argumentPair.Value)
                    {
                        var renderedArgument = string.Format(argumentPair.Key + Arguments.Space, argument);
                        var indexOfValue = renderedArgument.IndexOf(argument);
                        if (indexOfValue <= 0)
                            WriteWithColors(renderedArgument, argumentNameOnly.Length > 0 ? Color.Magenta : Color.Red);
                        else
                        {
                            Console.WriteFormatted(argumentPair.Key + Arguments.Space, Color.Green, Color.Red, argument);
                        }
                    }
                }

                Console.WriteLine();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void WriteWithColors(string text, Color foregroundColor)
        {
            var previousForegroundColor = Console.ForegroundColor;

            using (DelegateDisposable.CreateBracket(
                () => Console.ForegroundColor = foregroundColor,
                () => Console.ForegroundColor = previousForegroundColor))
            {
                Console.Write(text);
            }
        }

        private void WriteLineWithColors(string text, Color foregroundColor)
        {
            WriteWithColors(text, foregroundColor);
            Console.WriteLine();
        }
    }
}
