using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using R4nd0mApps.TddStud10.Common.Domain;

namespace R4nd0mApps.TddStud10.Engine
{
    public class PerAssemblySequencePointsCoverage : SerializableDictionary<AssemblyId, List<SequencePointCoverage>>
    {
        public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(PerAssemblySequencePointsCoverage));
    }
}
