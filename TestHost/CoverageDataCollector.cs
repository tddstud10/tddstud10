using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.TestHost.Diagnostics;
using R4nd0mApps.TddStud10.TestRuntime;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CoverageDataCollector : ICoverageDataCollector
    {
        private PerAssemblySequencePointsCoverage store = new PerAssemblySequencePointsCoverage();

        private static ConcurrentDictionary<string, System.Lazy<List<string[]>>> tempStore = new ConcurrentDictionary<string, System.Lazy<List<string[]>>>();

        public CoverageDataCollector()
        {
            Logger.I.LogError("CoverageDataCollector: Instance is being created");
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

            var list = tempStore.GetOrAdd(testRunId, new System.Lazy<List<string[]>>(() => new List<string[]>())).Value;
            list.Add(new[] { assemblyId, methodMdRid, spId });
        }

        public void ExitUnitTest(string testRunId, string source, string document, string line)
        {
            System.Lazy<List<string[]>> sequencePoint = null;
            if (!tempStore.TryRemove(testRunId, out sequencePoint))
            {
                Logger.I.LogError("CoverageDataCollector: ExitUnitTest: Did not have any sequence points in thread {0} for {1},{2},{3}.", testRunId, source, document, line);
            }

            Logger.I.LogError("CoverageDataCollector: ExitUnitTest: Exiting unit test {0},{1},{2}. Sequence Points = {3}", source, document, line, sequencePoint.Value.Count);

            if (source == null || document == null || line == null || sequencePoint == null)
            {
                Logger.I.LogError(
                    "CoverageDataCollector: ExitUnitTest: Unexpected payload in ExitUnitTest: {0} {1} {2} {3}",
                    source ?? "<null>",
                    document ?? "<null>",
                    line ?? "<null>",
                    sequencePoint == null ? "<null>" : "<not null>");
                return;
            }

            var testId = new TestId
            {
                source = FilePath.NewFilePath(source),
                document = FilePath.NewFilePath(document),
                line = DocumentCoordinate.NewDocumentCoordinate(int.Parse(line))
            };

            sequencePoint.Value.ForEach(
                sp =>
                {
                    var assemId = AssemblyId.NewAssemblyId(Guid.Parse(sp[0]));
                    var list = store.GetOrAdd(assemId, _ => new List<SequencePointCoverage>());

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
        }

        #endregion
    }
}
