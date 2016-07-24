using EditorUtils;
using R4nd0mApps.TddStud10.Hosts.VS.EditorExtensions;
using System.ComponentModel.Composition.Hosting;

namespace R4nd0mApps.TddStud10.Hosts.Console.TddStud10App
{
    public sealed class EditorHostLoader
    {
        private readonly EditorHost _editorHost;
        public EditorHost EditorHost
        {
            get { return _editorHost; }
        }

        public CompositionContainer CompositionContainer
        {
            get { return _editorHost.CompositionContainer; }
        }

        internal EditorHostLoader()
        {
            var editorHostFactory = new EditorHostFactory();
            editorHostFactory.Add(new AssemblyCatalog(typeof(DefaultKeyProcessorProvider).Assembly));
            editorHostFactory.Add(new AssemblyCatalog(typeof(MarginFactory).Assembly));
            _editorHost = editorHostFactory.CreateEditorHost();
        }
    }
}
