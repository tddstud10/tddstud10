using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;

namespace R4nd0mApps.TddStud10.Hosts.Common.TestCode
{
    public class FakeTextSnapshot : ITextSnapshot
    {
        private string _text;

        private IEnumerable<FakeTextSnapshotLine> _textSnapshotLines;

        public FakeTextSnapshot(string text)
        {
            /*
             * NOTE: While creating each TextSnapshotLine, we are adding trailing newline.
             * Meaning the last TSL gets a trailing newline even if the original text didnt 
             * have a trailing newline. For now this is a YAGNI call.
             */
            _text = text;
            _textSnapshotLines =
                _text
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Aggregate(
                    new Tuple<List<FakeTextSnapshotLine>, int, int>(new List<FakeTextSnapshotLine>(), 0, 0),
                    (acc, e) =>
                    {
                        acc.Item1.Add(new FakeTextSnapshotLine(this, acc.Item2, e + Environment.NewLine, acc.Item3));
                        return new Tuple<List<FakeTextSnapshotLine>, int, int>(
                            acc.Item1,
                            acc.Item2 + e.Length + Environment.NewLine.Length,
                            acc.Item3 + 1);
                    }).Item1;
        }

        public IEnumerable<FakeTextSnapshotLine> FakeTextSnapshotLines
        {
            get { return _textSnapshotLines; }
        }

        #region ITextSnapshot Members

        public Microsoft.VisualStudio.Utilities.IContentType ContentType
        {
            get { throw new NotImplementedException(); }
        }

        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            throw new NotImplementedException();
        }

        public ITrackingPoint CreateTrackingPoint(int position, PointTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
        {
            throw new NotImplementedException();
        }

        public ITrackingPoint CreateTrackingPoint(int position, PointTrackingMode trackingMode)
        {
            throw new NotImplementedException();
        }

        public ITrackingSpan CreateTrackingSpan(int start, int length, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
        {
            throw new NotImplementedException();
        }

        public ITrackingSpan CreateTrackingSpan(int start, int length, SpanTrackingMode trackingMode)
        {
            throw new NotImplementedException();
        }

        public ITrackingSpan CreateTrackingSpan(Span span, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
        {
            throw new NotImplementedException();
        }

        public ITrackingSpan CreateTrackingSpan(Span span, SpanTrackingMode trackingMode)
        {
            throw new NotImplementedException();
        }

        public ITextSnapshotLine GetLineFromLineNumber(int lineNumber)
        {
            throw new NotImplementedException();
        }

        public ITextSnapshotLine GetLineFromPosition(int position)
        {
            return _textSnapshotLines
                .Where(l => l.Start.Position <= position && position < l.EndIncludingLineBreak.Position)
                .FirstOrDefault();
        }

        public int GetLineNumberFromPosition(int position)
        {
            throw new NotImplementedException();
        }

        public string GetText()
        {
            throw new NotImplementedException();
        }

        public string GetText(int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        public string GetText(Span span)
        {
            return _text.Substring(span.Start, span.Length);
        }

        public int Length
        {
            get { return _text.Length; }
        }

        public int LineCount
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<ITextSnapshotLine> Lines
        {
            get { return _textSnapshotLines; }
        }

        public ITextBuffer TextBuffer
        {
            get { throw new NotImplementedException(); }
        }

        public char[] ToCharArray(int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        public ITextVersion Version
        {
            get { throw new NotImplementedException(); }
        }

        public void Write(System.IO.TextWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Write(System.IO.TextWriter writer, Span span)
        {
            throw new NotImplementedException();
        }

        public char this[int position]
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
