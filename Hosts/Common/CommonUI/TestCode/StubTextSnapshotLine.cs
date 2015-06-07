using System;
using Microsoft.VisualStudio.Text;

namespace R4nd0mApps.TddStud10.Hosts.Common.TestCode
{
    public class StubTextSnapshotLine : ITextSnapshotLine
    {
        private StubTextSnapshot _textSnapshot;

        private int _startPos;

        private string _text;

        private int _lineNumber;

        public StubTextSnapshotLine(StubTextSnapshot _textSnapshot, int startPos, string text, int lineNumber)
        {
            this._textSnapshot = _textSnapshot;
            this._startPos = startPos;
            this._text = text;
            this._lineNumber = lineNumber;
        }

        #region ITextSnapshotLine Members

        public SnapshotPoint End
        {
            get { throw new NotImplementedException(); }
        }

        public SnapshotPoint EndIncludingLineBreak
        {
            get { return new SnapshotPoint(_textSnapshot, _startPos + _text.Length); }
        }

        public SnapshotSpan Extent
        {
            get
            {
                return new SnapshotSpan(
                    _textSnapshot,
                    new Span(_startPos, _text.Length - Environment.NewLine.Length));
            }
        }

        public SnapshotSpan ExtentIncludingLineBreak
        {
            get
            {
                return new SnapshotSpan(
                    _textSnapshot,
                    new Span(_startPos, _text.Length));
            }
        }

        public string GetLineBreakText()
        {
            throw new NotImplementedException();
        }

        public string GetText()
        {
            throw new NotImplementedException();
        }

        public string GetTextIncludingLineBreak()
        {
            throw new NotImplementedException();
        }

        public int Length
        {
            get { throw new NotImplementedException(); }
        }

        public int LengthIncludingLineBreak
        {
            get { throw new NotImplementedException(); }
        }

        public int LineBreakLength
        {
            get { throw new NotImplementedException(); }
        }

        public int LineNumber
        {
            get { return _lineNumber; }
        }

        public ITextSnapshot Snapshot
        {
            get { throw new NotImplementedException(); }
        }

        public SnapshotPoint Start
        {
            get { return new SnapshotPoint(_textSnapshot, _startPos); }
        }

        #endregion
    }
}
