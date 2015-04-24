// Guids.cs
// MUST match guids.h
using System;

namespace R4nd0mApps.TddStud10.Hosts.VS.VSPackage
{
    static class GuidList
    {
        public const string guidVSPackagePkgString = "232fb38f-9556-4d67-9a5a-b44c191c51cd";
        public const string guidVSPackageCmdSetString = "7df2f318-f5a7-44de-aef3-415d87ba7c2b";

        public static readonly Guid guidVSPackageCmdSet = new Guid(guidVSPackageCmdSetString);
    };
}