/*
Copyright (c) 2015 Raghavendra Nagaraj

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using R4nd0mApps.TddStud10.Hosts.VS.Tagger;

namespace R4nd0mApps.TddStud10.Hosts.VS.Glyphs
{    
    /// <summary>
    /// Creates the <see cref="LineCoverageGlyphFactory"/> instance.
    /// </summary>
    [Export(typeof(IGlyphFactoryProvider))]
    [Name("TDD Studio Code Coverage Glyph")]
    [Order(After = "VsTextMarker")]
    [ContentType("code")]
    [TagType(typeof(LineCoverageTag))]
    internal sealed class LineCoverageGlyphFactoryProvider : IGlyphFactoryProvider
    {
        /// <summary>
        /// Creates the factory instance for the glyphs.
        /// </summary>
        /// <param name="view">The editor view.</param>
        /// <param name="margin">The editor margin instance.</param>
        /// <returns></returns>
        public IGlyphFactory GetGlyphFactory(IWpfTextView view, IWpfTextViewMargin margin)
        {
            return new LineCoverageGlyphFactory(view);
        }
    }
}
