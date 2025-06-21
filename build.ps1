#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Gideon WPF Build Automation Script

.DESCRIPTION
    Provides automated build, test, and deployment functionality for the Gideon WPF application.
    Supports development and release builds with comprehensive quality checks.

.PARAMETER Target
    The build target to execute (Dev, Release, Clean, Test, Package, Help)

.PARAMETER Configuration
    Build configuration (Debug or Release)

.PARAMETER Verbosity
    MSBuild verbosity level (quiet, minimal, normal, detailed, diagnostic)

.EXAMPLE
    .\build.ps1 -Target Dev
    .\build.ps1 -Target Release -Configuration Release
    .\build.ps1 -Target Package -Configuration Release

.NOTES
    Author: Gideon Development Team
    Version: 2.0.0
#>

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("Dev", "Release", "Clean", "Test", "Package", "Help")]
    [string]$Target = "Dev",
    
    [Parameter(Mandatory = $false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    
    [Parameter(Mandatory = $false)]
    [ValidateSet("quiet", "minimal", "normal", "detailed", "diagnostic")]
    [string]$Verbosity = "normal",
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipTests = $false,
    
    [Parameter(Mandatory = $false)]
    [switch]$Force = $false
)

# Script Configuration
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Project Paths
$RootPath = $PSScriptRoot
$ProjectPath = Join-Path $RootPath "Gideon.WPF"
$ProjectFile = Join-Path $ProjectPath "Gideon.WPF.csproj"
$ArtifactsPath = Join-Path $RootPath "artifacts"
$LogsPath = Join-Path $ArtifactsPath "logs"

# Ensure directories exist
@($ArtifactsPath, $LogsPath) | ForEach-Object {
    if (-not (Test-Path $_)) {
        New-Item -Path $_ -ItemType Directory -Force | Out-Null
    }
}

# Utility Functions
function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host "=" * 60 -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Yellow
    Write-Host "=" * 60 -ForegroundColor Cyan
    Write-Host ""
}

