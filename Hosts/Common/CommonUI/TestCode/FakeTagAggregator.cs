using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Tagging;

namespace R4nd0mApps.TddStud10.Hosts.Common.TestCode
{
    public class FakeTagAggregator<T> : ITagAggregator<T> where T : ITag
    {
        #region ITagAggregator<T> Members

        public event EventHandler<BatchedTagsChangedEventArgs> BatchedTagsChanged
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.Projection.IBufferGraph BufferGraph
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IMappingTagSpan<T>> GetTags(Microsoft.VisualStudio.Text.NormalizedSnapshotSpanCollection snapshotSpans)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMappingTagSpan<T>> GetTags(Microsoft.VisualStudio.Text.IMappingSpan span)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMappingTagSpan<T>> GetTags(Microsoft.VisualStudio.Text.SnapshotSpan span)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<TagsChangedEventArgs> TagsChanged;

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        public void FireTagsChangedEvent()
        {
            var handler = TagsChanged;
            if (handler != null)
            {
                handler(this, null);
            }
        }
    }
}
