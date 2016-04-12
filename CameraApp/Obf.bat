@echo off
rem 适合单执行文件的obf
echo by hzx 2016
echo 配置文件为obf*.xml
echo.

set OBF_EXE=D:\window\dev\Obfuscar2.1\Obfuscar.Console.exe
set OUT_DIR=Publish
set OBJ_FILE=CameraApp.exe

cd %~dp0
if /i "%1" == "release" (goto obf_release)
goto obf_debug

:obf_release
echo publishing Release ...
set IN_DIR=.\bin\Release
IF NOT EXIST %IN_DIR%\%OBJ_FILE% goto ERR_OBJ
@md %OUT_DIR% > nul
del %OUT_DIR%\ /s/q > nul
%OBF_EXE% obf_release.xml
goto copy_ext

:obf_debug
echo publishing Debug ...
set IN_DIR=.\bin\Debug
IF NOT EXIST %IN_DIR%\%OBJ_FILE% goto ERR_OBJ
IF NOT EXIST %OUT_DIR% (
@md %OUT_DIR% > nul
)
del %OUT_DIR%\ /s/q > nul
%OBF_EXE% obf_debug.xml
REM 测试版只进行混淆，不拷贝相关文件
REM goto copy_ext
goto done

:copy_ext
IF NOT EXIST %OUT_DIR%\voice (
md %OUT_DIR%\voice > nul
)
@copy voice\*  %OUT_DIR%\voice\
copy %IN_DIR%\%OBJ_FILE%.config  %OUT_DIR%\
copy .\References\*.dll  %OUT_DIR%\
rem copy %IN_DIR%\CameraApp.pdb  %OUT_DIR%\
goto done

:ERR_OBJ
echo %OBJ_FILE% 文件不存在!

:done
IF EXIST %OUT_DIR%\Mapping.txt (
@del %OUT_DIR%\Mapping.txt > nul
)
set OBF_EXE=
set IN_DIR=
set OUT_DIR=
set OBJ_FILE=
echo 完成。
