@echo off

for %%p in (Enterprise Professional Community BuildTools) do (
  if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\%%p" (
    set VSDir="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\%%p"
  )
)
set VSDir=%VSDir:"=%
echo VSDir=%VSDir%

"%VSDir%\MSBuild\15.0\Bin\MSBuild.exe" Firefly.sln /t:Rebuild /p:Configuration=Release

copy Doc\Readme.*.txt ..\Bin\
copy Doc\UpdateLog.*.txt ..\Bin\
copy Doc\License.*.txt ..\Bin\

pause
