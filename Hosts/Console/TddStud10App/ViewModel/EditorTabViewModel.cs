using System.IO;
using Microsoft.VisualStudio.Text.Editor;

namespace R4nd0mApps.TddStud10.Hosts.Console.TddStud10App.ViewModel
{
    public class EditorTabViewModel
    {
        public EditorTabViewModel(string filePath, IWpfTextViewHost textViewHost)
        {
            FilePath = filePath;
            TabName = Path.GetFileName(FilePath);
            TextViewHost = textViewHost;
        }

        public string FilePath { get; private set; }

        public string TabName { get; private set; }

        public IWpfTextViewHost TextViewHost { get; private set; }

        public object TabData { get { return TextViewHost.HostControl; } }
    }
}
