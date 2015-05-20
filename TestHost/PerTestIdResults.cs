using System.Xml.Serialization;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using R4nd0mApps.TddStud10.Common.Domain;

namespace R4nd0mApps.TddStud10.TestHost
{
    public class PerTestIdResults : SerializableDictionary<TestId, TestOutcome>
    {
        public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(PerTestIdResults));
    }
}
