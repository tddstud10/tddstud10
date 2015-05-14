/*
Copyright (c) 2015 Raghavendra Nagaraj

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */


namespace R4nd0mApps.TddStud10.Hosts.VS.Helpers
{
    /// <summary>
    /// Coverage state for a editor message
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
