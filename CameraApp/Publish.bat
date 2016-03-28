rem %windir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe obf.proj
@echo off
echo 正在生成发布版......
echo.
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe obf.proj
@del publish\Mapping.txt >nul
echo 生成完成。
echo.
