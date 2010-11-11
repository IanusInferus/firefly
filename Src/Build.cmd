PATH %SystemRoot%\Microsoft.NET\Framework\v4.0.30319;%PATH%

MSBuild Firefly.sln /t:Rebuild /p:Configuration=Release

copy Doc\Readme.*.txt ..\Bin\
copy Doc\UpdateLog.*.txt ..\Bin\
copy Doc\License.*.txt ..\Bin\

pause
