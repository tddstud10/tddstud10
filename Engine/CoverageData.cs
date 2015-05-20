using System;
using System.Collections.Generic;
using System.Linq;
using R4nd0mApps.TddStud10.Common.Domain;
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

        public PerAssemblySequencePointsCoverage CoverageSession { get; set; }

        public PerTestIdResults TestDetails { get; set; }

        public PerDocumentSequencePoints SequencePointSession { get; set; }

        public void UpdateCoverageResults(PerDocumentSequencePoints seqPtSession, PerAssemblySequencePointsCoverage data, PerTestIdResults testDetails)
        {
            SequencePointSession = seqPtSession;
            CoverageSession = data;
            TestDetails = testDetails;

            var handler = NewCoverageDataAvailable;
            if (NewCoverageDataAvailable != null)
            {
                NewCoverageDataAvailable(this, EventArgs.Empty);
            }
        }

        public event EventHandler NewCoverageDataAvailable;

        public IEnumerable<FilePath> GetFiles()
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

        public IEnumerable<TestId> GetUnitTestsCoveringSequencePoint(SequencePoint sequencePoint)
        {
            var unitTests = from kvp in CoverageSession
                            from chi in kvp.Value
                            where chi.methodId == sequencePoint.methodId
                            select chi.testId;
            return unitTests.Distinct();
        }
    }
}
