# Task Parallel Library for .NET Compact Framework 3.5Provides a Task class for Compact Framework 3.5 with similar functionality asprovided by Framework 4. For another framework it simply forwards the type tocorresponding assembly.The package includes:* Task for executing asynchronous operations.* SpinWait for faster spinning logic.* SynchronizationContext for propagating a synchronization context.* TaskEx provides static methods for Task as provided by Framework 4.5 and 4.6.* ThreadEx provides cross-platform Sleep method.Tested on Windows CE and Pocket PC.## AuthorsFabrício Godoy <skarllot@gmail.com>## Dependencies### .NET Compact Framework 3.5* mscorlib* System* System.Core### .NET Framework 3.5* mscorlib* System* System.Core* *TaskParallelLibrary*### .NET Framework 4* Microsoft.CSharp* mscorlib* System* System.Core* *Microsoft.Bcl.Async*### .NET Framework from 4.5 to 4.6.2* Microsoft.CSharp* mscorlib* System* System.Core### .NETPortable,Version=v4.0,Profile=Profile328* mscorlib* System* System.Core### .NETPortable,Version=v4.5,Profile=Profile259* mscorlib* System* System.Core* System.Diagnostics.Debug* System.Globalization* System.Reflection* System.Resources.ResourceManager* System.Runtime* System.Threading* System.Threading.Tasks### .NETStandard v1.0 and v1.3* *System.Collections** *System.Diagnostics.Debug** *System.Resources.ResourceManager** *System.Threading** *System.Threading.Tasks*## Version HistoryVersion | Last Updated--- | ---1.0 | 2016-09-211.1 | 2016-10-112.0 | 2016-11-083.0 | 2016-12-21