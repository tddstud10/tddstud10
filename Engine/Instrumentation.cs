using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using R4nd0mApps.TddStud10;
using Mono.Cecil.Rocks;
using R4nd0mApps.TddStud10.TestHost;
using R4nd0mApps.TddStud10.Engine.Diagnostics;

namespace R4nd0mApps.TddStud10
{
    internal class Instrumentation
    {
        private static string testRunnerPath;

        // TODO: Cleanup: Merge these 2 methods
        public static void GenerateSequencePointInfo(DateTime timeFilter, string buildOutputRoot, string seqencePointStore)
        {
            var dict = new SequencePointSession();
            foreach (var assemblyPath in Directory.EnumerateFiles(buildOutputRoot, "*.dll"))
            {
                if (!File.Exists(Path.ChangeExtension(assemblyPath, ".pdb")))
                {
                    continue;                
                }

                if (File.GetLastWriteTimeUtc(assemblyPath) < timeFilter)
                {
                    continue;
                }

                Logger.I.Log("Generating sequence point info for {0}.", assemblyPath);

                var assembly = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters { ReadSymbols = true });

                var sps = from mod in assembly.Modules
                          from t in mod.GetTypes()
                          from m in t.Methods
                          where m.Body != null
                          from i in m.Body.Instructions
                          where i.SequencePoint != null
                          where i.SequencePoint.StartLine != 0xfeefee
                          select new { mod, m, i.SequencePoint };

                int id = 0;
                foreach (var sp in sps)
                {
                    if (!dict.ContainsKey(sp.SequencePoint.Document.Url))
                    {
                        dict[sp.SequencePoint.Document.Url] = new List<SequencePoint>();
                    }

                    dict[sp.SequencePoint.Document.Url].Add(new SequencePoint
                    {
                        Mvid = sp.mod.Mvid.ToString(),
                        MdToken = sp.m.MetadataToken.RID.ToString(),
                        ID = (id++).ToString(),
                        File = sp.SequencePoint.Document.Url,
                        StartLine = sp.SequencePoint.StartLine,
                        StartColumn = sp.SequencePoint.StartColumn,
                        EndLine = sp.SequencePoint.EndLine,
                        EndColumn = sp.SequencePoint.EndColumn,
                    });
                }
            }

            using (StringWriter writer = new StringWriter())
            {
                SequencePointSession.Serializer.Serialize(writer, dict);
                File.WriteAllText(seqencePointStore, writer.ToString());
            }
        }

