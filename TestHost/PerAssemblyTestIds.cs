using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using R4nd0mApps.TddStud10.Common.Domain;

namespace R4nd0mApps.TddStud10.TestHost
{
    public class PerAssemblyTestIds : SerializableDictionary<FilePath, List<TestId>>
    {
        public PerAssemblyTestIds()
        {
        }

        public PerAssemblyTestIds(IEnumerable<KeyValuePair<FilePath, List<TestId>>> collection)
            : base(collection)
        {
        }

        public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(PerAssemblyTestIds));
    }
}
