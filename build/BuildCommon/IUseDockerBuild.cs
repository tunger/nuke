using Nuke.Common;
using Nuke.Common.Execution;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuildCommon
{
    interface IUseDockerBuild
    {
        [Parameter] string[] DockerTag => InjectionUtility.GetInjectionValue(() => DockerTag);
    }
}
