using System;
using System.Collections.Generic;
using System.Text;

namespace BuildCommon
{
    interface IProvideVersionInfo
    {
        string FullSemVer { get; }
    }
}
