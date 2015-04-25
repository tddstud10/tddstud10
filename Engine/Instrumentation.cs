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

namespace R4nd0mApps.TddStud10
{
    /*
     * TODO:
     * √ what about maxstack - cecil seems to be adjusting it automatically, revisit if needed
     * √ disable jit opt in assembly - it is done pre-jit, we'll gen debug info if needed
     * 
     * √ Generate span information
     * - Generate coverage information
     *   √ Inject calls
     *   √ Inject call for non-feefee
     *   √ Inject call for both assemblies and backup/save
     *   √ Inject call by passing mvid/mdtoken/spid
     *   √ Execute test host and see console output
     *   √ C# and F#
     *   - Implement marker to collect coverage information [may run into appdomain issues]
     *   - Save coverage information
     * - Integrate with VS
     *   - Check for multi-line spans [with older code]
     *   - Read new span format
     *   - Merge with new coverage format
     * 
     */
    class Instrumentation
    {
        private static string testRunnerPath;

        //static void Main(string[] args)
        //{
        //    if (args[0] == "gen")
        //    {
        //        GenerateSequencePointInfo(@"d:\tddstud10\fizzbuzz.out\");
        //    }
        //    else // instrument
        //    {
        //        Instrument(@"d:\tddstud10\fizzbuzz.out\");
        //    }
        //}

        // TODO: Merge these 2 methods
        public static void GenerateSequencePointInfo(string buildOutputRoot)
        {
            var dict = new SerializableDictionary<string, List<SequencePointInfo>>();
            foreach (var assemblyPath in Directory.EnumerateFiles(buildOutputRoot, "*.dll"))
            {
                if (!File.Exists(Path.ChangeExtension(assemblyPath, ".pdb")))
                {
                    continue;                
                }

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
                        dict[sp.SequencePoint.Document.Url] = new List<SequencePointInfo>();
                    }

                    dict[sp.SequencePoint.Document.Url].Add(new SequencePointInfo
                    {
                        Mvid = sp.mod.Mvid,
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

            StringWriter writer = new StringWriter();

            XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, List<SequencePointInfo>>));
            serializer.Serialize(writer, dict);
            File.WriteAllText(Path.Combine(buildOutputRoot, "seqpoints.txt"), writer.ToString());
        }

        public static void Instrument(string buildOutputRoot)
        {
            string currFolder = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
            testRunnerPath = Path.Combine(Path.GetDirectoryName(currFolder), "TddStud10.TestHost.exe");

            foreach (var assemblyPath in Directory.EnumerateFiles(buildOutputRoot, "*.dll"))
            {
                if (!File.Exists(Path.ChangeExtension(assemblyPath, ".pdb")))
                {
                    continue;                
                }   

                var assembly = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters { ReadSymbols = true });

                // TODO: This path is hard coded, need to fix it.
                var marker = from t in ModuleDefinition.ReadModule(testRunnerPath).GetTypes()
                             where t.Name == "Marker"
                             from m in t.Methods
                             where m.Name == "EnterSequencePoint"
                             select m;

                /*
	            IL_0001: ldstr <mvid>
	            IL_0006: ldstr <mdtoken>
	            IL_000b: ldstr <spid>
	            IL_000d: call void R4nd0mApps.TddStud10.TestHost.Marker::EnterSequencePoint(string, ldstr, ldstr)
                 */
                MethodReference markerCall = assembly.MainModule.Import(marker.First());

                foreach (var module in assembly.Modules)
                foreach (var item in module.Types)
                {
                    foreach (MethodDefinition method in item.Methods)
                    {
                        if (method.Body == null)
                        {
                            continue;
                        }

                        var spi = from i in method.Body.Instructions
                                  where i.SequencePoint != null
                                  // TODO: Check for start/end/line/column 0xfeefee
                                  where i.SequencePoint.StartLine != 0xfeefee
                                  select i;

                        var spId = 0;
                        var instructions = spi.ToArray();
                        method.Body.SimplifyMacros();
                        foreach (var sp in instructions)
                        {
                            Instruction instrMarker = sp;
                            Instruction instr = null;
                            var ilProcessor = method.Body.GetILProcessor();

                            // IL_000d: call void R4nd0mApps.TddStud10.TestHost.Marker::EnterSequencePoint(string, ldstr, ldstr)
                            instr = ilProcessor.Create(OpCodes.Call, markerCall);
                            ilProcessor.InsertBefore(instrMarker, instr);
                            instrMarker = instr;
                            // IL_000b: ldstr <spid>
                            instr = ilProcessor.Create(OpCodes.Ldstr, (spId++).ToString());
                            ilProcessor.InsertBefore(instrMarker, instr);
                            instrMarker = instr;
                            // IL_0006: ldstr <mdtoken>
                            instr = ilProcessor.Create(OpCodes.Ldstr, method.MetadataToken.RID.ToString());
                            ilProcessor.InsertBefore(instrMarker, instr);
                            instrMarker = instr;
                            // IL_0001: ldstr <mvid>
                            instr = ilProcessor.Create(OpCodes.Ldstr, module.Mvid.ToString());
                            ilProcessor.InsertBefore(instrMarker, instr);
                            instrMarker = instr;
                            // TODO: Fxcop and other release build stuff
                        }
                        method.Body.OptimizeMacros();
                    }
                }

                var backupAssemblyPath = Path.ChangeExtension(assemblyPath, ".original");
                File.Delete(backupAssemblyPath);
                File.Move(assemblyPath, backupAssemblyPath);
                assembly.Write(assemblyPath, new WriterParameters { WriteSymbols = true });
            }
        }
    }
}
