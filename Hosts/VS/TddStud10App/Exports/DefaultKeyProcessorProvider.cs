using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace R4nd0mApps.TddStud10.Hosts.Console.TddStud10App.Exports
{
    [Export(typeof(IKeyProcessorProvider))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    [Name("TddStud10App KeyProcessorProvider")]
    public sealed class DefaultKeyProcessorProvider : IKeyProcessorProvider
    {
        public KeyProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
        {
            return new DefaultKeyProcessor();
        }
    }
}
