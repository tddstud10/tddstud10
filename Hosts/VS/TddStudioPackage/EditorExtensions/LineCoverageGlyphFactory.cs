/*
Copyright (c) 2015 Raghavendra Nagaraj

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Engine;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Hosts.Common;
using R4nd0mApps.TddStud10.Hosts.VS.Diagnostics;

namespace R4nd0mApps.TddStud10.Hosts.VS.EditorExtensions
{
    public class LineCoverageGlyphFactory : IGlyphFactory
    {
        const double _glyphSize = 8.0;

        private static Brush _uncoveredBrush = new SolidColorBrush(RunStateToIconColorConverter.ColorForUnknown);
        private static Brush _coveredWithPassingTestBrush = new SolidColorBrush(RunStateToIconColorConverter.ColorForGreen);
        private static Brush _coveredWithFailedTestBrush = new SolidColorBrush(RunStateToIconColorConverter.ColorForRed);

        private ITextView _textView;

        private readonly Dictionary<SnapshotSpan, IEnumerable<TestRunId>> _spanCoverage;

        private List<SnapshotSpan> _currentSpans;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged = delegate { };

        public LineCoverageGlyphFactory(IWpfTextView view)
        {
            if (TddStud10Package.Instance == null)
            {
                return;
            }

            _textView = view;

            _spanCoverage = new Dictionary<SnapshotSpan, IEnumerable<TestRunId>>();

            _currentSpans = GetWordSpans(_textView.TextSnapshot);

            _textView.GotAggregateFocus += SetupSelectionChangedListener;
            CoverageData.Instance.NewCoverageDataAvailable += OnNewCoverageDataAvailable;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag)
        {
            LineCoverageState state = GetLineCoverageState(line);

            if (state == LineCoverageState.Unknown)
                return null;

            var brush = GetBrushForState(state);

            if (brush == null)
                return null;

            Ellipse ellipse = new Ellipse();
            ellipse.Fill = brush;
            ellipse.Height = _glyphSize;
            ellipse.Width = _glyphSize;

            ellipse.ToolTip = GetToolTipText(state);

            return ellipse;
        }

        private Brush GetBrushForState(LineCoverageState state)
        {
            switch (state)
            {
                case LineCoverageState.CoveredWithPassingTests:
                    return _coveredWithPassingTestBrush;
                case LineCoverageState.CoveredWithAtleastOneFailedTest:
                    return _coveredWithFailedTestBrush;
                case LineCoverageState.Uncovered:
                    return _uncoveredBrush;
            }

            return null;
        }

        private string GetToolTipText(LineCoverageState state)
        {
            switch (state)
            {
                case LineCoverageState.CoveredWithPassingTests:
                    return "This message is covered by passing tests.";
                case LineCoverageState.CoveredWithAtleastOneFailedTest:
                    return "This message is covered by at least one failing test.";
                case LineCoverageState.Uncovered:
                    return "This message is not covered by any test.";
            }

            return null;
        }

        private LineCoverageState GetLineCoverageState(ITextViewLine line)
        {
            var spans = GetSpansForLine(line, _currentSpans);

            if (spans.Any())
            {
                var allTrackedMethods = spans.SelectMany(s => _spanCoverage[s]);
                if (!allTrackedMethods.Any())
                {
                    return LineCoverageState.Uncovered;
                }

                var results = from tm in allTrackedMethods
                              from tr in CoverageData.Instance.PerTestIdResults[tm.testId]
                              select tr;

                if (results.Any(r => r.result.Outcome == TestOutcome.Failed))
                {
                    return LineCoverageState.CoveredWithAtleastOneFailedTest;
                }

                if (results.All(r => r.result.Outcome == TestOutcome.Passed))
                {
                    return LineCoverageState.CoveredWithPassingTests;
                }
            }

            return LineCoverageState.Unknown;
        }

        public static IEnumerable<SnapshotSpan> GetSpansForLine(ITextViewLine line, IEnumerable<SnapshotSpan> spanContainer)
        {
            return spanContainer.Where(s => (s.Snapshot == line.Snapshot) && ((s.Start >= line.Start && s.Start <= line.End) || (s.Start < line.Start && s.End >= line.Start)));
        }

        private void Dispose(bool disposing)
        {
            if (_textView != null)
            {
                _textView.GotAggregateFocus -= SetupSelectionChangedListener;
                _textView.LayoutChanged -= ViewLayoutChanged;
            }

            CoverageData.Instance.NewCoverageDataAvailable -= OnNewCoverageDataAvailable;

            _textView = null;
            _spanCoverage.Clear();
            _currentSpans.Clear();
        }

        private void SetupSelectionChangedListener(object sender, EventArgs e)
        {
            if (_textView != null)
            {
                _textView.LayoutChanged += ViewLayoutChanged;
                _textView.GotAggregateFocus -= SetupSelectionChangedListener;
            }
        }

        private void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (e.OldSnapshot != e.NewSnapshot)
            {
                _currentSpans = GetWordSpans(e.NewSnapshot);
                TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(e.NewSnapshot, 0, e.NewSnapshot.Length)));
            }
        }

        private void RaiseAllTagsChanged()
        {
            if (TagsChanged != null)
                TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(_textView.TextBuffer.CurrentSnapshot, 0, _textView.TextBuffer.CurrentSnapshot.Length)));
        }

        private void OnNewCoverageDataAvailable(object sender, EventArgs e)
        {
            _spanCoverage.Clear();
            _currentSpans = GetWordSpans(_textView.TextBuffer.CurrentSnapshot);

            RaiseAllTagsChanged();
        }

        private void AddWordSpan(List<SnapshotSpan> wordSpans, ITextSnapshot snapshot, int startColumn, int totalCharacters, IEnumerable<TestRunId> trackedMethods)
        {
            var snapshotPoint = new SnapshotSpan(snapshot, new Span(startColumn, totalCharacters));
            wordSpans.Add(snapshotPoint);

            if (!_spanCoverage.ContainsKey(snapshotPoint))
            {
                _spanCoverage.Add(snapshotPoint, trackedMethods);
            }
        }

        private List<SnapshotSpan> GetWordSpans(ITextSnapshot snapshot)
        {
            var wordSpans = new List<SnapshotSpan>();

            try
            {
                var sequencePoints = GetSequencePointsForActiveDocument();

                if (sequencePoints != null)
                {
                    foreach (var sequencePoint in sequencePoints)
                    {
                        var trackedMethods = CoverageData.Instance.GetUnitTestsCoveringSequencePoint(sequencePoint);

                        int sequencePointStartLine = sequencePoint.startLine.Item - 1;
                        int sequencePointEndLine = sequencePoint.endLine.Item - 1;

                        var startLine = snapshot.Lines.FirstOrDefault(line => line.LineNumber == sequencePointStartLine);

                        if (sequencePoint.endLine.Equals(sequencePoint.startLine))
                        {
                            AddWordSpan(wordSpans, snapshot,
                                        startLine.Extent.Start.Position + sequencePoint.startColumn.Item - 1,
                                        sequencePoint.endColumn.Item - sequencePoint.startColumn.Item + 1, trackedMethods);
                        }
                        else
                        {
                            AddWordSpansForSequencePointsCoveringMultipleLines(snapshot, wordSpans, sequencePoint,
                                                                                sequencePointStartLine, sequencePointEndLine, trackedMethods);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.I.LogError(ex.Message);
                Logger.I.LogError(ex.StackTrace);
            }


            return (wordSpans);
        }

        private void AddWordSpansForSequencePointsCoveringMultipleLines(ITextSnapshot snapshot,
                                                                        List<SnapshotSpan> wordSpans,
                                                                        SequencePoint sequencePoint,
                                                                        int sequencePointStartLine,
                                                                        int sequencePointEndLine,
                                                                        IEnumerable<TestRunId> trackedMethods)
        {
            int totalCharacters = 0;

            var selectedLines = snapshot.Lines
                                        .Where(line => line.LineNumber >= sequencePointStartLine &&
                                                        line.LineNumber <= sequencePointEndLine);

            foreach (var selectedLine in selectedLines)
            {
                if (selectedLine.LineNumber == sequencePointStartLine)
                {
                    totalCharacters = selectedLine.Length - sequencePoint.startColumn.Item + 1;

                    AddWordSpan(wordSpans, snapshot, selectedLine.Extent.Start.Position + sequencePoint.startColumn.Item - 1, totalCharacters, trackedMethods);
                }
                else if (selectedLine.LineNumber == sequencePointEndLine)
                {
                    var temp = selectedLine.Length - (sequencePoint.endColumn.Item - 1);
                    totalCharacters = selectedLine.Length - temp;

                    AddWordSpan(wordSpans, snapshot, selectedLine.Extent.Start.Position, totalCharacters, trackedMethods);
                }
                else
                {
                    AddWordSpan(wordSpans, snapshot, selectedLine.Extent.Start.Position, selectedLine.Length, trackedMethods);
                }
            }
        }

        public IEnumerable<SequencePoint> GetSequencePoints(CoverageData coverageData, string fileName)
        {
            var allSequencePoints = coverageData.GetSequencePoints();
            var allFiles = coverageData.GetFiles();
            IEnumerable<SequencePoint> sequencePoints = null;

            if (allFiles != null && allSequencePoints != null)
            {
                // NOTE: fileName comes out NULL sometimes. i.e. ITextBuffer does not have the ITextDocument property.
                // Not sure why that happens, but it doesnt seem to impact anything. TB fixed later once we finalize the
                // design for this layer.
                var selectedFile = allFiles.FirstOrDefault(file => fileName != null && PathBuilder.arePathsTheSame(TddStud10Package.Instance.GetSolutionPath(), file.Item, fileName));
                if (selectedFile != null)
                {
                    sequencePoints = allSequencePoints.Where(sp => sp.document.Equals(selectedFile));
                }
            }

            return sequencePoints;
        }

        private IEnumerable<SequencePoint> GetSequencePointsForActiveDocument()
        {
            if (CoverageData.Instance.PerAssemblySequencePointsCoverage != null)
                return GetSequencePoints(CoverageData.Instance, GetFileName(_textView));
            else
                return new List<SequencePoint>();
        }

        public static string GetFileName(ITextView view)
        {
            ITextBuffer TextBuffer = view.TextBuffer;

            ITextDocument TextDocument = GetTextDocument(TextBuffer);

            if (TextDocument == null || TextDocument.FilePath == null || TextDocument.FilePath.Equals("Temp.txt"))
            {
                return null;
            }

            return TextDocument.FilePath;
        }

        private static ITextDocument GetTextDocument(ITextBuffer TextBuffer)
        {
            if (TextBuffer == null)
                return null;

            ITextDocument textDoc;
            var rc = TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out textDoc);

            if (rc == true)
                return textDoc;
            else
                return null;
        }
    }
}
