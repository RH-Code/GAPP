GAPP
====

Globalcaching Application, the geocache manager


First Build
===========

- Open the solution in Visual Studio Express 2013
- Select target x86
- Build the GlobalcachingApplication project first (will fail)
- Build the GlobalcachingApplication project again (will succeed)
- Build the complete solution

The fact that you need to build the application first is that other projects copy their output to the output folder
of the application (like Plugins). Initially the folder does not exist.


Setup project
=============

The Setup project is a WiX project created in SharpDevelop 4.3 (or higher)
First build release of GAPP in Visual Studio Express 2013 for desired platform (x86 or x64)
Then build the setup in SharpDevelop 4.3 (or higher) for the same platform (x86 or x64)

Note
=============
GAPPSF project is a project that is in development and not functional yet.
