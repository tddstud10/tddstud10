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
    [TagType(typeof(CodeCoverageTag))]
    public class CodeCoverageTaggerProvider : ITaggerProvider
    {
        [Import]
        private IBufferTagAggregatorFactoryService _aggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new CodeCoverageTagger(
                buffer,
                _aggregatorFactory.CreateTagAggregator<SequencePointTag>(buffer),
                DataStore.Instance) as ITagger<T>;
        }
    }
}
