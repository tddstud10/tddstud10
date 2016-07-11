> **Test Driven Development Studio - An environment for practicing Kent Beck style Test Driven Development.**
>
>Copyright (C) 2015  Partho P. Das.
>
>`This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.`
>
>`This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.`
>
>`You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.`

# Goals, Intentions, Guidelines
  - Environment for practicing Kent Beck style TDD [F.I.R.S.T. Unit Tests, fast builds, No Mocks, Hexagonal architecture]
  - Open source alternative to nCrunch
  - Eventually not just unit tests - but a collection of facilities that makes TDD a joy! e.g. TODO list, hotspot analysis, etc.

# Features that currently work [[Demo](https://www.youtube.com/watch?v=Bdo_Z-tj_T8)]
  - Enable/Disable TDDStudio
  - Run build/test cycle on every change to any file under the solution folder
  - Incrementalv0 - i.e. build/test happen only for projects that have changed
  - Show code coverage indicators next to each line [green => tests covering it have all passed, red => otherwise]
  - nCrunch style status indicator at the bottom-right in VS Status Bar.

# TODO
  - disable tdd, takes icon away, persist settings
  - generalize test discovery and execution by loading test adapter from project output dir 
  - hardcoding: d:\xcrunch, c:\..\msbuild.exe
  - FxCop, StyleCop, FSharpLint and AppVeyor
  - telemetry
  - perf
  - hover to see List of tests, status, errors
  - click on icon -> Window with build + test errors, status - messages from build, test
  - run/debug single test
  - quicker feedback: incremental tests, failed tests fast
  - test: itself, xunit, nunit, opencover, roslyn

# Acceptance Tests
  - Help About - icon, version, name
  - Extensions and Updates - icon, version, name
  - Vanilla project: Auto trigger, SequencePoint markers show, Icon changes state
  - Notification Icon
    - status icon on build failure
    - status icon on test failure
    - ? status icon on both
    - status icon on close project
    - enable shows indicator, disable takes indicator away
  - Test Start Markers
    - Glyph
      - No lines on project open
      - White marker appear mid-way during build
      - Green markers for passing tests
      - Red for failing tests
      - Red for failing theories
      - Green for making the tests pass again
      - Zoom at 400 and back
    - Context Menu
      - Right click in design-mode -> breakpoint is inserted -> bp is hit
      - Right click in debug-mode -> Empty context menu appears
    - Tooltip
    - Mouse
  - Failure Point Markers
    - Glyph
      - No lines on project open
      - Red marker appear for point of throw
      - Red marker appear for caller of point of throw
      - Goes away when the throw is fixed
      - Zoom back to 70 and back
    - Context Menu
    - Tooltip
    - Mouse
  - Code Coverage Markers
    - Glyph
      - No lines on project open
      - White markers appear mid-way during build
      - All green for passing tests
      - Red for code impacted by failing tests
      - Red for code impacted by failing theories
      - Green lines on making the tests pass
      - White for uncovered lines
    - Context Menu
      - Right click in design-mode -> breakpoint is inserted -> bp is hit
      - Right click in debug-mode -> Empty context menu appears
    - Tooltip
    - Mouse
  - C#/F#/VB, xunit/nunit

  [[Markdown](http://daringfireball.net/projects/markdown/) syntax]

  # Feature Set #
  - Unit Test Frameworks [nUnit, xUnit, VS' CppUnit] 
  - Languages [C++/C#/F#] 
  - Hosts [VS2013/VS2015/Atom/VSCode] 
  - Editor enhancements [Code Coverage, Test Markers] 
  - Automated build and run test 
  - Debug first failing 1 test 
  - Test List 
  - Automated change detection
