PATH %SystemRoot%\Microsoft.NET\Framework\v4.0.30319;%PATH%

MSBuild /t:Rebuild /p:Configuration=Release /p:VCTargetsPath="%ProgramFiles(x86)%\MSBuild\Microsoft.Cpp\v4.0\V120
pause
