using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using R4nd0mApps.TddStud10.Hosts.VS.Helpers;
//using R4nd0mApps.TddStud10.Hosts.VS.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using R4nd0mApps.TddStud10.Hosts.VS;
using Server;
using R4nd0mApps.TddStud10.Engine;

namespace R4nd0mApps.TddStud10.Hosts.VS.Helper
{
    /// <summary>
    /// Base class to provide coverage information for the text view.
    /// </summary>
    public class TextViewCoverageProviderBase : IDisposable
    {
        /// <summary>
        /// The current editor view.
        /// </summary>
        protected ITextView _textView;

        /// <summary>
        /// Coverage info for spans.
        /// </summary>
        protected readonly Dictionary<SnapshotSpan, CovData> _spanCoverage;

        /// <summary>
        /// Span for current editor content.
        /// </summary>
        protected List<SnapshotSpan> _currentSpans;

        /// <summary>
        /// Occurs when tags are changed.
        /// </summary>
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged = delegate { };

        /// <summary>
        /// Initializes a new instance of the <see cref="TextViewCoverageProviderBase"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        public TextViewCoverageProviderBase(ITextView view)
        {
            if (TddStud10Package.Instance == null)
            {
                return;
            }

            _textView = view;

            _spanCoverage = new Dictionary<SnapshotSpan, CovData>();

            _currentSpans = GetWordSpans(_textView.TextSnapshot);

            _textView.GotAggregateFocus += SetupSelectionChangedListener;
            CoverageData.Instance.NewCoverageDataAvailable += OnNewCoverageDataAvailable;
        }       

