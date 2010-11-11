PATH %SystemRoot%\Microsoft.NET\Framework\v4.0.30319;%PATH%

MSBuild FireflyDoc.shfbproj /t:Rebuild /p:Configuration=Release

pause
