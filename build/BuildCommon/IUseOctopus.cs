using Nuke.Common;
using Nuke.Common.Execution;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuildCommon
{
    interface IUseOctopus : IRequireVersionInfo
    {
        public Target OctoPush => _ => _
            .Executes(() =>
            {
                Logger.Info($"I require a version, and got: {FullSemVer}");
            });
    }
}
