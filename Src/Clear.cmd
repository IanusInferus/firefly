@rd "FireflyCore\My Project" /S /Q
@for /d %%a in (*) do @(
  @if exist %%a\obj @(
    @rd %%a\obj /S /Q
  )
)
@cd..
@if exist Bin (
  @cd Bin
  @for %%a in (*.exe;*.dll) do @(
    @echo %%a | findstr /R "Firefly\..*\.dll" > nul
    @if errorlevel 1 @(
      @del %%~na.pdb /F /Q
      @del %%~na.xml /F /Q
    )
  )
  @del *.vshost.exe /F /Q
  @del *.manifest /F /Q
  @del *.CodeAnalysisLog.xml /F /Q
  @del *.lastcodeanalysissucceeded /F /Q
  @del Test.* /F /S /Q
  @cd..
)
@cd Src
@del *.cache /F /Q
@pause
