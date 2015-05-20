using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using R4nd0mApps.TddStud10.Common.Domain;

namespace R4nd0mApps.TddStud10.TestHost
{
    [Serializable]
    public class PerDocumentSequencePoints : SerializableDictionary<FilePath, List<SequencePoint>>
    {
        public static PerDocumentSequencePoints Deserialize(string file)
        {
            return Deserialize<PerDocumentSequencePoints>(file);
        }
    }
}
