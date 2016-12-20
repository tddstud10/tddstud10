[![Join the chat at https://gitter.im/parthopdas/tddstud10](https://badges.gitter.im/parthopdas/tddstud10.svg)](https://gitter.im/parthopdas/tddstud10?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

> **Test Driven Development Studio - An environment for practicing Kent Beck style Test Driven Development.**

# Project Components
  - Main: This repo, for work item tracking and discussions related to the project. Also for source code history.
  - VS: Plugin for VS2013, VS2015 [![Build status](https://ci.appveyor.com/api/projects/status/561lghv3qmarn2fn/branch/master?svg=true)](https://ci.appveyor.com/project/TddStud10/vs/branch/master)
  - VSCode (Work has just started, no ETA. Help needed!!!)
  - Common: Common Library for all components. [![Build status](https://ci.appveyor.com/api/projects/status/dd2cnas76bgku651/branch/master?svg=true)](https://ci.appveyor.com/project/TddStud10/common/branch/master)
  - Engine: Workflow coordinator. [![Build status](https://ci.appveyor.com/api/projects/status/j18x5rn2lbqhsb4s/branch/master?svg=true)](https://ci.appveyor.com/project/TddStud10/engine/branch/master)
  - TestHost: Test host, runner and runtime. [![Build status](https://ci.appveyor.com/api/projects/status/6s52sajc86x0vvj6/branch/master?svg=true)](https://ci.appveyor.com/project/TddStud10/testhost/branch/master)

# Goals, Intentions, Guidelines

  - Environment for practicing Kent Beck style TDD [F.I.R.S.T. Unit Tests, fast builds, No Mocks, Hexagonal architecture]
  - Open source alternative to nCrunch
  - Eventually not just unit tests - but a collection of facilities that makes TDD a joy! e.g. TODO list, hotspot analysis, etc.

# Features that currently work 
  - Supported on VS2013 & VS2015
  - Enable/Disable TDDStudio
  - Ability to ignore tests (e.g. long range/acceptance tests, using a setting in .sln.tddstud10.user)
  - Multiple Unit Test Frameworks [nUnit v2/v3, xUnit v1/v2]
  - Multiple Languages [C#/F#/VB]
  - Run build/test cycle on every change to any file under the solution folder
  - Incrementalv0 - i.e. build/test happen only for projects that have changed
  - Show code coverage indicators next to each line [green => tests covering it have all passed, red => otherwise]
  - nCrunch style status indicator at the bottom-right in VS Status Bar.
  - Each coverage marker has a rich list of covering tests with (a) error message/stacktrace (b) option to debug test

# Demo
  [![TDD Studio - Making TDD a joy!](http://img.youtube.com/vi/Bdo_Z-tj_T8/0.jpg)](https://www.youtube.com/watch?v=Bdo_Z-tj_T8 "TDD Studio - Making TDD a joy!")

# Wish list and stuff really far off into the future #
  - Other Unit Test Frameworks [VS' CppUnit] 
  - Languages [C++] 
  - Hosts [Atom/VSCode] 
  - Test List 
  - Automated change detection
