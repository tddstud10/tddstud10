using System;
using System.Collections.Generic;
using System.Linq;
using R4nd0mApps.TddStud10.Common.Domain;

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

        public PerAssemblySequencePointsCoverage PerAssemblySequencePointsCoverage { get; set; }

        public PerTestIdResults PerTestIdResults { get; set; }

        public PerDocumentSequencePoints PerDocumentSequencePoints { get; set; }

        public void UpdateCoverageResults(PerDocumentSequencePoints seqPtSession, PerAssemblySequencePointsCoverage data, PerTestIdResults testDetails)
        {
            PerDocumentSequencePoints = seqPtSession;
            PerAssemblySequencePointsCoverage = data;
            PerTestIdResults = testDetails;

            var handler = NewCoverageDataAvailable;
            if (NewCoverageDataAvailable != null)
            {
                NewCoverageDataAvailable(this, EventArgs.Empty);
            }
        }

        public event EventHandler NewCoverageDataAvailable;

        public IEnumerable<FilePath> GetFiles()
        {
            return from kvp in PerDocumentSequencePoints
                   select kvp.Key;
        }

        public IEnumerable<SequencePoint> GetSequencePoints()
        {
            return from kvp in PerDocumentSequencePoints
                   from sps in kvp.Value
                   select sps;
        }

        public IEnumerable<TestRunId> GetUnitTestsCoveringSequencePoint(SequencePoint sequencePoint)
        {
            var unitTests = from kvp in PerAssemblySequencePointsCoverage
                            from chi in kvp.Value
                            where chi.methodId.Equals(sequencePoint.methodId)
                            select chi.testRunId;
            return unitTests.Distinct();
        }
    }
}
