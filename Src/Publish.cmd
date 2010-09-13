@echo off
@PATH %ProgramFiles%\WinRar;%PATH%

@pushd ..
@for %%* in (.) do set PackName=%%~n*
@popd

::@set PackName=PackageName

@call Clear.cmd
@del ..\Bin\System.Core.dll /F /Q
@del ..\Bin\System.Speech.dll /F /Q
@del ..\Bin\System.Xml.Linq.dll /F /Q
@rd ..\Bin\zh-CHS /S /Q
@cd ..
@del %PackName%.rar
@rar a -av- -m5 -md4096 -tsm -tsc -s -k -t %PackName%.rar -x*\.svn -xSrc\*.user Src Bin Manual Examples
@if not exist Versions\ md Versions\
@copy %PackName%.rar Versions\
