using Nuke.Common.Tools.GitVersion;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuildCommon
{
    interface IProvideVersionInfoFromFile : IProvideVersionInfo
    {
        string IProvideVersionInfo.FullSemVer => "static";
    }
}
