echo off
cls
cd .paket
paket.bootstrapper.exe
paket.exe update
pause