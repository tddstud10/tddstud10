using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using R4nd0mApps.TddStud10.Hosts.VS.Diagnostics;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;

namespace R4nd0mApps.TddStud10.Hosts.VS.EditorExtensions
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(MarginConstants.Name)]
    [Order(After = PredefinedMarginNames.Outlining)]
    [MarginContainer(PredefinedMarginNames.Left)]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    public sealed class MarginFactory : IWpfTextViewMarginProvider
    {
        [Import]
        private IBufferTagAggregatorFactoryService _aggregatorFactory = null;

        //[Import]
        public IMenuCommandService _menuCmdService = null;

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            if (_menuCmdService == null)
            {
                Logger.I.LogError("Unable to get IMenuCommandService. Context menus will be disabled!");
            }

            return new Margin(
                textViewHost.TextView,
                _aggregatorFactory.CreateTagAggregator<IMarginGlyphTag>(textViewHost.TextView.TextBuffer),
                _menuCmdService != null
                    ? _menuCmdService.ShowContextMenu
                    : new Action<CommandID, int, int>((_, __, ___) => { }));
        }
    }
}
