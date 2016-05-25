echo off
cls
ECHO Restoring Packages
if exist Output RMDIR Output /q/s
.paket\paket.bootstrapper.exe
.paket\paket.exe restore

ECHO Trying Build On PATH
msbuild.exe src\Gallifrey.sln /t:Clean,Rebuild /p:Configuration=Release

if NOT %ERRORLEVEL% == 9009 (
	goto:END
)

ECHO Trying To Locate MSBuild
SET origPath = %PATH%
if exist "%ProgramFiles%\MSBuild\14.0\bin" set PATH=%ProgramFiles%\MSBuild\14.0\bin;%PATH%
if exist "%ProgramFiles(x86)%\MSBuild\14.0\bin" set PATH=%ProgramFiles(x86)%\MSBuild\14.0\bin;%PATH%
msbuild.exe src\Gallifrey.sln /t:Clean,Rebuild /p:Configuration=Release
set PATH=%origPath%

:END
ECHO Finished