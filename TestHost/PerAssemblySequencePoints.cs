using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using R4nd0mApps.TddStud10.Common.Domain;

namespace R4nd0mApps.TddStud10.TestHost
{
    public class PerAssemblySequencePoints : SerializableDictionary<FilePath, List<SequencePoint>>
    {
        public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(PerAssemblySequencePoints));
    }
}
