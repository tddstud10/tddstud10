using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using R4nd0mApps.TddStud10.Engine.Diagnostics;
using R4nd0mApps.TddStud10.TestHost;

namespace R4nd0mApps.TddStud10
{
    internal class Instrumentation
    {
        public static SequencePoints GenerateSequencePointInfo(DateTime timeFilter, string buildOutputRoot)
        {
            try
            {
                return GenerateSequencePointInfoImpl(timeFilter, buildOutputRoot);
            }
            catch (Exception e)
            {
                Logger.I.LogError("Failed to instrument. Exception: {0}", e);
            }

            return null;
        }

        public static SequencePoints GenerateSequencePointInfoImpl(DateTime timeFilter, string buildOutputRoot)
        {
            Logger.I.LogInfo(
                "Generating sequence point info: Time filter - {0}, Build output root - {1}.",
                timeFilter.ToLocalTime(),
                buildOutputRoot);

            var dict = new SequencePoints();
            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".dll", ".exe" };
            foreach (var assemblyPath in Directory.EnumerateFiles(buildOutputRoot, "*").Where(s => extensions.Contains(Path.GetExtension(s))))
            {
                if (!File.Exists(Path.ChangeExtension(assemblyPath, ".pdb")))
                {
                    continue;
                }

                var lastWriteTime = File.GetLastWriteTimeUtc(assemblyPath);
                if (lastWriteTime < timeFilter)
                {
                    continue;
                }

                Logger.I.LogInfo("Generating sequence point info for {0}. Last write time: {1}.", assemblyPath, lastWriteTime.ToLocalTime());

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

            return dict;
        }

        public static DiscoveredUnitTests Instrument(DateTime timeFilter, string solutionRoot, string buildOutputRoot)
        {
            try
            {
                return InstrumentImpl(timeFilter, solutionRoot, buildOutputRoot);
            }
            catch (Exception e)
            {
                Logger.I.LogError("Failed to instrument. Exception: {0}", e);
            }

            return null;
        }

        public static DiscoveredUnitTests InstrumentImpl(DateTime timeFilter, string solutionRoot, string buildOutputRoot)
        {
            Logger.I.LogInfo(
                "Instrumenting: Time filter - {0}, Build output root - {1}.",
                timeFilter,
                buildOutputRoot);

            StrongNameKeyPair snKeyPair = null;
            var snKeyFile = Directory.EnumerateFiles(solutionRoot, "*.snk").FirstOrDefault();
            if (snKeyFile != null)
            {
                snKeyPair = new StrongNameKeyPair(File.ReadAllBytes(snKeyFile));
                Logger.I.LogInfo("Using strong name from {0}.", snKeyFile);
            }

            string testRunnerPath = Path.GetFullPath(typeof(R4nd0mApps.TddStud10.TestHost.Marker).Assembly.Location);

            var unitTests = new DiscoveredUnitTests();

            var asmResolver = new DefaultAssemblyResolver();
            Array.ForEach(asmResolver.GetSearchDirectories(), asmResolver.RemoveSearchDirectory);
            asmResolver.AddSearchDirectory(buildOutputRoot);
            var readerParams = new ReaderParameters
            {
                AssemblyResolver = asmResolver,
                ReadSymbols = true,
            };

            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".dll", ".exe" };
            foreach (var assemblyPath in Directory.EnumerateFiles(buildOutputRoot, "*").Where(s => extensions.Contains(Path.GetExtension(s))))
            {
                if (!File.Exists(Path.ChangeExtension(assemblyPath, ".pdb")))
                {
                    continue;
                }

                var lastWriteTime = File.GetLastWriteTimeUtc(assemblyPath);
                if (lastWriteTime < timeFilter)
                {
                    continue;
                }

                Logger.I.LogInfo("Instrumenting {0}. Last write time: {1}.", assemblyPath, lastWriteTime.ToLocalTime());

                var assembly = AssemblyDefinition.ReadAssembly(assemblyPath, readerParams);

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
                {
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
                }

                var backupAssemblyPath = Path.ChangeExtension(assemblyPath, ".original");
                File.Delete(backupAssemblyPath);
                File.Move(assemblyPath, backupAssemblyPath);
                try
                {
                    assembly.Write(assemblyPath, new WriterParameters { WriteSymbols = true, StrongNameKeyPair = snKeyPair });
                }
                catch
                {
                    Logger.I.LogInfo("Backing up or instrumentation failed. Attempting to revert back changes to {0}.", assemblyPath);
                    File.Delete(assemblyPath);
                    File.Move(backupAssemblyPath, assemblyPath);
                    throw;
                }
            }

            return unitTests;
        }
    }
}
