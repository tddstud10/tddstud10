using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
//using R4nd0mApps.TddStud10.Hosts.VS.Views;
using R4nd0mApps.TddStud10.Hosts.VS.Helpers;
using OpenCover.Framework.Model;
using R4nd0mApps.TddStud10.Hosts.VS.Helper;
using R4nd0mApps.TddStud10.Hosts.VS.Tagger;
using R4nd0mApps.TddStud10.Hosts.VS;

namespace R4nd0mApps.TddStud10.Hosts.VS.Glyphs
{
    /// <summary>
    /// Factory for creating the line covergage glyphs.
    /// </summary>
    public class LineCoverageGlyphFactory : TextViewCoverageProviderBase, IGlyphFactory
    {
        const double _glyphSize = 8.0;
        
        // TODO: Show lines when they go out of date
        // TODO: Put a question mark on unknown brush
        private static Brush _uncoveredBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        private static Brush _coveredWithPassingTestBrush = new SolidColorBrush(Color.FromRgb(0, 171, 0));
        private static Brush _coveredWithFailedTestBrush = new SolidColorBrush(Color.FromRgb(171, 0, 0));

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="view">The current text editor.</param>
        public LineCoverageGlyphFactory(IWpfTextView view) :base(view)
        {
        }

        /// <summary>
        /// Create the glyph element.
        /// </summary>
        /// <param name="line">Editor line to create the glyph for.</param>
        /// <param name="tag">The corresponding tag.</param>
        /// <returns></returns>
        public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag)
        {
            // get the coverage info for the current line
            LineCoverageState state = GetLineCoverageState(line);

            // no coverage info found -> exit here
            if (state == LineCoverageState.Unknown)
                return null;

            var brush = GetBrushForState(state);

            if (brush == null)
                return null;

            System.Windows.Shapes.Ellipse ellipse = new Ellipse();
            ellipse.Fill = brush;
            ellipse.Height = _glyphSize;
            ellipse.Width = _glyphSize;

            // TODO: Tooltip should have a list of tests covering this line.
            ellipse.ToolTip = GetToolTipText(state);

            if (state == LineCoverageState.Uncovered)
            {
                ellipse.MouseEnter += OnGlyphMouseEnter;
                ellipse.MouseLeave += OnGlyphMouseLeave;
                ellipse.Tag = line;
            }

            return ellipse;
        }            

        /// <summary>
        /// Determines the correct brush for the coverage state.
        /// </summary>
        /// <param name="state">The line coverage state.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Determines the tooltip text the coverage state.
        /// </summary>
        /// <param name="state">The line coverage state.</param>
        /// <returns></returns>
        private string GetToolTipText(LineCoverageState state)
        {
            // TODO: Nicely list out the failed and passing methods, not just these
            switch (state)
            {
                case LineCoverageState.CoveredWithPassingTests:
                    return "This line is covered by passing tests.";
                case LineCoverageState.CoveredWithAtleastOneFailedTest:
                    return "This line is covered by at least one failing test.";
                case LineCoverageState.Uncovered:
                    return "This line is not covered by any test.";
            }

            return null;
        }

        private static string SubstringWithoutParanthesis(string str)
        {
            var pIndex = str.IndexOf('(');
            if (pIndex == -1)
            {
                return str;
            }

            return str.Substring(0, pIndex);
        }

        // TODO: Determine unknown coverage state for this line
        // TODO: Bring back partly covered
        private LineCoverageState GetLineCoverageState(ITextViewLine line)
        {
            // get cover state for all spans included in this line
            var spans = GetSpansForLine(line, _currentSpans);

            if (spans.Any())
            {
                var allTrackedMethods = spans.SelectMany(s => _spanCoverage[s].TrackedMethods);
                if (!allTrackedMethods.Any())
                {
                    return LineCoverageState.Uncovered;
                }

                var results = allTrackedMethods
                    .Select(tm => SubstringWithoutParanthesis(tm.Name))
                    .Select(s => CoverageData.Instance.TestDetails.Dictionary[s]);

                if (results.Any(r => r == TestResult.Failed))
                {
                    return LineCoverageState.CoveredWithAtleastOneFailedTest;
                }

                if (results.All(r => r == TestResult.Passed))
                {
                    return LineCoverageState.CoveredWithPassingTests;
                }
            }
            
            return LineCoverageState.Unknown;
        }

        /// <summary>
        /// Calculates the spans covered by the given line
        /// </summary>
        /// <param name="line">Line to retrieve the spans for.</param>
        /// <param name="spanContainer">container of all spans</param>
        /// <returns></returns>
        public static IEnumerable<SnapshotSpan> GetSpansForLine(ITextViewLine line, IEnumerable<SnapshotSpan> spanContainer)
        {
            return spanContainer.Where(s => (s.Snapshot == line.Snapshot) && ((s.Start >= line.Start && s.Start <= line.End) || (s.Start < line.Start && s.End >= line.Start)));
        }    

        /// <summary>
        /// Hide the colored line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnGlyphMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextTagger tagger = TextTagger.GetTagger(_textView);

            if (tagger != null)
                tagger.RemoveLineRestriction();
        }

        /// <summary>
        /// Shows the line colors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnGlyphMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            IWpfTextViewLine line = (IWpfTextViewLine)((System.Windows.Shapes.Ellipse)sender).Tag;
            TextTagger tagger = TextTagger.GetTagger(_textView);

            if (tagger != null)
                tagger.ShowForLine(line);
        }    
    }
}
