using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace R4nd0mApps.TddStud10.TestHost
{
    public class DiscoveredUnitTests : SerializableDictionary<string, List<string>>
    {
        public DiscoveredUnitTests()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(DiscoveredUnitTests));
    }
}
