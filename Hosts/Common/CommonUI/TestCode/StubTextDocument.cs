using System;
using System.Text;
using Microsoft.VisualStudio.Text;

namespace R4nd0mApps.TddStud10.Hosts.Common.TestCode
{
    public class StubTextDocument : ITextDocument
    {
        private string _filePath;

        private ITextBuffer _textBuffer;

        public StubTextDocument(string filePath, ITextBuffer textBuffer)
        {
            _filePath = filePath;
            _textBuffer = textBuffer;
        }

        #region ITextDocument Members

        public event EventHandler DirtyStateChanged;

        public Encoding Encoding
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler<EncodingChangedEventArgs> EncodingChanged;

        public event EventHandler<TextDocumentFileActionEventArgs> FileActionOccurred;

        public string FilePath
        {
            get { return _filePath; }
        }

        public bool IsDirty
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReloading
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime LastContentModifiedTime
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime LastSavedTime
        {
            get { throw new NotImplementedException(); }
        }

        public ReloadResult Reload(EditOptions options)
        {
            throw new NotImplementedException();
        }

        public ReloadResult Reload()
        {
            throw new NotImplementedException();
        }

        public void Rename(string newFilePath)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void SaveAs(string filePath, bool overwrite, bool createFolder, Microsoft.VisualStudio.Utilities.IContentType newContentType)
        {
            throw new NotImplementedException();
        }

        public void SaveAs(string filePath, bool overwrite, Microsoft.VisualStudio.Utilities.IContentType newContentType)
        {
            throw new NotImplementedException();
        }

        public void SaveAs(string filePath, bool overwrite, bool createFolder)
        {
            throw new NotImplementedException();
        }

        public void SaveAs(string filePath, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public void SaveCopy(string filePath, bool overwrite, bool createFolder)
        {
            throw new NotImplementedException();
        }

        public void SaveCopy(string filePath, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public void SetEncoderFallback(EncoderFallback fallback)
        {
            throw new NotImplementedException();
        }

        public ITextBuffer TextBuffer
        {
            get { return _textBuffer; }
        }

        public void UpdateDirtyState(bool isDirty, DateTime lastContentModifiedTime)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
