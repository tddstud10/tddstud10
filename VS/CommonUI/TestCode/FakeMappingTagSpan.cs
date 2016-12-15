using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace R4nd0mApps.TddStud10.Hosts.Common.TestCode
{
    public class FakeMappingTagSpan<T> : IMappingTagSpan<T> where T : ITag
    {
        #region IMappingTagSpan<T> Members

        public IMappingSpan Span
        {
            get;
            set;
        }

        public T Tag
        {
            get;
            set;
        }

        #endregion
    }
}
