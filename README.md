Test Driven Development Studio - An environment for practicing Kent Beck style Test Driven Development.
Copyright (C) 2015  Partho P. Das

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

# Mission
  - Environment for practicing Kent Beck style TDD [F.I.R.S.T. Unit Tests, fast builds, No Mocks, Hexagonal architecture]
  - Make TDD a joy!

# Principles
  - Fully open source development
  - One click setup, then continous streaming delivery
  - Host agnostic [VS/MonoDevelop/Sublime/etc. & Windows/Linux/Mac]
  - Framework agnostic [xUnit.NET, JUnit, RSpec, not tied to .NET/Windows]
  - Does not attempt to replace the awesome nCrunch, but meet/beat it w.r.t. snappiness & stability 
  - Not just unit tests - but a collection of facilities that makes TDD a joy! e.g. TODO list, hotspot analysis, etc.

# Features that currently work
  - Run build/test cycle on every change to any file under the solution folder
  - Incremental - i.e. build/test happen only for projects that have changed
  - Show code coverage indicators next to each line [green => tests covering it have all passed, red => otherwise]
  - nCrunch style status indicator at the bottom-right in VS Status Bar.

# Roadmap
  - ☑ v0.1 - dogfood
    - ☑ Disable toolwindow
    - ☑ Trigger on Ctrl T, Ctrl T
    - ☑ Stop a barrage of events
    - ☑ Progress on status bar
    - ☑ Pick unit test dlls
    - ☑ enable on filesystem update
  - ☑ v0.1.x - be able to run tddstud10
    - ☑ version number update - 3 places
    - ☑ startup fixes
    - ☑ logging around seqpt and instru
    - ☑ crash on event source: clear default resolve's default paths, add resolver path
    - ☑ make it work for c# project that logs using event source
  - ☑ v0.2.0 - fix top annoyances + prep for pulling in some engineering
    - ☑ version number update - 3 places
    - ☑ Enable/Disable TDDStud10
    - ☑ Ignore: when build is going on, solution not loaded, ide shutting down
    - ☑ Dont close solution if run is on.
    - ☑ disable vsix/build/deploy/etc.
  - ☑ v0.2.0.1 - fix annoyances
    - ☑ version number update - 3 places
    - ☑ additional logging
  - ☑ v0.2.0.x - fix crashes
    - ☑ version number update - 3 places
    - ☑ Add symbols to vsix
    - ☑ crash handlers in all engine entry points.
    - ☑ TestHost unhandled exception handler
    - ☑ [temporary fix] Strong name fails
    - ☑ [temporary fix] Editor crashes as unit test name doesnt ge registered sometime through Maker
  - ☑ v0.3.1 - Progress reporting pipeline online
    - ☑ version number update - 3 places
    - ☑ move svcs.cs and slnexn to fs and tdd them
    - ☑ Remove vision notes untill they are bit more ready
    - ☑ progress reporting 
    - ☑ timing, logging
    - ☑ report: start, steps, errors [e.g. build failres], exceptions, cancellations, end
    - ☑ dont proceed if one of the step fails
    - ☑ ask host if you can continue
    - ☑ pass data between steps
    - ☑ UX
        - ☑ R-G indications need to be much clearer
  - ☐ v0.3.1.1 - fixups for dogfooding for v0.3.1
    - ☑ version number update - 3 places
    - ☑ add license
    - ☑ issues
      - ☑ same ide process - open n times, n failed transition messages get fired!
      - ☑ session timestamp being the changenotification - causes markers to not show up in the code files if only unit tests are changed
      - ☑ crash in are paths the same null reference exception in PathBuilder.arePathsTheSame
      - ☑ red markers werent getting shown
    - ☐ generate dogfood build
    - ☐ test dogfood build
  - ☐ v0.3.2 - Test Host design online
    - ☐ version number update - 3 places
    - ☐ fix fsunit
    - ☐ Support theory
    - ☐ Unit test name comparision is through simple text - will fail for generics e.g.
    - ☐ xunit 1.9 tests when mixed with xunit 2.0 projects (in same solution) doesn't execute
    - ☐ incorrect comparision between cecil and xunit: crash
        from cecil: R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.ServicesTests/ITestServiceInterface R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.ServicesTests::Service Provider returns service interface if service is found
        from xunit: R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.ServicesTests+ITestServiceInterface R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.ServicesTests::Service Provider returns service interface if service is found      
    - ☐ debug test is needed - right click on one of the green, set bp, launch db
      - ☐ for test failures
      - ☐ and for comparing the coverage
    - ☐ [permanent fix] Test name comparision [Editor crashes as unit test name doesnt ge registered sometime through Maker]
    - ☐ Reduce perf of run tests
    - ☐ debug specific tests [a] indicate test in margin [b] might requrie gallileo integration [c] debug test host

  - ☐☐ Visual Feedback & User Experience
      - ☐ 3 stage update of markers [a] dim the greens once out of date [b] new code should be uncovered to start with [c] update coverage
      - ☐ progress of individual steps
      - ☐ true cancellation semantics
      - ☐ Editor discrepancies
        - ☐ first time markers are not gettign shown - ☐ on scroll up and then down, they get shown
        - ☐ [permanent fix] Editor crashes as unit test name doesnt ge registered sometime through Maker
        - ☐ markers are getting created on the fly adn a new line is added
        - ☐ crash in editor - ☐ thought we fixed this
                <description>System.ArgumentNullException: Value cannot be null.&#x000D;&#x000A;Parameter name: key&#x000D;&#x000A; 
            System.Collections.Concurrent.ConcurrentDictionary`2.TryGetValue(TKey key, TValue&amp; value)&#x000D;&#x000A; 
            System.Collections.Concurrent.ConcurrentDictionary`2.get_Item(TKey key)&#x000D;&#x000A; 
            R4nd0mApps.TddStud10.Hosts.VS.Glyphs.LineCoverageGlyphFactory.&lt;GetLineCoverageState&gt;b__12(String tm) in d:\src\r4nd0mapps\tddstud10.1\Hosts\VS\TddStudioPackage\EditorExtensions\LineCoverageGlyphFactory.cs:line 137&#x000D;&#x000A; 
            System.Linq.Enumerable.WhereSelectEnumerableIterator`2.MoveNext()&#x000D;&#x000A; 
            System.Linq.Enumerable.Any[TSource](IEnumerable`1 source, Func`2 predicate)&#x000D;&#x000A; 
            R4nd0mApps.TddStud10.Hosts.VS.Glyphs.LineCoverageGlyphFactory.GetLineCoverageState(ITextViewLine line) in d:\src\r4nd0mapps\tddstud10.1\Hosts\VS\TddStudioPackage\EditorExtensions\LineCoverageGlyphFactory.cs:line 139&#x000D;&#x000A;   at R4nd0mApps.TddStud10.Hosts.VS.Glyphs.LineCoverageGlyphFactory.GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag) in d:\src\r4nd0mapps\tddstud10.1\Hosts\VS\TddStudioPackage\EditorExtensions\LineCoverageGlyphFactory.cs:line 54&#x000D;&#x000A;   at Microsoft.VisualStudio.Text.Editor.Implementation.GlyphMarginVisualManager.AddGlyph(IGlyphTag tag, SnapshotSpan span)&#x000D;&#x000A;   at Microsoft.VisualStudio.Text.Editor.Implementation.GlyphMargin.RefreshGlyphsOver(ITextViewLine textViewLine)&#x000D;&#x000A;   at Microsoft.VisualStudio.Text.Editor.Implementation.GlyphMargin.OnBatchedTagsChanged(Object sender, BatchedTagsChangedEventArgs e)&#x000D;&#x000A;   at Microsoft.VisualStudio.Text.Utilities.GuardedOperations.RaiseEvent[TArgs](Object sender, EventHandler`1 eventHandlers, TAr
    - ☐ test start and test failure point markers
      ☐ list of tests on each point
      ☐ What is the exception thrown and the point of faiure?
    - ☐ Toolwindow
          - ☐ write errors in toolwindow, clean for every session
          - ☐ click on the status icon should open the toolwindow


  - ☐☐ instantenous trigger
    - ☐ accurate snapshotting
    - ☐ incremental copy of files [chutzpah has solved this]
    - ☐ incremental build
    - ☐ engine is always getting rebuilt
    - ☐ instrumentation 
      - ☐ [a] cannot crash [b] report as warnings [c] restore assembly
      - ☐ [permanent fix] Strong name fails
    - ☐ if solution items are not there, then sln does not get copied
    - ☐ all files getting copied as upper case
  - ☐ v0.3.5 - clear debt
    - ☐ version number update - 3 places
    - ☐ release build stuff [fxcop etc.]
    - ☐ integrage with omnisharp/sublime
      - ☐ host in sublime text
  - ☐ v0.4 - prep for perf tests + ncruch compare
    - ☐ version number update - 3 places
    - ☐ Resync
    - ☐ Enable on edit trigger [read file text from buffer]
    - ☐ support for nunit
    - ☐ cancel run
  - ☐ v0.5 - perf tests + ncruch compare
    - ☐ version number update - 3 places
    - ☐ dont interfere with ncruch [build/test/editor adornments]
    - ☐ xunit, nunit
    - ☐ support vs2010/vs2011
  - ☐ v0.9 - beta
    - ☐ version number update - 3 places
    - ☐ support matrix - ☐ should be from 2010 ideally
    - ☐ video tutorial with 3 katas
    - ☐ telemetry
    - ☐ support multiple vs/clr versions
    - ☐ icons and artwork
  - ☐ v0.9.1 - rc
    - ☐ version number update - 3 places
  - ☐ v1.0 - v1 release
    - ☐ version number update - 3 places
  - ☐ v2.0
    - ☐ version number update - 3 places
    - ☐ test covered on each list, debug that test etc.
    - ☐ partly covered statements
    - ☐ test timings
    - ☐ todo List
    - ☐ *hotspot

