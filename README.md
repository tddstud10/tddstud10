[![Build status](https://ci.appveyor.com/api/projects/status/ustuq9veqf22qenb/branch/master?svg=true)](https://ci.appveyor.com/project/parthopdas/tddstud10/branch/master)

> **Test Driven Development Studio - An environment for practicing Kent Beck style Test Driven Development.**
>
>  Copyright (c) 2015-3015 Partho P. Das
>
>  Licensed under the Apache License, Version 2.0 (the "License");
>  you may not use this file except in compliance with the License.
>  You may obtain a copy of the License at
>
>      http://www.apache.org/licenses/LICENSE-2.0
>
>  Unless required by applicable law or agreed to in writing, software
>  distributed under the License is distributed on an "AS IS" BASIS,
>  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
>  See the License for the specific language governing permissions and
>  limitations under the License.

# Goals, Intentions, Guidelines
  - Environment for practicing Kent Beck style TDD [F.I.R.S.T. Unit Tests, fast builds, No Mocks, Hexagonal architecture]
  - Open source alternative to nCrunch
  - Eventually not just unit tests - but a collection of facilities that makes TDD a joy! e.g. TODO list, hotspot analysis, etc.

# Features that currently work 
  - Enable/Disable TDDStudio
  - Run build/test cycle on every change to any file under the solution folder
  - Incrementalv0 - i.e. build/test happen only for projects that have changed
  - Show code coverage indicators next to each line [green => tests covering it have all passed, red => otherwise]
  - nCrunch style status indicator at the bottom-right in VS Status Bar.

# Demo
  [![Everything Is AWESOME](http://img.youtube.com/vi/Bdo_Z-tj_T8/0.jpg)](https://www.youtube.com/watch?v=Bdo_Z-tj_T8 "TDD Studio - Making TDD a joy!")

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

  # Wish list and stuff really far off into the future #
  - Unit Test Frameworks [nUnit, xUnit, VS' CppUnit] 
  - Languages [C++/C#/F#] 
  - Hosts [VS2013/VS2015/Atom/VSCode] 
  - Editor enhancements [Code Coverage, Test Markers] 
  - Automated build and run test 
  - Debug first failing 1 test 
  - Test List 
  - Automated change detection
