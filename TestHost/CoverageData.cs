using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R4nd0mApps.TddStud10.TestHost;

namespace R4nd0mApps.TddStud10.Engine
{
    public class CoverageData
    {
        private static CoverageData _instance;
        public static CoverageData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CoverageData();
                }

                return _instance;
            }
        }

        public CoverageSession CoverageSession { get; set; }

        public TestDetails TestDetails { get; set; }

        public SequencePointSession SequencePointSession { get; set; }

        public void UpdateCoverageResults(SequencePointSession seqPtSession, CoverageSession data, TestDetails testDetails)
        {
            SequencePointSession = seqPtSession;
            CoverageSession = data;
            TestDetails = testDetails;

            if (seqPtSession != null && CoverageSession != null && testDetails != null)
            {
                if (NewCoverageDataAvailable != null)
                    NewCoverageDataAvailable(this, EventArgs.Empty);
            }
        }

        public event EventHandler NewCoverageDataAvailable;

        public IEnumerable<string> GetFiles()
        {
            return from kvp in SequencePointSession
                   select kvp.Key;
        }

        public IEnumerable<SequencePoint> GetSequencePoints()
        {
            return from kvp in SequencePointSession
                   from sps in kvp.Value
                   select sps;
        }

        public IEnumerable<string> GetUnitTestsCoveringSequencePoint(SequencePoint sequencePoint)
        {
            var unitTests = from kvp in CoverageSession
                            from chi in kvp.Value
                            where chi.Method.Mvid == sequencePoint.Mvid && chi.Method.MdToken == sequencePoint.MdToken
                            select chi.UnitTest;
            return unitTests.Distinct();
        }
    }
}
