using Nuke.Common;
using Nuke.Common.Execution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BuildCommon
{
    interface IWriteVersionInfo : IRequireVersionInfo
    {
        public Target WriteVersionInfo => _ => _
            .Executes(() =>
            {
                File.WriteAllText("version.txt", FullSemVer);
            });
    }
}