        public static void Instrument(DateTime timeFilter, string buildOutputRoot, string discoveredUnitTestsStore)
        {
            string currFolder = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
            testRunnerPath = Path.Combine(Path.GetDirectoryName(currFolder), "TddStud10.TestHost.exe");

            var unitTests = new DiscoveredUnitTests();

            foreach (var assemblyPath in Directory.EnumerateFiles(buildOutputRoot, "*.dll"))
            {
                if (!File.Exists(Path.ChangeExtension(assemblyPath, ".pdb")))
                {
                    continue;                
                }

                if (File.GetLastWriteTimeUtc(assemblyPath) < timeFilter)
                {
                    continue;
                }

                Logger.I.Log("Instrumenting {0}.", assemblyPath);

                var assembly = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters { ReadSymbols = true });

                var enterSeqPointMethDef = from t in ModuleDefinition.ReadModule(testRunnerPath).GetTypes()
                                          where t.Name == "Marker"
                                          from m in t.Methods
                                          where m.Name == "EnterSequencePoint"
                                          select m;

                var enterUnitTestMethDef = from t in ModuleDefinition.ReadModule(testRunnerPath).GetTypes()
                                          where t.Name == "Marker"
                                          from m in t.Methods
                                           where m.Name == "EnterUnitTest"
                                          select m;

                /*
	            IL_0001: ldstr <mvid>
	            IL_0006: ldstr <mdtoken>
	            IL_000b: ldstr <spid>
	            IL_000d: call void R4nd0mApps.TddStud10.TestHost.Marker::EnterSequencePoint(string, ldstr, ldstr)
                 */
                MethodReference enterSeqPointMethRef = assembly.MainModule.Import(enterSeqPointMethDef.First());
                MethodReference enterUnitTestMethRef = assembly.MainModule.Import(enterUnitTestMethDef.First());

                foreach (var module in assembly.Modules)
                foreach (var type in module.Types)
                foreach (MethodDefinition meth in type.Methods)
                {
                    if (meth.Body == null)
                    {
                        continue;
                    }

                    meth.Body.SimplifyMacros();
                    if (meth.CustomAttributes.Any(ca => ca.AttributeType.Name == "FactAttribute"))
                    {
                        var unitTestName = string.Format("{0} {1}::{2}", meth.ReturnType.FullName, meth.DeclaringType.FullName, meth.Name);
                        if (!unitTests.ContainsKey(assemblyPath))
                        {
                            unitTests[assemblyPath] = new List<string>();
                        }
                        unitTests[assemblyPath].Add(unitTestName);

                        Instruction instrMarker = meth.Body.Instructions[0];
                        Instruction instr = null;
                        var ilProcessor = meth.Body.GetILProcessor();

                        // IL_000d: call void R4nd0mApps.TddStud10.TestHost.Marker::EnterUnitTest(ldstr, ldstr)
                        instr = ilProcessor.Create(OpCodes.Call, enterUnitTestMethRef);
                        ilProcessor.InsertBefore(instrMarker, instr);
                        instrMarker = instr;
                        // IL_0006: ldstr <methodName>
                        instr = ilProcessor.Create(OpCodes.Ldstr, unitTestName);
                        ilProcessor.InsertBefore(instrMarker, instr);
                        instrMarker = instr;
                    }

                    var spi = from i in meth.Body.Instructions
                                where i.SequencePoint != null
                                where i.SequencePoint.StartLine != 0xfeefee
                                select i;

                    var spId = 0;
                    var instructions = spi.ToArray();
                    foreach (var sp in instructions)
                    {
                        Instruction instrMarker = sp;
                        Instruction instr = null;
                        var ilProcessor = meth.Body.GetILProcessor();

                        // IL_000d: call void R4nd0mApps.TddStud10.TestHost.Marker::EnterSequencePoint(string, ldstr, ldstr)
                        instr = ilProcessor.Create(OpCodes.Call, enterSeqPointMethRef);
                        ilProcessor.InsertBefore(instrMarker, instr);
                        instrMarker = instr;
                        // IL_000b: ldstr <spid>
                        instr = ilProcessor.Create(OpCodes.Ldstr, (spId++).ToString());
                        ilProcessor.InsertBefore(instrMarker, instr);
                        instrMarker = instr;
                        // IL_0006: ldstr <mdtoken>
                        instr = ilProcessor.Create(OpCodes.Ldstr, meth.MetadataToken.RID.ToString());
                        ilProcessor.InsertBefore(instrMarker, instr);
                        instrMarker = instr;
                        // IL_0001: ldstr <mvid>
                        instr = ilProcessor.Create(OpCodes.Ldstr, module.Mvid.ToString());
                        ilProcessor.InsertBefore(instrMarker, instr);
                        instrMarker = instr;
                    }
                    meth.Body.OptimizeMacros();
                }

                var backupAssemblyPath = Path.ChangeExtension(assemblyPath, ".original");
                File.Delete(backupAssemblyPath);
                File.Move(assemblyPath, backupAssemblyPath);
                assembly.Write(assemblyPath, new WriterParameters { WriteSymbols = true });
            }

            using (StringWriter writer = new StringWriter())
            {
                DiscoveredUnitTests.Serializer.Serialize(writer, unitTests);
                File.WriteAllText(discoveredUnitTestsStore, writer.ToString());
            }
        }
    }
}
