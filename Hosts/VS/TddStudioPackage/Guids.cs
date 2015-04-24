/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

// Guids.cs
// MUST match guids.h
using System;

namespace R4nd0mApps.TddStud10.Hosts.VS
{
    static class GuidList
    {
        public const string guidProgressBarPkgString = "6c8e3ef7-58d8-44a6-b4eb-71bcccc8f2e0";
        public const string guidProgressBarCmdSetString = "6c8e3ef8-58d8-44a6-b4eb-71bcccc8f2e0";
        public const string guidToolWindowPersistanceString = "6c8e3ef9-58d8-44a6-b4eb-71bcccc8f2e0";

        public static readonly Guid guidProgressBarCmdSet = new Guid(guidProgressBarCmdSetString);
    };
}