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

namespace R4nd0mApps.TddStud10.Hosts.VS.Tagger
{
    public sealed class LineCoverageTagger : ITagger<LineCoverageTag>, IDisposable
    {
        private IClassifier _classifier;
        private ITextBuffer _buffer;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged = delegate { };

        public LineCoverageTagger(ITextBuffer buffer, IClassifier classifier)
        {
            _classifier = classifier;
            _buffer = buffer;

            CoverageData.Instance.NewCoverageDataAvailable += OnNewCoverageDataAvailable;
        }

        public void Dispose()
        {
            _classifier = null;
            _buffer = null;

            CoverageData.Instance.NewCoverageDataAvailable -= OnNewCoverageDataAvailable;
        }

        IEnumerable<ITagSpan<LineCoverageTag>> ITagger<LineCoverageTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan span in spans)
            {
                foreach (ClassificationSpan classification in _classifier.GetClassificationSpans(span))
                {
                    var classificationString = classification.ClassificationType.Classification.ToLower();
                    if (!classificationString.Contains("xml doc") && !classificationString.Contains("comment"))
                    {
                        yield return new TagSpan<LineCoverageTag>(new SnapshotSpan(classification.Span.Start, 1), new LineCoverageTag());
                    }
                }
            }
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
