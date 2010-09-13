@echo off
@PATH %ProgramFiles%\WinRar;%PATH%

@pushd ..
@for %%* in (.) do set PackName=%%~n*
@popd

::@set PackName=PackageName

@call Clear.cmd
@cd ..
@del %PackName%Src.rar
@del %PackName%Bin.rar
@rar a -av- -m5 -md4096 -tsm -tsc -s -k -t %PackName%Src.rar Src
@rar a -av- -m5 -md4096 -tsm -tsc -s -k -t %PackName%Bin.rar Bin
@if not exist Versions\ md Versions\
@copy %PackName%Src.rar Versions\