# Miscellenous notes

  - ☐ Progress/Notification Bar ideas
    - ☐ Hook into statusbar
    - ☐ Dock toobar to bottom [waste of space from the toolbar title]
    - ☐ WPF window without status/menu/system bar, positioned relative to top left, draggable by user
    - ☐ viewport adornment [con: need to show only for active document, so it will jump around a bit]
  - ☐ Consider Roslyn's msbuildworkspace [a] parse sln [b] get exact list of project files
  - ☐ Get a robust stdout/err redirector from msbuild
  - ☐ icons
  - ☐ multithreaded filecopy/discovery
  - ☐ version specific builder/testrunner
  - ☐ timing info of tests
  - ☐ make the obvious speed improvements (don't copy all files, filter out assemblies, remove o(n^2) search in discoverer, filter out known assemblies from coverage)
  - ☐ failures in the process show show up clearly 
  - ☐ incremental b/r  
  - ☐ automatic trigger
  - ☐ run/debug single test
  - ☐ indicator of current progress

  In the project:
  - ☐ nunit and xunit and both
  - ☐ c#/f#-C++-RSpec
  - ☐ 64 and 32 bit

  later 
  - ☐ self sufficient vsix
  - ☐ vs2010
  - ☐ c++/js
  - ☐ test: itself, xunit, nunit, opencover, roslyn


  - ☐ Enable fxcop
  - ☐ release build - ☐ enable all warning, etc.
  - ☐ licensing and other stuff


  [[Markdown](http://daringfireball.net/projects/markdown/) syntax]

  # Feature Set #
  - ☐ Unit Test Frameworks [nUnit, xUnit, VS' CppUnit] 
  - ☐ Languages [C++/C#/F#] 
  - ☐ Hosts [VS/Sublime Text] 
  - ☐ Editor enhancements [Code Coverage, Test Markers] 
  - ☐ Automated build and run test 
  - ☐ Debug first failing 1 test 
  - ☐ Test List 
  - ☐ Automated change detection
