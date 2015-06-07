using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace R4nd0mApps.TddStud10.Hosts.Common.TestCode
{
    public class StubWpfTextViewLine : IWpfTextViewLine
    {
        private StubTextSnapshot _textSnapshot;

        private StubTextSnapshotLine _textSnapshotLine;

        private Rect _bounds;

        public StubWpfTextViewLine(StubTextSnapshot textSnapshot, StubTextSnapshotLine textSnapshotLine, Rect bounds)
        {
            _textSnapshot = textSnapshot;
            _textSnapshotLine = textSnapshotLine;
            _bounds = bounds;
        }

        #region ITextViewLine Members

        public double Baseline
        {
            get { throw new NotImplementedException(); }
        }

        public double Bottom
        {
            get { throw new NotImplementedException(); }
        }

        public TextViewLineChange Change
        {
            get { throw new NotImplementedException(); }
        }

        public bool ContainsBufferPosition(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition)
        {
            throw new NotImplementedException();
        }

        public LineTransform DefaultLineTransform
        {
            get { throw new NotImplementedException(); }
        }

        public double DeltaY
        {
            get { throw new NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.SnapshotPoint End
        {
            get { throw new NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.SnapshotPoint EndIncludingLineBreak
        {
            get { throw new NotImplementedException(); }
        }

        public double EndOfLineWidth
        {
            get { throw new NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.SnapshotSpan Extent
        {
            get
            {
                return _textSnapshotLine.Extent;
            }
        }

        public Microsoft.VisualStudio.Text.IMappingSpan ExtentAsMappingSpan
        {
            get { throw new NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.SnapshotSpan ExtentIncludingLineBreak
        {
            get
            {
                return _textSnapshotLine.ExtentIncludingLineBreak;
            }
        }

        public Microsoft.VisualStudio.Text.IMappingSpan ExtentIncludingLineBreakAsMappingSpan
        {
            get { throw new NotImplementedException(); }
        }

        public TextBounds? GetAdornmentBounds(object identityTag)
        {
            throw new NotImplementedException();
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<object> GetAdornmentTags(object providerTag)
        {
            throw new NotImplementedException();
        }

        public Microsoft.VisualStudio.Text.SnapshotPoint? GetBufferPositionFromXCoordinate(double xCoordinate)
        {
            throw new NotImplementedException();
        }

        public Microsoft.VisualStudio.Text.SnapshotPoint? GetBufferPositionFromXCoordinate(double xCoordinate, bool textOnly)
        {
            throw new NotImplementedException();
        }

        public TextBounds GetCharacterBounds(Microsoft.VisualStudio.Text.VirtualSnapshotPoint bufferPosition)
        {
            throw new NotImplementedException();
        }

        public TextBounds GetCharacterBounds(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition)
        {
            throw new NotImplementedException();
        }

        public TextBounds GetExtendedCharacterBounds(Microsoft.VisualStudio.Text.VirtualSnapshotPoint bufferPosition)
        {
            throw new NotImplementedException();
        }

        public TextBounds GetExtendedCharacterBounds(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition)
        {
            throw new NotImplementedException();
        }

        public Microsoft.VisualStudio.Text.VirtualSnapshotPoint GetInsertionBufferPositionFromXCoordinate(double xCoordinate)
        {
            throw new NotImplementedException();
        }

        public System.Collections.ObjectModel.Collection<TextBounds> GetNormalizedTextBounds(Microsoft.VisualStudio.Text.SnapshotSpan bufferSpan)
        {
            throw new NotImplementedException();
        }

        public Microsoft.VisualStudio.Text.SnapshotSpan GetTextElementSpan(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition)
        {
            throw new NotImplementedException();
        }

        public Microsoft.VisualStudio.Text.VirtualSnapshotPoint GetVirtualBufferPositionFromXCoordinate(double xCoordinate)
        {
            throw new NotImplementedException();
        }

        public double Height
        {
            get { return _bounds.Height; }
        }

        public object IdentityTag
        {
            get { throw new NotImplementedException(); }
        }

        public bool IntersectsBufferSpan(Microsoft.VisualStudio.Text.SnapshotSpan bufferSpan)
        {
            throw new NotImplementedException();
        }

        public bool IsFirstTextViewLineForSnapshotLine
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsLastTextViewLineForSnapshotLine
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsValid
        {
            get { throw new NotImplementedException(); }
        }

        public double Left
        {
            get { return _bounds.Left; }
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

        public LineTransform LineTransform
        {
            get { throw new NotImplementedException(); }
        }

        public double Right
        {
            get { throw new NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.ITextSnapshot Snapshot
        {
            get { throw new NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.SnapshotPoint Start
        {
            get { throw new NotImplementedException(); }
        }

        public double TextBottom
        {
            get { throw new NotImplementedException(); }
        }

        public double TextHeight
        {
            get { throw new NotImplementedException(); }
        }

        public double TextLeft
        {
            get { throw new NotImplementedException(); }
        }

        public double TextRight
        {
            get { throw new NotImplementedException(); }
        }

        public double TextTop
        {
            get { throw new NotImplementedException(); }
        }

        public double TextWidth
        {
            get { throw new NotImplementedException(); }
        }

        public double Top
        {
            get { return _bounds.Top; }
        }

        public double VirtualSpaceWidth
        {
            get { throw new NotImplementedException(); }
        }

        public VisibilityState VisibilityState
        {
            get { throw new NotImplementedException(); }
        }

        public double Width
        {
            get { return _bounds.Width; }
        }

        #endregion

        #region IWpfTextViewLine Members

        public System.Windows.Media.TextFormatting.TextRunProperties GetCharacterFormatting(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition)
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<System.Windows.Media.TextFormatting.TextLine> TextLines
        {
            get { throw new System.NotImplementedException(); }
        }

        public System.Windows.Rect VisibleArea
        {
            get { throw new System.NotImplementedException(); }
        }

        #endregion
    }

    public class StubWpfTextViewLineCollection : IWpfTextViewLineCollection
    {
        IEnumerable<IWpfTextViewLine> _textViewLines;

        public StubWpfTextViewLineCollection(IEnumerable<IWpfTextViewLine> textViewLines)
        {
            _textViewLines = textViewLines;
        }

        #region IWpfTextViewLineCollection Members

        public Microsoft.VisualStudio.Text.Formatting.IWpfTextViewLine FirstVisibleLine
        {
            get { throw new System.NotImplementedException(); }
        }

        public System.Windows.Media.Geometry GetLineMarkerGeometry(Microsoft.VisualStudio.Text.SnapshotSpan bufferSpan, bool clipToViewport, System.Windows.Thickness padding)
        {
            throw new System.NotImplementedException();
        }

        public System.Windows.Media.Geometry GetLineMarkerGeometry(Microsoft.VisualStudio.Text.SnapshotSpan bufferSpan)
        {
            throw new System.NotImplementedException();
        }

        public System.Windows.Media.Geometry GetMarkerGeometry(Microsoft.VisualStudio.Text.SnapshotSpan bufferSpan)
        {
            throw new System.NotImplementedException();
        }

        public System.Windows.Media.Geometry GetMarkerGeometry(Microsoft.VisualStudio.Text.SnapshotSpan bufferSpan, bool clipToViewport, System.Windows.Thickness padding)
        {
            throw new System.NotImplementedException();
        }

        public System.Windows.Media.Geometry GetTextMarkerGeometry(Microsoft.VisualStudio.Text.SnapshotSpan bufferSpan, bool clipToViewport, System.Windows.Thickness padding)
        {
            throw new System.NotImplementedException();
        }

        public System.Windows.Media.Geometry GetTextMarkerGeometry(Microsoft.VisualStudio.Text.SnapshotSpan bufferSpan)
        {
            throw new System.NotImplementedException();
        }

        public Microsoft.VisualStudio.Text.Formatting.IWpfTextViewLine GetTextViewLineContainingBufferPosition(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition)
        {
            throw new System.NotImplementedException();
        }

        public Microsoft.VisualStudio.Text.Formatting.IWpfTextViewLine LastVisibleLine
        {
            get { throw new System.NotImplementedException(); }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Microsoft.VisualStudio.Text.Formatting.IWpfTextViewLine> WpfTextViewLines
        {
            get { throw new System.NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.Formatting.IWpfTextViewLine this[int index]
        {
            get { throw new System.NotImplementedException(); }
        }

        #endregion

        #region ITextViewLineCollection Members

        public bool ContainsBufferPosition(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition)
        {
            throw new System.NotImplementedException();
        }

        Microsoft.VisualStudio.Text.Formatting.ITextViewLine ITextViewLineCollection.FirstVisibleLine
        {
            get { throw new System.NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.SnapshotSpan FormattedSpan
        {
            get { throw new System.NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.Formatting.TextBounds GetCharacterBounds(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition)
        {
            throw new System.NotImplementedException();
        }

        public int GetIndexOfTextLine(Microsoft.VisualStudio.Text.Formatting.ITextViewLine textLine)
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.ObjectModel.Collection<Microsoft.VisualStudio.Text.Formatting.TextBounds> GetNormalizedTextBounds(Microsoft.VisualStudio.Text.SnapshotSpan bufferSpan)
        {
            throw new System.NotImplementedException();
        }

        public Microsoft.VisualStudio.Text.SnapshotSpan GetTextElementSpan(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition)
        {
            throw new System.NotImplementedException();
        }

        Microsoft.VisualStudio.Text.Formatting.ITextViewLine ITextViewLineCollection.GetTextViewLineContainingBufferPosition(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition)
        {
            throw new System.NotImplementedException();
        }

        public Microsoft.VisualStudio.Text.Formatting.ITextViewLine GetTextViewLineContainingYCoordinate(double y)
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.ObjectModel.Collection<Microsoft.VisualStudio.Text.Formatting.ITextViewLine> GetTextViewLinesIntersectingSpan(Microsoft.VisualStudio.Text.SnapshotSpan bufferSpan)
        {
            throw new System.NotImplementedException();
        }

        public bool IntersectsBufferSpan(Microsoft.VisualStudio.Text.SnapshotSpan bufferSpan)
        {
            throw new System.NotImplementedException();
        }

        public bool IsValid
        {
            get { throw new System.NotImplementedException(); }
        }

        Microsoft.VisualStudio.Text.Formatting.ITextViewLine ITextViewLineCollection.LastVisibleLine
        {
            get { throw new System.NotImplementedException(); }
        }

        #endregion

        #region IList<ITextViewLine> Members

        public int IndexOf(Microsoft.VisualStudio.Text.Formatting.ITextViewLine item)
        {
            throw new System.NotImplementedException();
        }

        public void Insert(int index, Microsoft.VisualStudio.Text.Formatting.ITextViewLine item)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new System.NotImplementedException();
        }

        Microsoft.VisualStudio.Text.Formatting.ITextViewLine System.Collections.Generic.IList<Microsoft.VisualStudio.Text.Formatting.ITextViewLine>.this[int index]
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        #endregion

        #region ICollection<ITextViewLine> Members

        public void Add(Microsoft.VisualStudio.Text.Formatting.ITextViewLine item)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(Microsoft.VisualStudio.Text.Formatting.ITextViewLine item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(Microsoft.VisualStudio.Text.Formatting.ITextViewLine[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public int Count
        {
            get { throw new System.NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new System.NotImplementedException(); }
        }

        public bool Remove(Microsoft.VisualStudio.Text.Formatting.ITextViewLine item)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region IEnumerable<ITextViewLine> Members

        public System.Collections.Generic.IEnumerator<Microsoft.VisualStudio.Text.Formatting.ITextViewLine> GetEnumerator()
        {
            return _textViewLines.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
