rd obj /S /Q
cd..
cd Bin
del *.pdb /F /Q
del *.xml /F /Q
del *.vshost.exe /F /Q
del *.manifest /F /Q
del *.user /F /Q
rd Debug /S /Q
rd Release /S /Q
cd..
cd Src
attrib -H PackageManager.suo
del *.suo /F /Q
del *.cache /F /Q
del *.ncb /F /Q
del *.sdf /F /Q
del *.user /F /Q
pause
