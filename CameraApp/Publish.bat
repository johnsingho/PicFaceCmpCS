rem %windir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe obf.proj
@echo off
echo �������ɷ�����......
echo.
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe obf.proj
@del publish\Mapping.txt >nul
echo ������ɡ�
echo.
