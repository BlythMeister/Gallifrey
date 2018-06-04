ECHO OFF
cls
.paket\paket.exe restore
packages\FAKE\tools\FAKE.exe .build\build.fsx