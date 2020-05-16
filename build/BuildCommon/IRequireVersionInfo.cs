using Nuke.Common;
using Nuke.Common.Execution;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BuildCommon
{
    interface IRequireVersionInfo
    {
        [Parameter] string VersionFullSemVer => InjectionUtility.GetInjectionValue(() => VersionFullSemVer)
            ?? EnvironmentInfo.GetParameter<string>("fullsemver");

        string GetVersionInfo(
            string parameterValue,
            Func<IProvideVersionInfo, string> selector,
            [CallerMemberName] string memberName = "")
        {
            if (!string.IsNullOrWhiteSpace(parameterValue))
                return parameterValue;

            if (this is IProvideVersionInfoFromGitVersion)
                return selector((IProvideVersionInfoFromGitVersion)this);

            Logger.Warn($"No version info found for: {memberName}");

            return null;
        }

        string FullSemVer => GetVersionInfo(VersionFullSemVer, x => x.FullSemVer);
    }
}
