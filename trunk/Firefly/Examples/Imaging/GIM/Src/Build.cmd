PATH %SystemRoot%\Microsoft.NET\Framework\v4.0.30319;%PATH%

MSBuild /t:Rebuild /p:Configuration=Release
copy ..\*.MIG ..\Bin\
copy ..\*.GIM ..\Bin\
pause
