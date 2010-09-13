PATH %windir%\Microsoft.NET\Framework\v4.0.30319;%PATH%

MSBuild /t:Rebuild /p:Configuration=Release
pause