        /// <summary>
        /// Disposes the base class
        /// </summary>
        public void Dispose()
        {           
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the base class
        /// <param name="disposing">True for clean up managed ressources.</param>
        /// </summary>
        protected virtual void Dispose(bool disposing)
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

        /// <summary>
        /// Setups the selection changed listener.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SetupSelectionChangedListener(object sender, EventArgs e)
        {
            if (_textView != null)
            {
                _textView.LayoutChanged += ViewLayoutChanged;
                _textView.GotAggregateFocus -= SetupSelectionChangedListener;
            }
        }

        /// <summary>
        /// Updates tags when the view layout is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextViewLayoutChangedEventArgs"/> instance containing the event data.</param>
        private void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (e.OldSnapshot != e.NewSnapshot)
            {
                _currentSpans = GetWordSpans(e.NewSnapshot);
                TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(e.NewSnapshot, 0, e.NewSnapshot.Length)));
            }
        }

        /// <summary>
        /// Tell the editor that the tags in the whole buffer changed. It will call back into GetTags().
        /// </summary>
        protected void RaiseAllTagsChanged()
        {
            if (TagsChanged != null)
                TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(_textView.TextBuffer.CurrentSnapshot, 0, _textView.TextBuffer.CurrentSnapshot.Length)));
        }

        /// <summary>
        /// Will be called when new data is available
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnNewCoverageDataAvailable(object sender, EventArgs e)
        {
            // update spans
            _spanCoverage.Clear();
            _currentSpans = GetWordSpans(_textView.TextBuffer.CurrentSnapshot);

            RaiseAllTagsChanged();
        }

        /// <summary>
        /// Adds the word span.
        /// </summary>
        /// <param name="wordSpans">The word spans.</param>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="startColumn">The start column.</param>
        /// <param name="totalCharacters">The total characters.</param>
        /// <param name="covered">if set to <c>true</c> [covered].</param>
        protected void AddWordSpan(List<SnapshotSpan> wordSpans, ITextSnapshot snapshot, int startColumn, int totalCharacters, CovData covered)
        {
            var snapshotPoint = new SnapshotSpan(snapshot, new Span(startColumn, totalCharacters));
            wordSpans.Add(snapshotPoint);

            if (!_spanCoverage.ContainsKey(snapshotPoint))
            {
                _spanCoverage.Add(snapshotPoint, covered); 
            }
        }       

        public class CovData
        {
            public IEnumerable<string> TrackedMethods { get; set; }

            public CovData()
            {
                TrackedMethods = new string[0];
            }
        }

        /// <summary>
        /// Returns the word spans based on covered lines.
        /// </summary>
        /// <param name="snapshot">The text snapshot of file being opened.</param>
        /// <returns>Collection of word spans</returns>
        protected List<SnapshotSpan> GetWordSpans(ITextSnapshot snapshot)
        {
            var wordSpans = new List<SnapshotSpan>();
            
            // Get covered sequence points
            try
            {
                var sequencePoints = GetSequencePointsForActiveDocument();

                if (sequencePoints != null)
                {
                    foreach (var sequencePoint in sequencePoints)
                    {
                        var covData = new CovData();
                        covData.TrackedMethods = CoverageData.Instance.GetUnitTestsCoveringSequencePoint(sequencePoint);

                        int sequencePointStartLine = sequencePoint.StartLine - 1;
                        int sequencePointEndLine = sequencePoint.EndLine - 1;

                        var startLine = snapshot.Lines.FirstOrDefault(line => line.LineNumber == sequencePointStartLine);

                        if (sequencePoint.EndLine == sequencePoint.StartLine)
                        {
                            AddWordSpan(wordSpans, snapshot,
                                        startLine.Extent.Start.Position + sequencePoint.StartColumn - 1,
                                        sequencePoint.EndColumn - sequencePoint.StartColumn + 1, covData);
                        }
                        else
                        {
                            // Get selected lines
                            AddWordSpansForSequencePointsCoveringMultipleLines(snapshot, wordSpans, sequencePoint,
                                                                                sequencePointStartLine, sequencePointEndLine, covData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                IDEHelper.WriteToOutputWindow(ex.Message);
                IDEHelper.WriteToOutputWindow(ex.StackTrace);
            }
            

            return (wordSpans);
        }

        /// <summary>
        /// Adds the word spans for sequence points covering multiple lines.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="wordSpans">The word spans.</param>
        /// <param name="sequencePoint">The sequence point.</param>
        /// <param name="sequencePointStartLine">The sequence point start line.</param>
        /// <param name="sequencePointEndLine">The sequence point end line.</param>
        /// <param name="covered">if set to <c>true</c> [covered].</param>
        protected void AddWordSpansForSequencePointsCoveringMultipleLines(ITextSnapshot snapshot, 
                                                                        List<SnapshotSpan> wordSpans, 
                                                                        SequencePoint sequencePoint, 
                                                                        int sequencePointStartLine, 
                                                                        int sequencePointEndLine,
                                                                        CovData covered)
        {
            int totalCharacters = 0;

            var selectedLines = snapshot.Lines
                                        .Where(line => line.LineNumber >= sequencePointStartLine &&
                                                        line.LineNumber <= sequencePointEndLine);

            foreach (var selectedLine in selectedLines)
            {
                if (selectedLine.LineNumber == sequencePointStartLine)
                {
                    totalCharacters = selectedLine.Length - sequencePoint.StartColumn + 1;

                    AddWordSpan(wordSpans, snapshot, selectedLine.Extent.Start.Position + sequencePoint.StartColumn - 1, totalCharacters, covered);
                }
                else if (selectedLine.LineNumber == sequencePointEndLine)
                {
                    var temp = selectedLine.Length - (sequencePoint.EndColumn - 1);
                    totalCharacters = selectedLine.Length - temp;

                    AddWordSpan(wordSpans, snapshot, selectedLine.Extent.Start.Position, totalCharacters, covered);
                }
                else
                {
                    AddWordSpan(wordSpans, snapshot, selectedLine.Extent.Start.Position, selectedLine.Length, covered);
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
                var selectedFile = allFiles.FirstOrDefault(file => Engine.Engine.Instance != null && Engine.Engine.Instance.ArePathsTheSame(file, fileName));
                if (selectedFile != null)
                {
                    sequencePoints = allSequencePoints.Where(sp => sp != null && sp.File == selectedFile);
                }
            }

            return sequencePoints;
        }

        /// <summary>
        /// Gets the coverage infor (sequence points) for the editor's document
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<SequencePoint> GetSequencePointsForActiveDocument()
        {
            // Get the sequence points of the current file
            if (CoverageData.Instance.CoverageSession != null)
                return GetSequencePoints(CoverageData.Instance, IDEHelper.GetFileName(_textView));
            else
                return new List<SequencePoint>();
        }        
    }
}
