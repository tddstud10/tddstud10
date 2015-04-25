using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R4nd0mApps.TddStud10
{
    public class SequencePointInfo
    {
        public Guid Mvid { get; set; }
        public string MdToken { get; set; }
        public string ID { get; set; }
        public string File { get; set; }
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
    }
}
