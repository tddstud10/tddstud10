# Roadmap

  - √ v0.1 - dogfood
    - √ Disable toolwindow
    - √ Trigger on Ctrl T, Ctrl T
    - √ Stop a barrage of events
    - √ Progress on status bar
    - √ Pick unit test dlls
    - √ enable on filesystem update
  - √ v0.1.x - be able to run tddstud10
    - √ version number update - 3 places
    - √ startup fixes
    - √ logging around seqpt and instru
    - √ crash on event source: clear default resolve's default paths, add resolver path
    - √ make it work for c# project that logs using event source
  - √ v0.2.0 - fix top annoyances + prep for pulling in some engineering
    - √ version number update - 3 places
    - √ Enable/Disable TDDStud10
    - √ Ignore: when build is going on, solution not loaded, ide shutting down
    - √ Dont close solution if run is on.
    - √ disable vsix/build/deploy/etc.
  - √ v0.2.0.1 - fix annoyances
    - √ version number update - 3 places
    - √ additional logging
  - v0.2.1 - fix top annoyances
    - version number update - 3 places
    - engine is always getting rebuilt
    - first time markers are not gettign shown
    - markers are getting created on the fly adn a new line is added
    - version downgrade to 4.0
    - move svcs.cs and slnexn to fs and tdd them
    - single assembly for diagnostics stuff
    - instrumentation - [a] cannot crash [b] report as warnings [c] restore assembly
    - if solution items are not there, then sln does not get copied
    - all files getting copied as upper case
    - support Theory
    - 3 stage update of markers [a] dim the greens once out of date [b] new code should be uncovered to start with [c] update coverage
    - failure experience [more prominent marqeee, one of the steps failed dont proceed, indicate in progressbar, update status on statusbar progressbar]
    - debug specific tests [a] indicate test in margin [b] might requrie gallileo integration [c] debug test host
    - Reduce perf of run tests
  - v0.2.5
    - version number update - 3 places
    - host in sublime text
  - v0.3 - prep for perf tests + ncruch compare
    - version number update - 3 places
    - Resync
    - release build stuff [fxcop etc.]
    - incremental
    - Enable on edit trigger [read file text from buffer]
    - support for nunit
  - v0.3 - perf tests + ncruch compare
    - version number update - 3 places
    - dont interfere with ncruch [build/test/editor adornments]
    - xunit, nunit
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
  - c# and f#
  - 64 and 32 bit

  later 
  - switch to rock steady protocol
  - self sufficient vsix
  - vs2010
  - c++/js
  - test: itself, xunit, nunit, opencover, roslyn

   */

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

  # Staging Plan #
  ## MVP0: ##
  - *Definition*: Console Host -> Pointed to xUnit/F# Sln folder -> Detect File Changes -> Builds -> Runs Tests -> Shows failing tests 
  - Console Host:
      - Point to folder, detect changes in that folder
      - Trigger Builder.exe -> Break on error
      - Run Tests -> Detect tests, Show current running test, Show progress 
  - Builder
      - All FS APIs Hooked and logged
      - VFS Implementation
      - VFS Implementation tested on xUnit codebase
  - TestRunner 
      - Detect number of tests
      - Run Tests one by one
      - Report progress

  # Components #
  - UX Integration 
      - CC Markers 
      - Automation Triggers 
      - Debugger launch 
 
  - Source Snapshoter 
      - Determine  
 
  - Builder 
      - Build sources 
 
  - Tester 
      - Discover Tests 
      - Execute tests 
