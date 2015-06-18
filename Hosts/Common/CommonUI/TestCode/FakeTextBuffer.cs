using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace R4nd0mApps.TddStud10.Hosts.Common.TestCode
{
    public class FakeTextBuffer : ITextBuffer
    {
        private PropertyCollection _propertyCollection;

        private FakeTextSnapshot _textSnapshot;

        public FakeTextBuffer(string text, string filePath)
        {
            _propertyCollection = new PropertyCollection();
            if (filePath != null)
            {
                _propertyCollection.AddProperty(typeof(ITextDocument), new FakeTextDocument(filePath, this));
            }

            _textSnapshot = new FakeTextSnapshot(text);
        }

        #region ITextBuffer Members

        public void ChangeContentType(Microsoft.VisualStudio.Utilities.IContentType newContentType, object editTag)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<TextContentChangedEventArgs> Changed
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler<TextContentChangedEventArgs> ChangedHighPriority
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler<TextContentChangedEventArgs> ChangedLowPriority
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler<TextContentChangingEventArgs> Changing
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public bool CheckEditAccess()
        {
            throw new NotImplementedException();
        }

        public Microsoft.VisualStudio.Utilities.IContentType ContentType
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<ContentTypeChangedEventArgs> ContentTypeChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public ITextEdit CreateEdit()
        {
            throw new NotImplementedException();
        }

        public ITextEdit CreateEdit(EditOptions options, int? reiteratedVersionNumber, object editTag)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyRegionEdit CreateReadOnlyRegionEdit()
        {
            throw new NotImplementedException();
        }

        public ITextSnapshot CurrentSnapshot
        {
            get { return _textSnapshot; }
        }

        public ITextSnapshot Delete(Span deleteSpan)
        {
            throw new NotImplementedException();
        }

        public bool EditInProgress
        {
            get { throw new NotImplementedException(); }
        }

        public NormalizedSpanCollection GetReadOnlyExtents(Span span)
        {
            throw new NotImplementedException();
        }

        public ITextSnapshot Insert(int position, string text)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly(Span span, bool isEdit)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly(Span span)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly(int position, bool isEdit)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly(int position)
        {
            throw new NotImplementedException();
        }

        public event EventHandler PostChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> ReadOnlyRegionsChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public ITextSnapshot Replace(Span replaceSpan, string replaceWith)
        {
            throw new NotImplementedException();
        }

        public void TakeThreadOwnership()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IPropertyOwner Members

        public Microsoft.VisualStudio.Utilities.PropertyCollection Properties
        {
            get { return _propertyCollection; }
        }

        #endregion
    }
}
