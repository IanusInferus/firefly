@echo off
@PATH %ProgramFiles%\WinRar;%PATH%

@pushd ..
@for %%* in (.) do set PackName=%%~n*
@popd

::@set PackName=PackageName

@call Clear.cmd
@cd ..
@del %PackName%.rar
@rar a -av- -m5 -md4096 -tsm -tsc -s -k -t %PackName%.rar -x*\.svn -x*\.svn\* -x*.user -x*.suo Src Bin Manual Examples
@if not exist Versions\ md Versions\
@copy %PackName%.rar Versions\