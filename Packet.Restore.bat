echo off
cls
cd .paket
paket.bootstrapper.exe --run restore
pause