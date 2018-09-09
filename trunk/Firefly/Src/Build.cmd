PATH %ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin;%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin;%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin;%ProgramFiles(x86)%\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin;%PATH%

MSBuild Firefly.sln /t:Rebuild /p:Configuration=Release

copy Doc\Readme.*.txt ..\Bin\
copy Doc\UpdateLog.*.txt ..\Bin\
copy Doc\License.*.txt ..\Bin\

pause
