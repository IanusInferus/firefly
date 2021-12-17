@echo off

setlocal EnableDelayedExpansion
if "%SUB_NO_PAUSE_SYMBOL%"=="1" set NO_PAUSE_SYMBOL=1
if /I "%COMSPEC%" == %CMDCMDLINE% set NO_PAUSE_SYMBOL=1
set SUB_NO_PAUSE_SYMBOL=1
call :main %*
set EXIT_CODE=%ERRORLEVEL%
if not "%NO_PAUSE_SYMBOL%"=="1" pause
exit /b %EXIT_CODE%

:main
for %%f in ("%ProgramFiles%" "%ProgramFiles(x86)%") do (
  for %%v in (2022 2019) do (
    for %%p in (Enterprise Professional Community BuildTools) do (
      if exist "%%~f\Microsoft Visual Studio\%%v\%%p\MSBuild\Current\Bin\MSBuild.exe" (
        set MSBuild="%%~f\Microsoft Visual Studio\%%v\%%p\MSBuild\Current\Bin\MSBuild.exe"
        goto MSBuild_Found
      )
    )
  )
)
echo MSBuild not found.
echo You need to install Visual Studio 2019/2022 or add MSBuild environment variable.
exit /b 1
:MSBuild_Found

%MSBuild% /t:Rebuild /p:Configuration=Release || exit /b 1
