using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R4nd0mApps.TddStud10.Hosts.VS.Helpers
{
    /// <summary>
    /// Coverage state for a editor line
    /// </summary>
    public enum LineCoverageState
    {
        /// <summary>
        /// Unknown. State will be ignored.
        /// </summary>
        /// <remarks>Mainly used for no "real" code</remarks>
        Unknown = 0,

        /// <summary>
        /// Line is fully covered
        /// </summary>
        CoveredWithPassingTests = 1,

        /// <summary>
        /// Line is not covered
        /// </summary>
        CoveredWithAtleastOneFailedTest = 2,

        /// <summary>
        /// Line is partly covered
        /// </summary>
        Uncovered = 3
    }
}
