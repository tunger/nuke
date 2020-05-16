using Nuke.Common.Tools.GitVersion;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuildCommon
{
    interface IProvideVersionInfoFromGitVersion : IProvideVersionInfo
    {
        [GitVersion] static GitVersion GitVersion;

        string IProvideVersionInfo.FullSemVer => GitVersion?.FullSemVer;
    }
}
