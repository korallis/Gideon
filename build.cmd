@echo off
setlocal enabledelayedexpansion

REM Gideon WPF Build Script (Windows Batch)
REM Simple wrapper for common build operations

echo.
echo ===================================
echo Gideon WPF Build Automation
echo ===================================
echo.

REM Check if PowerShell is available (preferred)
where pwsh >nul 2>&1
if %ERRORLEVEL% == 0 (
    echo Using PowerShell Core...
    pwsh -ExecutionPolicy Bypass -File "%~dp0build.ps1" %*
    goto :end
)

where powershell >nul 2>&1
if %ERRORLEVEL% == 0 (
    echo Using Windows PowerShell...
    powershell -ExecutionPolicy Bypass -File "%~dp0build.ps1" %*
    goto :end
)

REM Fallback to direct dotnet commands if no PowerShell
echo PowerShell not found, using fallback mode...

set TARGET=%1
if "%TARGET%"=="" set TARGET=Dev

if /i "%TARGET%"=="help" goto :help
if /i "%TARGET%"=="clean" goto :clean
if /i "%TARGET%"=="dev" goto :dev
if /i "%TARGET%"=="release" goto :release

:help
echo.
echo Available targets:
echo   help     - Show this help
echo   clean    - Clean build artifacts
echo   dev      - Development build
echo   release  - Release build
echo.
echo Examples:
echo   build.cmd dev
echo   build.cmd release
echo   build.cmd clean
goto :end

:clean
echo Cleaning build artifacts...
dotnet clean "Gideon.WPF\Gideon.WPF.csproj"
rmdir /s /q artifacts 2>nul
rmdir /s /q Gideon.WPF\bin 2>nul
rmdir /s /q Gideon.WPF\obj 2>nul
echo Clean completed.
goto :end

:dev
echo Development build...
dotnet restore "Gideon.WPF\Gideon.WPF.csproj"
dotnet build "Gideon.WPF\Gideon.WPF.csproj" --configuration Debug
goto :end

:release
echo Release build...
dotnet restore "Gideon.WPF\Gideon.WPF.csproj"
dotnet build "Gideon.WPF\Gideon.WPF.csproj" --configuration Release
goto :end

:end
echo.
if %ERRORLEVEL% == 0 (
    echo Build completed successfully!
) else (
    echo Build failed with error code %ERRORLEVEL%
)