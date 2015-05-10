# Mission
  - Environment for promoting Kent Beck style TDD [F.I.R.S.T. Unit Tests, fast builds, No Mocks, Hexagonal architecture]
  - Does not replace the awesome nCrunch.NET [Features that dont require the above attributes]
  - Add supporting features [TODO list, hotspot analysis]

# Principles
  - Fully open source [TBD - similar to the Gallio license]
  - One click setup, then continous streaming delivery
  - Host agnostic [VS or MonoDevelop, not tied to .NET/Windows]
  - Framework agnostic [xUnit.NET, JUnit, RSpec, not tied to .NET/Windows]
  - Meet or beat nCrunch w.r.t. snappiness & stability [not necessarily w.r.t. the feature set]

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
  - v0.2.0.x - fix crashes
    - ☑ version number update - 3 places
    - ☑ Add symbols to vsix
    - ☑ crash handlers in all engine entry points.
    - ☑ TestHost unhandled exception handler
    - ☑ [temporary fix] Strong name fails
    - ☑ [temporary fix] Editor crashes as unit test name doesnt ge registered sometime through Maker
  - v0.3 - fix top annoyances
    - ☑ version number update - 3 places
    - ☑ move svcs.cs and slnexn to fs and tdd them
    - ☑ Remove vision notes untill they are bit more ready
    - * progress reporting 
      - timing
      - logging
      - report 
        - start
        - steps and substeps
        - errors [e.g. build failres]
        - exceptions
        - cancellations
        - end
      - dont proceed if one of the step fails
      - cancellable
      - ask host if you can continue
      - pass data between steps
      - UX
        - R-G indications need to be much clearer
        - 3 stage update of markers [a] dim the greens once out of date [b] new code should be uncovered to start with [c] update coverage
<* Feedback
  testdoubles.runexector methods are not called + methods inside an async block
  unable to understand red due to build failure - needed to build
  unable to understand exception caused and where - needed to look into the test failures
  test execution seems to halt initially - not as fast as the rest of teh steps!!!
  debugging really really needed
 *>
    - * Test Host design
      - Unit test name comparision is through simple text - will fail for generics e.g.
      - xunit 1.9 tests when mixed with xunit 2.0 projects (in same solution) doesn't execute
      - incorrect comparision between cecil and xunit: crash
          from cecil: R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.ServicesTests/ITestServiceInterface R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.ServicesTests::Service Provider returns service interface if service is found
          from xunit: R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.ServicesTests+ITestServiceInterface R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.ServicesTests::Service Provider returns service interface if service is found      
      - debug test is needed
        - for test failures
        - and for comparing the coverage
      - support Theory
      - Run on gallio - http://www.gallio.org/book/GallioBook.pdf - got to internet history
      - [permanent fix] Test name comparision [Editor crashes as unit test name doesnt ge registered sometime through Maker]
      - Reduce perf of run tests
    - * test start and test failure point markers
      - list of tests on each point
      - What is the exception thrown and the point of faiure?
    - * instantenous trigger
      - accurate snapshotting
      - incremental copy of files [chutzpah has solved this]
      - incremental build
      - engine is always getting rebuilt
    - * Toolwindow with rich details
    - Editor discrepancies
      - first time markers are not gettign shown - on scroll up and then down, they get shown
      - [permanent fix] Editor crashes as unit test name doesnt ge registered sometime through Maker
      - markers are getting created on the fly adn a new line is added
    - crash in editor - thought we fixed this
            <description>System.ArgumentNullException: Value cannot be null.&#x000D;&#x000A;Parameter name: key&#x000D;&#x000A; 
        System.Collections.Concurrent.ConcurrentDictionary`2.TryGetValue(TKey key, TValue&amp; value)&#x000D;&#x000A; 
        System.Collections.Concurrent.ConcurrentDictionary`2.get_Item(TKey key)&#x000D;&#x000A; 
        R4nd0mApps.TddStud10.Hosts.VS.Glyphs.LineCoverageGlyphFactory.&lt;GetLineCoverageState&gt;b__12(String tm) in d:\src\r4nd0mapps\tddstud10.1\Hosts\VS\TddStudioPackage\EditorExtensions\LineCoverageGlyphFactory.cs:line 137&#x000D;&#x000A; 
        System.Linq.Enumerable.WhereSelectEnumerableIterator`2.MoveNext()&#x000D;&#x000A; 
        System.Linq.Enumerable.Any[TSource](IEnumerable`1 source, Func`2 predicate)&#x000D;&#x000A; 
        R4nd0mApps.TddStud10.Hosts.VS.Glyphs.LineCoverageGlyphFactory.GetLineCoverageState(ITextViewLine line) in d:\src\r4nd0mapps\tddstud10.1\Hosts\VS\TddStudioPackage\EditorExtensions\LineCoverageGlyphFactory.cs:line 139&#x000D;&#x000A;   at R4nd0mApps.TddStud10.Hosts.VS.Glyphs.LineCoverageGlyphFactory.GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag) in d:\src\r4nd0mapps\tddstud10.1\Hosts\VS\TddStudioPackage\EditorExtensions\LineCoverageGlyphFactory.cs:line 54&#x000D;&#x000A;   at Microsoft.VisualStudio.Text.Editor.Implementation.GlyphMarginVisualManager.AddGlyph(IGlyphTag tag, SnapshotSpan span)&#x000D;&#x000A;   at Microsoft.VisualStudio.Text.Editor.Implementation.GlyphMargin.RefreshGlyphsOver(ITextViewLine textViewLine)&#x000D;&#x000A;   at Microsoft.VisualStudio.Text.Editor.Implementation.GlyphMargin.OnBatchedTagsChanged(Object sender, BatchedTagsChangedEventArgs e)&#x000D;&#x000A;   at Microsoft.VisualStudio.Text.Utilities.GuardedOperations.RaiseEvent[TArgs](Object sender, EventHandler`1 eventHandlers, TAr
    - instrumentation 
      - [a] cannot crash [b] report as warnings [c] restore assembly
      - [permanent fix] Strong name fails
    - if solution items are not there, then sln does not get copied
    - all files getting copied as upper case
    - debug specific tests [a] indicate test in margin [b] might requrie gallileo integration [c] debug test host
  - v0.3.5 - clear debt
    - version number update - 3 places
    - release build stuff [fxcop etc.]
    - integrage with omnisharp/sublime
      - host in sublime text
  - v0.4 - prep for perf tests + ncruch compare
    - version number update - 3 places
    - Resync
    - Enable on edit trigger [read file text from buffer]
    - support for nunit
    - cancel run
  - v0.5 - perf tests + ncruch compare
    - version number update - 3 places
    - dont interfere with ncruch [build/test/editor adornments]
    - xunit, nunit
    - support vs2010/vs2011
  - v0.9 - beta
    - version number update - 3 places
    - support matrix - should be from 2010 ideally
    - video tutorial with 3 katas
    - telemetry
    - support multiple vs/clr versions
    - icons and artwork
  - v0.9.1 - rc
    - version number update - 3 places
  - v1.0 - v1 release
    - version number update - 3 places
  - v2.0
    - version number update - 3 places
    - test covered on each list, debug that test etc.
    - partly covered statements
    - test timings
    - todo List
    - *hotspot

