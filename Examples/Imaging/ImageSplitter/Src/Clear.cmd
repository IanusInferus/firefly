rd "My Project" /S /Q
rd obj /S /Q
attrib -H ImageSplitter.suo
del *.suo /F /Q
del *.cache /F /Q
cd..
cd Bin
del *.pdb /F /Q
del *.xml /F /Q
del *.vshost.exe /F /Q
del *.manifest /F /Q
del *.user /F /Q
pause
