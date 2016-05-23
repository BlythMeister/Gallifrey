echo off
cls

SET origPath = %PATH%
if exist "%ProgramFiles%\MSBuild\14.0\bin" set PATH=%ProgramFiles%\MSBuild\14.0\bin;%PATH%
if exist "%ProgramFiles(x86)%\MSBuild\14.0\bin" set PATH=%ProgramFiles(x86)%\MSBuild\14.0\bin;%PATH%
if exist Output RMDIR Output /q/s
.paket\paket.bootstrapper.exe
.paket\paket.exe restore
msbuild src\Gallifrey.sln /t:Clean,Rebuild /p:Configuration=Release
set PATH=%origPath%
pause