# Dogfood build checklist
  - Rename all assemblies
  - Rename event source channels
  - df next to menu items
  - df in product name
  - tddstud10.df

# Miscellenous notes

  - Progress/Notification Bar ideas
    - Hook into statusbar
    - Dock toobar to bottom [waste of space from the toolbar title]
    - WPF window without status/menu/system bar, positioned relative to top left, draggable by user
    - viewport adornment [con: need to show only for active document, so it will jump around a bit]
  - Consider Roslyn's msbuildworkspace [a] parse sln [b] get exact list of project files
  - Get a robust stdout/err redirector from msbuild
  - icons
  - multithreaded filecopy/discovery
  - version specific builder/testrunner
  - timing info of tests
  - make the obvious speed improvements (don't copy all files, filter out assemblies, remove o(n^2) search in discoverer, filter out known assemblies from coverage)
  - failures in the process show show up clearly 
  - incremental b/r  
  - automatic trigger
  - run/debug single test
  - indicator of current progress

  In the project:
  - nunit and xunit and both
  - c#/f#-C++-RSpec
  - 64 and 32 bit

  later 
  - self sufficient vsix
  - vs2010
  - c++/js
  - test: itself, xunit, nunit, opencover, roslyn


  - Enable fxcop
  - release build - enable all warning, etc.
  - licensing and other stuff


  [[Markdown](http://daringfireball.net/projects/markdown/) syntax]

  # Feature Set #
  - Unit Test Frameworks [nUnit, xUnit, VS' CppUnit] 
  - Languages [C++/C#/F#] 
  - Hosts [VS/Sublime Text] 
  - Editor enhancements [Code Coverage, Test Markers] 
  - Automated build and run test 
  - Debug first failing 1 test 
  - Test List 
  - Automated change detection
