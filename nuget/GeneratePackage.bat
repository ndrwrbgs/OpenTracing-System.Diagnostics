@echo off

echo This script must be run from 'Developer Command Prompt for VS 2017' or similar developer environment.
msbuild %~dp0\..\src\Library\OpenTracing-System.Diagnostics.csproj /p:Configuration=Release /v:m

REM exit if if failed
if %errorlevel% neq 0 exit /b %errorlevel%
xcopy /Y %~dp0\..\src\Library\bin\Release\net4.5\OpenTracing.Contrib.SystemDiagnostics.dll %~dp0\lib\net45
xcopy /Y %~dp0\..\src\Library\bin\Release\net4.5\OpenTracing.Contrib.SystemDiagnostics.pdb %~dp0\lib\net45

echo.
echo.
nuget pack %~dp0\Package.nuspec -OutputDirectory %~dp0