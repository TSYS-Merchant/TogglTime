@echo off
cd %~dp0

SETLOCAL
SET CACHED_NUGET=%LocalAppData%\NuGet\NuGet.exe

IF EXIST %CACHED_NUGET% goto copynuget
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%CACHED_NUGET%'"

:copynuget
IF EXIST .nuget\nuget.exe goto build
md .nuget
copy %CACHED_NUGET% .nuget\nuget.exe > nul

:build
call .nuget\nuget.exe restore
call msbuild TogglTime.sln /p:Configuration=Release

:copybuild
IF EXIST Release rd /S /Q Release
echo d | xcopy src\TogglTime\bin\Release Release /y /d
