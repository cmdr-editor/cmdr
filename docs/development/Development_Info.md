# Development Info

Building and Development info goes here

## Quickest editing

The quickest way to edit is to change directly "Traktor Settings.tsi", and run traktor without a collection.
When you finish edits, just restart traktor (no need to import mappings, etc)

* backup collectio  n.nml
* open Traktor
  * remove whole collection
* open cmdr
  * open TraktorSettings.tsi
  * do edits
* traktor
  * close traktor
* cmdr
  * save file with edits
* restart traktor

* repeat editing loop with cmdr always open

## Development Quick Start

The simplest way to get started is to install VS2017 and implement a small feature via "cloning".
This ensures the WPF "glue" will work immediatly.

Another simple approach is to read the check-ins of small features 
(small changes often touch a lot of files because of the WPF "glue")

## Building using VS2017

* install VS2017 community edition. Do not install VS2019! 
* install .net and c# development pack 
* get blend for windows as an individual component
* Integrate VS2017 with github: [link](https://blogs.msdn.microsoft.com/benjaminperkins/2017/04/04/setting-up-and-using-github-in-visual-studio-2017/)

## Forum Threads:

*  https://www.native-instruments.com/forum/threads/release-cmdr-controller-manager-done-right.269818/page-2
*  https://forum.djtechtools.com/showthread.php?t=91303
*  https://forum.djtechtools.com/showthread.php?t=84669
    

## Specific Versions used (as of Dec 2019):
	Microsoft Visual Studio Community 2017 
	Version 15.9.13
	VisualStudio.15.Release/15.9.13+28307.718
	Microsoft .NET Framework
	Version 4.8.03752
	Installed Version: Community

	C# Tools   2.10.0-beta2-63501-03+b9fb1610c87cccc8ceb74a770dba261a58e39c4a
	Common Azure Tools   1.10
	NuGet Package Manager   4.6.0
	ProjectServicesPackage Extension   1.0
	ResourcePackage Extension   1.0
	ResourcePackage Extension   1.0
	Visual Basic Tools   2.10.0-beta2-63501-03+b9fb1610c87cccc8ceb74a770dba261a58e39c4a
	Visual Studio Code Debug Adapter Host Package   1.0

## Visual studio tips 

Cant change code on the fly in the debugger:
 - ensure code is specifically 32 bit:
   -  https://stackoverflow.com/questions/10113532/how-do-i-fix-the-visual-studio-compile-error-mismatch-between-processor-archit

     
Auto sync solution explorer:
  Tools->Options->Projects and Solutions->Track Active Item in Solution Explorer  

  
   
## Visual studio extensions 
  
   
* RealClean extension: https://marketplace.visualstudio.com/items?itemName=FlorentGoetz.RealCleanExt

* SelectNext extension: https://marketplace.visualstudio.com/items?itemName=thomaswelen.SelectNextOccurrence

* MArkdown editor:
  
* Indent Guides: 
* github support: 

* Disable No Source Available Tab
  
  
## C# Tutorials:
*  XAML binding: https://www.c-sharpcorner.com/article/explain-binding-mode-in-wpf/
*  sorting a aclasS: https://csharp.net-tutorials.com/linq/sorting-data-the-orderby-thenby-methods/
*  Datagrid to text file: https://stackoverflow.com/questions/56138612/c-sharp-save-datagridview-to-text-file
*  Filtering datagrid: https://www.codeproject.com/Articles/41755/Filtering-the-WPF-DataGrid-automatically-via-the-h
  
C# for python programmers:
*  https://gist.github.com/mrkline/8302959
*  https://www.neowin.net/forum/topic/1216223-going-to-c-from-heavy-python-experience/
*  https://education.launchcode.org/skills-back-end-csharp/csharp4python/
  
  
    
## Visual Studio debugger tutorial:
* https://docs.microsoft.com/en-us/visualstudio/debugger/debugger-tips-and-tricks?view=vs-2019
* https://devblogs.microsoft.com/visualstudio/7-lesser-known-hacks-for-debugging-in-visual-studio/
* https://devblogs.microsoft.com/visualstudio/7-hidden-gems-in-visual-studio-2017/
    

WPF FAQ:
  https://social.msdn.microsoft.com/Forums/en-US/a2988ae8-e7b8-4a62-a34f-b851aaf13886/windows-presentation-foundation-faq?forum=wpf#search_text
  
  
  
DotNet Fiddle:
  https://dotnetfiddle.net/  
  
## C# 7.3 info

* https://stackoverflow.com/questions/203377/getting-the-max-value-of-an-enum 
* https://www.davidyardy.com/blog/visual-studio%E2%80%93how-to-target-differentlatest-c-version-net-core-3-and-c-8/
* https://devblogs.microsoft.com/dotnet/a-belated-welcome-to-c-7-3/

        
  
## git cheatsheet

This is how the filtering remote branch was merged.
  git clone "https://github.com/TomWeps/cmdr.git"
  git branch -a -vv
  git checkout remotes/origin/feature/filtering  

  
## WPF Filtering feature

pull request: https://github.com/pestrela/cmdr/commit/b7b6312bf4e9fe7649e1d8a88d6b71a910489465
original WPF example: https://www.codeproject.com/Articles/41755/Filtering-the-WPF-DataGrid-automatically-via-the-h
unchanged datagrid tutorial: https://www.wpftutorial.net/DataGrid.html 


VS2017 Keyboard shortcuts
  CTRL+J - find references
  CTRL+SHIFT+J - search files
  CTRL+G - format document
  CTRL+B  - breakpoint
  CTRL+E, C - comment
  CTRL+E, U - uncomment
  CTRL+O - Options
  CTRL+; - go to solution explorer
  
  
Traktor global midi controls:  
  https://djtechtools.com/2015/09/08/traktor-global-midi-control-control-multiple-midi-devices-one-controller/
  
WPF tutorial:

  https://docs.microsoft.com/en-us/dotnet/framework/wpf/getting-started/walkthrough-my-first-wpf-desktop-application

    Canvas - children positied by coordinates relative to the Canvas area.
    DockPanel - child elements arranged horizontally or vertically relative to each other.
    Grid - Organizes columns and rows.
    StackPanel - Children are a single horizontal or vertical line 
    VirtualizingStackPanel - ?
    WrapPanel - child in sequential position from left to right, breaking content to the next line 

datagrid tutorial:
  https://www.wpftutorial.net/DataGrid.html    
  fundamentals: https://www.wpftutorial.net/ArchitecturePattern.html

  
XAML designer info:
  https://docs.microsoft.com/en-us/visualstudio/xaml-tools/creating-a-ui-by-using-xaml-designer-in-visual-studio?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev15.query%3FappId%3DDev15IDEF1%26l%3DEN-US%26k%3Dk(VS.XamlEditor);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.5)%26rd%3Dtrue&view=vs-2019
  

## Github
  how to get a staic download link to latets releae:
    https://www.rarst.net/code/link-latest-github-release-binary/
    
  
# out of memory situations
    https://stackoverflow.com/questions/1153702/system-outofmemoryexception-was-thrown-when-there-is-still-plenty-of-memory-fr
 
    https://msdn.microsoft.com/en-us/library/system.outofmemoryexception(v=vs.110).aspx
 
    https://www.codeproject.com/Questions/224672/Exception-of-type-System-OutOfMemoryException-was

    
## WPF C# console:
  https://github.com/Iron-YeHong/.NETUtilities
  http://www.programmersought.com/article/39642139957/
  https://stackoverflow.com/questions/160587/no-output-to-console-from-a-wpf-application
   
   

## C# new operators:
 
* https://stackoverflow.com/questions/1660269/empty-string-if-null
* https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operators#null-conditional-operators--and-
* https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-coalescing-operator
   
* A ?? B 
  *  ==>  if(A!=null) then A else B
* A?.B 
  * ==>  if(A!=null) then A.B else NULL
      
      
    
## What is the fastest way to edit mappings:

I've found that the fastest to edit mappings is to directly edit your settings, having an empty collection, and restart whole traktor every time.

Specific steps:
- run CMDR first
- open Traktor Settings.TSI directly
- Use an empty collection (everything is backed-up on root/backups/collection)
- create versioned backups of the TSI (CTRL+B)
- Open traktor:
  - Fix bgs in the mapping
  - Only use the controller editor for on-off tests where you need to see the modifier state, etc
  - When ready to retest, close tracktor and then save in CMDR
  