using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R4nd0mApps.TddStud10.Engine
{
    public class MethodId
    {
        public string Mvid { get; set; }
        public string MdToken { get; set; }
    
        public override bool Equals(object obj)
        {
            var obj2 = obj as MethodId;
            if (obj2 == null) return false;
            return this.Mvid == obj2.Mvid && this.MdToken == obj2.MdToken;
        }

        public override int GetHashCode()
        {
            return this.MdToken.GetHashCode() ^ this.Mvid.GetHashCode();
        }
    }

    public class CoverageHitInfo
    {
        public MethodId Method { get; set; }
        public string SpId { get; set; }
        public string UnitTest { get; set; }
    }

    public class CoverageSession : SerializableDictionary<string, List<CoverageHitInfo>>
    {
    }
}
