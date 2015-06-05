using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;

namespace R4nd0mApps.TddStud10.Hosts.Common.TestCode
{
    public class StubTextSnapshot : ITextSnapshot
    {
        public string _text { get; set; }

        public StubTextSnapshot(string text)
        {
            _text = text;
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
            throw new NotImplementedException();
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
            get { throw new NotImplementedException(); }
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
