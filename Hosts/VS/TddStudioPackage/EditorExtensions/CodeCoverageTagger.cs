/*
Copyright (c) 2015 Raghavendra Nagaraj

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using R4nd0mApps.TddStud10.Engine;
using System.Linq;
using Microsoft.FSharp.Control;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Common.Domain;

namespace R4nd0mApps.TddStud10.Hosts.VS.EditorExtensions
{
    public sealed class CodeCoverageTagger : ITagger<CodeCoverageTag>, IDisposable
    {
        private ITextBuffer _buffer;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged = delegate { };
        private FSharpHandler<PerAssemblySequencePointsCoverage> _ciUpdatedEventHandler;

        public CodeCoverageTagger(ITextBuffer buffer)
        {
            _buffer = buffer;

            _ciUpdatedEventHandler = new FSharpHandler<PerAssemblySequencePointsCoverage>((_, __) => OnNewCoverageDataAvailable(null, new EventArgs()));
            DataStore.Instance.CoverageInfoUpdated += _ciUpdatedEventHandler;
        }

        public void Dispose()
        {
            _buffer = null;

            DataStore.Instance.CoverageInfoUpdated -= _ciUpdatedEventHandler;
        }

        IEnumerable<ITagSpan<CodeCoverageTag>> ITagger<CodeCoverageTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            return from span in spans
                   where !span.IsEmpty
                   select new TagSpan<CodeCoverageTag>(new SnapshotSpan(span.Start, 1), new CodeCoverageTag());
        }

        private void RaiseAllTagsChanged()
        {
            if (TagsChanged != null)
                TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(_buffer.CurrentSnapshot, 0, _buffer.CurrentSnapshot.Length)));
        }

        private void OnNewCoverageDataAvailable(object sender, EventArgs e)
        {
            RaiseAllTagsChanged();
        }
    }
}
