@echo off

set tardir=Y:\
set reladir=.\bin\Debug
set RefDir=.\References

copy %reladir%\CameraApp.exe %tardir%
copy %RefDir%\*.dll %tardir%

rem copy %reladir%\CameraApp.pdb %tardir%

