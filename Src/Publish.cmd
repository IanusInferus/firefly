@echo off
@PATH %ProgramFiles%\WinRar;%PATH%

@pushd ..\..
@for %%* in (.) do set PackName=%%~n*
@popd

::@set PackName=PackageName

@call Clear.cmd
@cd ..
@if not exist ..\Versions\ md ..\Versions\
@rar a -av- -m5 -md4096 -tsm -tsc -s -k -t ..\Versions\%PackName%.rar -x*\.*\ -x*.user -x*.suo Src Bin Examples
