using Microsoft.VisualStudio.Text.Editor;

namespace R4nd0mApps.TddStud10.Hosts.Common.TestCode
{
    public class StubWpfTextViewLineCollection : IWpfTextViewLineCollection
    {
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
            throw new System.NotImplementedException();
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
