echo off
cls
cd .paket
paket.bootstrapper.exe
paket.exe restore
pause