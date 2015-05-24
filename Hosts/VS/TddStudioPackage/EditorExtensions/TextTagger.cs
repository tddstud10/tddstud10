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
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Operations;
using R4nd0mApps.TddStud10.Hosts.VS.Glyphs;
using R4nd0mApps.TddStud10.Hosts.VS.Helper;

namespace R4nd0mApps.TddStud10.Hosts.VS.Tagger
{
    public sealed class TextTagger : TextViewCoverageProviderBase
    {
        private ITextSearchService _searchService;
        private IClassificationType _coveredType;
        private IClassificationType _notCoveredType;
        private IEnumerable<SnapshotSpan> _lineSpans;

        static Dictionary<ITextView, TextTagger> _instances = new Dictionary<ITextView, TextTagger>();

        public TextTagger(ITextView view, ITextSearchService searchService, IClassificationType coveredType, IClassificationType notCoveredType)
            : base(view)
        {
            if (TddStud10Package.Instance == null)
            {
                return;
            }

            _searchService = searchService;
            _coveredType = coveredType;
            _notCoveredType = notCoveredType;

            _instances.Add(view, this);

            view.Closed += OnViewClosed;
        }

        protected override void Dispose(bool disposing)
        {
            if (_textView != null)
                _textView.Closed -= OnViewClosed;

            _searchService = null;
            _coveredType = null;
            _notCoveredType = null;

            if (_instances.ContainsKey(_textView))
                _instances.Remove(_textView);

            base.Dispose(disposing);
        }

        public static TextTagger GetTagger(ITextView view)
        {
            if (_instances.ContainsKey(view))
                return _instances[view];
            else
                return null;
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            _instances.Remove(sender as ITextView);
        }

        public void ShowForLine(IWpfTextViewLine line)
        {
            _lineSpans = LineCoverageGlyphFactory.GetSpansForLine(line, _currentSpans);
            RaiseAllTagsChanged();
        }

        public void RemoveLineRestriction()
        {
            _lineSpans = null;
            RaiseAllTagsChanged();
        }
    }
}
