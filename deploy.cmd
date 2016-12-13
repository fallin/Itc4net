@echo off
@cd /d "%~dp0"

echo info: Setup MSBuild environment
call "%VS140COMNTOOLS%VsMSBuildCmd.bat"

echo info: Build Itc4net
msbuild.exe ".\src\Itc4net.sln" /p:Configuration=Release /t:Rebuild
if ERRORLEVEL 1 (
    echo error: Failed to build Itc4net
    goto EXIT
)

echo info: Pack NuGet nuspec
for /f %%a IN ('dir /b *.nuspec') do (
	nuget pack "%%a"
)

echo info: Push NuGet nupkg
set /p APIKEY="Enter NuGet ApiKey:"
for /f %%b IN ('dir /b *.nupkg') do (
	nuget push "%%b" -ApiKey %APIKEY% -Source https://www.nuget.org/api/v2/package
)

mkdir .\build
move /y *.nupkg .\build

:EXIT
pause
