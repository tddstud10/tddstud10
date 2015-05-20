using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using R4nd0mApps.TddStud10.Common.Domain;

namespace R4nd0mApps.TddStud10.Engine
{
    [Serializable]
    public class PerAssemblySequencePointsCoverage : SerializableDictionary<AssemblyId, List<SequencePointCoverage>>
    {
        public static PerAssemblySequencePointsCoverage Deserialize(string file)
        {
            return Deserialize<PerAssemblySequencePointsCoverage>(file);
        }
    }
}
