using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace R4nd0mApps.TddStud10.TestHost
{
    public class SequencePointSession : SerializableDictionary<string, List<SequencePoint>>
    {
        public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(SequencePointSession));
    }
}