function Write-Step {
    param([string]$Message)
    Write-Host "→ $Message" -ForegroundColor Green
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Invoke-DotNet {
    param(
        [string]$Command,
        [string]$WorkingDirectory = $RootPath,
        [switch]$SuppressOutput = $false
    )
    
    Write-Step "Running: dotnet $Command"
    
    $startTime = Get-Date
    
    try {
        if ($SuppressOutput) {
            $result = & dotnet $Command.Split(' ') 2>&1
            if ($LASTEXITCODE -ne 0) {
                throw "Command failed with exit code $LASTEXITCODE"
            }
        } else {
            & dotnet $Command.Split(' ')
            if ($LASTEXITCODE -ne 0) {
                throw "Command failed with exit code $LASTEXITCODE"
            }
        }
        
        $duration = (Get-Date) - $startTime
        Write-Success "Completed in $($duration.TotalSeconds.ToString("F1"))s"
    }
    catch {
        Write-Error-Custom "Failed: $_"
        throw
    }
}

function Test-Prerequisites {
    Write-Step "Checking prerequisites..."
    
    # Check .NET SDK
    try {
        $dotnetVersion = & dotnet --version 2>$null
        Write-Host "  .NET SDK: $dotnetVersion" -ForegroundColor Gray
    }
    catch {
        throw ".NET SDK not found. Please install .NET 9.0 SDK."
    }
    
    # Check project file
    if (-not (Test-Path $ProjectFile)) {
        throw "Project file not found: $ProjectFile"
    }
    
    # Check essential configuration files
    $requiredFiles = @(
        "global.json",
        "Directory.Packages.props",
        "Directory.Build.props",
        "StyleCop.ruleset",
        ".editorconfig"
    )
    
    foreach ($file in $requiredFiles) {
        $filePath = Join-Path $RootPath $file
        if (-not (Test-Path $filePath)) {
            Write-Warning "Configuration file missing: $file"
        }
    }
    
    Write-Success "Prerequisites satisfied"
}

function Invoke-Clean {
    Write-Header "Cleaning Build Artifacts"
    
    # Clean using MSBuild target
    Invoke-DotNet "build `"$ProjectFile`" -t:FullClean --verbosity $Verbosity"
    
    # Additional cleanup for stubborn files
    $pathsToClean = @(
        (Join-Path $RootPath "artifacts"),
        (Join-Path $ProjectPath "bin"),
        (Join-Path $ProjectPath "obj")
    )
    
    foreach ($path in $pathsToClean) {
        if (Test-Path $path) {
            Write-Step "Removing: $path"
            Remove-Item $path -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
    
    Write-Success "Clean completed"
}

function Invoke-Restore {
    Write-Header "Restoring NuGet Packages"
    Invoke-DotNet "restore `"$ProjectFile`" --verbosity $Verbosity"
}

function Invoke-Build {
    Write-Header "Building Application"
    
    $buildArgs = @(
        "build `"$ProjectFile`""
        "--configuration $Configuration"
        "--verbosity $Verbosity"
        "--no-restore"
    )
    
    if ($Configuration -eq "Release") {
        $buildArgs += "--nologo"
    }
    
    Invoke-DotNet ($buildArgs -join " ")
}

function Invoke-Tests {
    if ($SkipTests) {
        Write-Host "Skipping tests (--SkipTests specified)" -ForegroundColor Yellow
        return
    }
    
    Write-Header "Running Tests"
    
    # Check if test projects exist
    $testProjects = Get-ChildItem -Path $RootPath -Filter "*.Tests.csproj" -Recurse
    
    if ($testProjects.Count -eq 0) {
        Write-Warning "No test projects found"
        return
    }
    
    foreach ($testProject in $testProjects) {
        Write-Step "Running tests: $($testProject.Name)"
        Invoke-DotNet "test `"$($testProject.FullName)`" --configuration $Configuration --no-build --verbosity $Verbosity"
    }
}

function Invoke-Package {
    Write-Header "Creating Package"
    
    $publishArgs = @(
        "publish `"$ProjectFile`""
        "--configuration $Configuration"
        "--output `"$ArtifactsPath\publish`""
        "--verbosity $Verbosity"
        "--no-restore"
        "--no-build"
    )
    
    Invoke-DotNet ($publishArgs -join " ")
    
    Write-Success "Package created in: $ArtifactsPath\publish"
}

function Show-Help {
    Write-Header "Gideon Build Script Help"
    
    Write-Host "TARGETS:" -ForegroundColor Yellow
    Write-Host "  Dev      - Development build (clean, restore, build, test)" -ForegroundColor Gray
    Write-Host "  Release  - Release build (clean, restore, build optimized)" -ForegroundColor Gray
    Write-Host "  Clean    - Clean all build artifacts" -ForegroundColor Gray
    Write-Host "  Test     - Run tests only" -ForegroundColor Gray
    Write-Host "  Package  - Create deployment package" -ForegroundColor Gray
    Write-Host "  Help     - Show this help" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "EXAMPLES:" -ForegroundColor Yellow
    Write-Host "  .\build.ps1 -Target Dev" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -Target Release -Configuration Release" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -Target Package -Configuration Release" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -Target Clean" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "OPTIONS:" -ForegroundColor Yellow
    Write-Host "  -Configuration  Debug|Release (default: Debug)" -ForegroundColor Gray
    Write-Host "  -Verbosity      quiet|minimal|normal|detailed|diagnostic (default: normal)" -ForegroundColor Gray
    Write-Host "  -SkipTests      Skip running tests" -ForegroundColor Gray
    Write-Host "  -Force          Force rebuild even if up-to-date" -ForegroundColor Gray
}

# Main Execution
try {
    $scriptStartTime = Get-Date
    
    Write-Header "Gideon WPF Build Automation"
    Write-Host "Target: $Target | Configuration: $Configuration | Verbosity: $Verbosity" -ForegroundColor Cyan
    
    # Always check prerequisites
    Test-Prerequisites
    
    switch ($Target) {
        "Help" {
            Show-Help
        }
        "Clean" {
            Invoke-Clean
        }
        "Test" {
            Test-Prerequisites
            Invoke-Tests
        }
        "Dev" {
            if ($Force) { Invoke-Clean }
            Invoke-Restore
            Invoke-Build
            Invoke-Tests
        }
        "Release" {
            $Configuration = "Release"
            if ($Force) { Invoke-Clean }
            Invoke-Restore
            Invoke-Build
            Invoke-Tests
        }
        "Package" {
            $Configuration = "Release"
            if ($Force) { Invoke-Clean }
            Invoke-Restore
            Invoke-Build
            Invoke-Tests
            Invoke-Package
        }
    }
    
    $totalDuration = (Get-Date) - $scriptStartTime
    Write-Header "Build Completed Successfully"
    Write-Success "Total time: $($totalDuration.TotalSeconds.ToString("F1"))s"
}
catch {
    $totalDuration = (Get-Date) - $scriptStartTime
    Write-Header "Build Failed"
    Write-Error-Custom $_.Exception.Message
    Write-Host "Total time: $($totalDuration.TotalSeconds.ToString("F1"))s" -ForegroundColor Gray
    exit 1
}