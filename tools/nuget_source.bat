REM Capture parameters
set ScriptDir=%~dp0
set SolutionDir=%~1
set AssemblyName=%~2
set ProjectRoot=%~3

set nuget_folder=%SolutionDir%.nuget\%AssemblyName%

rmdir /s/q "%nuget_folder%\src" > nul 2>&1
rmdir /s/q "%nuget_folder%\lib" > nul 2>&1

mkdir "%nuget_folder%\src\%AssemblyName%" || EXIT /B 1
xcopy %ProjectRoot%\%AssemblyName% %nuget_folder%\src\%AssemblyName% /sy /EXCLUDE:%ScriptDir%nuget_source_ignore.txt > nul
