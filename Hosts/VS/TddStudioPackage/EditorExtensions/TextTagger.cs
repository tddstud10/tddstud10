/*
Copyright (c) 2015 Raghavendra Nagaraj

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

//using R4nd0mApps.TddStud10.Hosts.VS.Views;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using R4nd0mApps.TddStud10.Hosts.VS.Glyphs;
using R4nd0mApps.TddStud10.Hosts.VS.Helper;

namespace R4nd0mApps.TddStud10.Hosts.VS.Tagger
{
    /// <summary>
    /// Text tagger to produce tags to change background color for covered lines
    /// </summary>
    public sealed class TextTagger : TextViewCoverageProviderBase//, ITagger<ClassificationTag>
    {
        private ITextSearchService _searchService;
        private IClassificationType _coveredType;
        private IClassificationType _notCoveredType;
        private IEnumerable<SnapshotSpan> _lineSpans;

        static Dictionary<ITextView, TextTagger> _instances = new Dictionary<ITextView, TextTagger>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TextTagger"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="searchService">The search service.</param>
        /// <param name="coveredType">The type.</param>
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

            // Register instance of the view
            _instances.Add(view, this);

            view.Closed += OnViewClosed;
        }

        /// <summary>
        /// Disposes the tagger
        /// </summary>
        /// <param name="disposing">True for managed ressources</param>
        protected override void Dispose(bool disposing)
        {
            if (_textView != null)
                _textView.Closed -= OnViewClosed;

            _searchService = null;
            _coveredType = null;
            _notCoveredType = null;

            // Unregister this instance
            if (_instances.ContainsKey(_textView))
                _instances.Remove(_textView);

            base.Dispose(disposing);
        }

        /// <summary>
        /// Generates tags based on Coverage information.
        /// </summary>
        /// <param name="spans">The spans.</param>
        /// <returns>Tags for the current src based on coverage information</returns>
        //public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        //{
        //    if (_currentSpans == null || _currentSpans.Count == 0 || (!TddStud10Package.Instance.Settings.ShowLinesColored && _lineSpans == null))
        //        yield break;

        //    var spansToSerach = _lineSpans ?? _currentSpans;

        //    foreach (var span in spansToSerach)
        //    {
        //        var covered = _spanCoverage.ContainsKey(span) ? _spanCoverage[span].TrackedMethods.Any() : false;
        //        var tag = covered ? new ClassificationTag(_coveredType) : new ClassificationTag(_notCoveredType);
        //        yield return new TagSpan<ClassificationTag>(span, tag);
        //    }
        //}

        /// <summary>
        /// Gets the tagger instance for the specified view.
        /// </summary>
        /// <param name="view">View to retrieve the tagger instance.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Show spans for message only
        /// </summary>
        /// <param name="message"></param>
        internal void ShowForLine(Microsoft.VisualStudio.Text.Formatting.IWpfTextViewLine line)
        {
            _lineSpans = LineCoverageGlyphFactory.GetSpansForLine(line, _currentSpans);
            RaiseAllTagsChanged();
        }

        /// <summary>
        /// Show all spans again
        /// </summary>
        internal void RemoveLineRestriction()
        {
            _lineSpans = null;
            RaiseAllTagsChanged();
        }
    }
}
