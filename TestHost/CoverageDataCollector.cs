using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.TestHost.Diagnostics;
using R4nd0mApps.TddStud10.TestRuntime;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CoverageDataCollector : ICoverageDataCollector
    {
        private PerAssemblySequencePointsCoverage store = new PerAssemblySequencePointsCoverage();

        private ConcurrentDictionary<string, ConcurrentBag<string[]>> tempStore = new ConcurrentDictionary<string, ConcurrentBag<string[]>>();

        public CoverageDataCollector()
        {
            Logger.I.LogInfo("CoverageDataCollector: Instance is being created");
        }

        public void SaveTestCases(string codeCoverageStore)
        {
            store.Serialize(codeCoverageStore);
        }

        #region ICoverageDataCollector Members

        public void Ping()
        {
            Logger.I.LogInfo("CoverageDataCollector - responding to ping.");
        }

        public void EnterSequencePoint(string testRunId, string assemblyId, string methodMdRid, string spId)
        {
            if (testRunId == null || assemblyId == null || methodMdRid == null | spId == null)
            {
                Logger.I.LogError(
                    "CoverageDataCollector: EnterSequencePoint: Invalid payload: {0} {1} {2} {3}",
                    testRunId ?? "<null>",
                    assemblyId ?? "<null>",
                    methodMdRid ?? "<null>",
                    spId ?? "<null>");

                return;
            }

            var list = tempStore.GetOrAdd(testRunId, new ConcurrentBag<string[]>());
            list.Add(new[] { assemblyId, methodMdRid, spId });
        }

        public void ExitUnitTest(string testRunId, string source, string document, string line)
        {
            ConcurrentBag<string[]> sequencePoints = null;
            if (!tempStore.TryRemove(testRunId, out sequencePoints))
            {
                Logger.I.LogError("CoverageDataCollector: ExitUnitTest: Did not have any sequence points in thread {0} for {1},{2},{3}.", testRunId, source, document, line);
                return;
            }

            if (source == null || document == null || line == null || sequencePoints == null)
            {
                Logger.I.LogError(
                    "CoverageDataCollector: ExitUnitTest: Unexpected payload in ExitUnitTest: {0} {1} {2} {3}",
                    source ?? "<null>",
                    document ?? "<null>",
                    line ?? "<null>",
                    sequencePoints == null ? "<null>" : "<not null>");
                return;
            }

            var testId = new TestId
            {
                source = FilePath.NewFilePath(source),
                document = FilePath.NewFilePath(document),
                line = DocumentCoordinate.NewDocumentCoordinate(int.Parse(line))
            };

            Parallel.ForEach(
                sequencePoints,
                sp =>
                {
                    var assemId = AssemblyId.NewAssemblyId(Guid.Parse(sp[0]));
                    var list = store.GetOrAdd(assemId, _ => new ConcurrentBag<SequencePointCoverage>());

                    list.Add(
                        new SequencePointCoverage
                        {
                            methodId = new MethodId
                            {
                                assemblyId = assemId,
                                mdTokenRid = MdTokenRid.NewMdTokenRid(uint.Parse(sp[1]))
                            },
                            sequencePointId = SequencePointId.NewSequencePointId(int.Parse(sp[2])),
                            testRunId = new TestRunId
                            {
                                testId = new TestId
                                {
                                    source = FilePath.NewFilePath(source),
                                    document = FilePath.NewFilePath(document),
                                    line = DocumentCoordinate.NewDocumentCoordinate(int.Parse(line))
                                },
                                testRunInstanceId = TestRunInstanceId.NewTestRunInstanceId(int.Parse(testRunId))
                            }
                        });
                });

            Logger.I.LogInfo("CoverageDataCollector: Servicing ExitUnitTest: {0},{1},{2}. Sequence Points = {3}", source, document, line, sequencePoints.Count);
        }

        #endregion
    }
}
