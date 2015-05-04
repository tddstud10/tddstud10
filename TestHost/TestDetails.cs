using System.Xml.Serialization;

namespace R4nd0mApps.TddStud10.TestHost
{
    public enum TestResult
    {
        Failed,
        Skipped,
        Passed,
    }

    public class TestId
    {
        public string Name { get; set; }
    }

    public class TestResults : SerializableDictionary<string, TestResult>
    {
        public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(TestResults));
    }
}
