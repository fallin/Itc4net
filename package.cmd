@echo off
@cd /d "%~dp0"

echo info: Package and release Itc4net NuGet package

echo info: Download vswhere (to locate msbuild)
mkdir .\build
set VSWHERE_URL=https://github.com/Microsoft/vswhere/releases/download/1.0.62/vswhere.exe
@powershell -NoProfile -ExecutionPolicy Bypass -Command "Invoke-WebRequest -Uri %VSWHERE_URL% -OutFile '.\build\vswhere.exe'"
if ERRORLEVEL 1 (
    echo error: Failed to download vswhere
    goto EXIT
)

:: See https://github.com/Microsoft/vswhere
for /f "usebackq tokens=1* delims=: " %%i in (`.\build\vswhere -latest -requires Microsoft.Component.MSBuild`) do (
  if /i "%%i"=="installationPath" set VSCOMNTOOLS=%%j
)

echo info: Setup MSBuild environment
:: This batch script modifies the current directory
set PRESERVE_CURRENT_DIRECTORY=%~dp0
call "%VSCOMNTOOLS%\Common7\tools\VsMSBuildCmd.bat"
cd /d %PRESERVE_CURRENT_DIRECTORY%
set PRESERVE_CURRENT_DIRECTORY=

echo info: Build Itc4net
msbuild.exe ".\src\Itc4net.sln" /p:Configuration=Release /t:Rebuild
if ERRORLEVEL 1 (
    echo error: Failed to build Itc4net
    goto EXIT
)

xcopy /y /r ".\src\Itc4net\bin\Release\*.nupkg" ".\build"

:: echo info: Push NuGet nupkg
:: set /p APIKEY="Enter NuGet ApiKey:"
:: for /f %%b IN ('dir /b *.nupkg') do (
::     nuget push "%%b" -ApiKey %APIKEY% -Source https://www.nuget.org/api/v2/package
:: )

:EXIT
pause
