using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.Text.Editor;

namespace R4nd0mApps.TddStud10.Hosts.Common.TestCode
{
    public class StubWpfTextView : IWpfTextView
    {
        private Point _vpLocation;

        private StubTextSnapshot _textSnapshot;
        
        private StubWpfTextViewLineCollection _textViewLineCollection;

        public StubWpfTextView(Point vpLocation, double lineHeight, string text)
        {
            _vpLocation = vpLocation;
            _textSnapshot = new StubTextSnapshot(text);
            _textViewLineCollection = new StubWpfTextViewLineCollection(
                text
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Aggregate(
                    new Tuple<List<StubWpfTextViewLine>, int, Point>(new List<StubWpfTextViewLine>(), 0, _vpLocation),
                    (acc, e) =>
                    {
                        acc.Item1.Add(new StubWpfTextViewLine(_textSnapshot, new Rect(acc.Item3, new Size(0, lineHeight)), acc.Item2, e + Environment.NewLine));
                        return new Tuple<List<StubWpfTextViewLine>, int, Point>(
                            acc.Item1, 
                            acc.Item2 + e.Length + Environment.NewLine.Length, 
                            new Point(acc.Item3.X, acc.Item3.Y + lineHeight));
                    }).Item1);
        }

        #region IWpfTextView Members

        public System.Windows.Media.Brush Background
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

        public event System.EventHandler<BackgroundBrushChangedEventArgs> BackgroundBrushChanged
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.Formatting.IFormattedLineSource FormattedLineSource
        {
            get { throw new System.NotImplementedException(); }
        }

        public IAdornmentLayer GetAdornmentLayer(string name)
        {
            throw new System.NotImplementedException();
        }

        public ISpaceReservationManager GetSpaceReservationManager(string name)
        {
            throw new System.NotImplementedException();
        }

        public Microsoft.VisualStudio.Text.Formatting.IWpfTextViewLine GetTextViewLineContainingBufferPosition(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition)
        {
            throw new System.NotImplementedException();
        }

        public Microsoft.VisualStudio.Text.Formatting.ILineTransformSource LineTransformSource
        {
            get { throw new System.NotImplementedException(); }
        }

        public IWpfTextViewLineCollection TextViewLines
        {
            get { return _textViewLineCollection; }
        }

        public System.Windows.FrameworkElement VisualElement
        {
            get { throw new System.NotImplementedException(); }
        }

        public double ZoomLevel
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

        public event System.EventHandler<ZoomLevelChangedEventArgs> ZoomLevelChanged
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        #endregion

        #region ITextView Members

        public Microsoft.VisualStudio.Text.Projection.IBufferGraph BufferGraph
        {
            get { throw new System.NotImplementedException(); }
        }

        public ITextCaret Caret
        {
            get { throw new System.NotImplementedException(); }
        }

        public void Close()
        {
            throw new System.NotImplementedException();
        }

        public event System.EventHandler Closed
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        public void DisplayTextLineContainingBufferPosition(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition, double verticalDistance, ViewRelativePosition relativeTo, double? viewportWidthOverride, double? viewportHeightOverride)
        {
            throw new System.NotImplementedException();
        }

        public void DisplayTextLineContainingBufferPosition(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition, double verticalDistance, ViewRelativePosition relativeTo)
        {
            throw new System.NotImplementedException();
        }

        public Microsoft.VisualStudio.Text.SnapshotSpan GetTextElementSpan(Microsoft.VisualStudio.Text.SnapshotPoint point)
        {
            throw new System.NotImplementedException();
        }

        Microsoft.VisualStudio.Text.Formatting.ITextViewLine ITextView.GetTextViewLineContainingBufferPosition(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition)
        {
            throw new System.NotImplementedException();
        }

        public event System.EventHandler GotAggregateFocus
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        public bool HasAggregateFocus
        {
            get { throw new System.NotImplementedException(); }
        }

        public bool InLayout
        {
            get { throw new System.NotImplementedException(); }
        }

        public bool IsClosed
        {
            get { throw new System.NotImplementedException(); }
        }

        public bool IsMouseOverViewOrAdornments
        {
            get { throw new System.NotImplementedException(); }
        }

        public event System.EventHandler<TextViewLayoutChangedEventArgs> LayoutChanged;

        public double LineHeight
        {
            get { throw new System.NotImplementedException(); }
        }

        public event System.EventHandler LostAggregateFocus
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        public double MaxTextRightCoordinate
        {
            get { throw new System.NotImplementedException(); }
        }

        public event System.EventHandler<MouseHoverEventArgs> MouseHover
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        public IEditorOptions Options
        {
            get { throw new System.NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.ITrackingSpan ProvisionalTextHighlight
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

        public void QueueSpaceReservationStackRefresh()
        {
            throw new System.NotImplementedException();
        }

        public ITextViewRoleSet Roles
        {
            get { throw new System.NotImplementedException(); }
        }

        public ITextSelection Selection
        {
            get { throw new System.NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.ITextBuffer TextBuffer
        {
            get { throw new System.NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.ITextDataModel TextDataModel
        {
            get { throw new System.NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.ITextSnapshot TextSnapshot
        {
            get { throw new System.NotImplementedException(); }
        }

        ITextViewLineCollection ITextView.TextViewLines
        {
            get { return _textViewLineCollection; }
        }

        public ITextViewModel TextViewModel
        {
            get { throw new System.NotImplementedException(); }
        }

        public IViewScroller ViewScroller
        {
            get { throw new System.NotImplementedException(); }
        }

        public double ViewportBottom
        {
            get { throw new System.NotImplementedException(); }
        }

        public double ViewportHeight
        {
            get { throw new System.NotImplementedException(); }
        }

        public event System.EventHandler ViewportHeightChanged
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        public double ViewportLeft
        {
            get
            {
                return _vpLocation.X;
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public event System.EventHandler ViewportLeftChanged
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        public double ViewportRight
        {
            get { throw new System.NotImplementedException(); }
        }

        public double ViewportTop
        {
            get { return _vpLocation.Y; }
        }

        public double ViewportWidth
        {
            get { throw new System.NotImplementedException(); }
        }

        public event System.EventHandler ViewportWidthChanged
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.ITextSnapshot VisualSnapshot
        {
            get { throw new System.NotImplementedException(); }
        }

        #endregion

        #region IPropertyOwner Members

        public Microsoft.VisualStudio.Utilities.PropertyCollection Properties
        {
            get { throw new System.NotImplementedException(); }
        }

        #endregion

        public void FireLayoutChangedEvent()
        {
            var handler = LayoutChanged;
            if (handler != null)
            {
                handler(this, null);
            }
        }
    }
}
