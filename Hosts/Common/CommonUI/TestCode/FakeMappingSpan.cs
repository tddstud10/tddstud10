using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;

namespace R4nd0mApps.TddStud10.Hosts.Common.TestCode
{
    public class FakeMappingSpan : IMappingSpan
    {
        #region IMappingSpan Members

        public ITextBuffer AnchorBuffer
        {
            get { throw new NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.Projection.IBufferGraph BufferGraph
        {
            get { throw new NotImplementedException(); }
        }

        public IMappingPoint End
        {
            get { throw new NotImplementedException(); }
        }

        public NormalizedSnapshotSpanCollection GetSpans(Predicate<ITextBuffer> match)
        {
            throw new NotImplementedException();
        }

        public NormalizedSnapshotSpanCollection GetSpans(ITextSnapshot targetSnapshot)
        {
            throw new NotImplementedException();
        }

        public NormalizedSnapshotSpanCollection GetSpans(ITextBuffer targetBuffer)
        {
            throw new NotImplementedException();
        }

        public IMappingPoint Start
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
