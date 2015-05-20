using System;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using R4nd0mApps.TddStud10.Common.Domain;

namespace R4nd0mApps.TddStud10.TestHost
{
    [Serializable]
    public class PerTestIdResults : SerializableDictionary<TestId, TestOutcome>
    {
        public static PerTestIdResults Deserialize(string file)
        {
            return Deserialize<PerTestIdResults>(file);
        }
    }
}
