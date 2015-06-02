using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor;

namespace R4nd0mApps.TddStud10.Hosts.VS.EditorExtensions
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("code")]
    [TagType(typeof(TestMarkerTag))]
    public class TestMarkerTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new TestMarkerTagger(buffer, DataStoreXXX.Instance) as ITagger<T>;
        }
    }
}
