using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using R4nd0mApps.TddStud10.Engine;

namespace R4nd0mApps.TddStud10.TestHost
{
    public enum TestResult
    {
        Failed,
        Skipped,
        Passed,
    }

    public class TestDetail
    {
        public string Method { get; set; }

        public string ReturnType { get; set; }

        public string Class { get; set; }

        public string Assembly { get; set; }

        public string Name { get; set; }
    }

    public class TestDetails
    {
        public SerializableDictionary<string, TestResult> Dictionary
        {
            get;
            set;
        }

        public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(TestDetails));

        public TestDetails()
        {
            Dictionary = new SerializableDictionary<string, TestResult>();
        }
    }
}
