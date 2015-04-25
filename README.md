- curate todo list


/*
    TODO:
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
    - c++
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
