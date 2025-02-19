# Build script for Mouse Macro
# Author: HaikalLLP
# Description: Cleans and rebuilds both Debug and Release configurations

$ErrorActionPreference = "Stop"
$projectPath = $PSScriptRoot

function Write-ColorOutput($ForegroundColor) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    if ($args) {
        Write-Output $args
    }
    $host.UI.RawUI.ForegroundColor = $fc
}

function Invoke-BuildStep {
    param (
        [string]$stepName,
        [scriptblock]$action
    )
    Write-ColorOutput Green "`n=== $stepName ===`n"
    try {
        & $action
        if ($LASTEXITCODE -ne 0) {
            throw "Command failed with exit code $LASTEXITCODE"
        }
        Write-ColorOutput Cyan "✓ $stepName completed successfully`n"
    }
    catch {
        Write-ColorOutput Red "✗ Error in $stepName : $_`n"
        exit 1
    }
}

# Change to project directory
Set-Location $projectPath

# Display build start
Write-ColorOutput Yellow "`nStarting Mouse Macro build process...`n"

# Clean and rebuild
Invoke-BuildStep "Cleaning solution" {
    dotnet clean
}

Invoke-BuildStep "Removing build directories" {
    if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
    if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
}

Invoke-BuildStep "Restoring packages" {
    dotnet restore
}

Invoke-BuildStep "Building Debug configuration" {
    dotnet build -c Debug
}

Invoke-BuildStep "Building Release configuration" {
    dotnet build -c Release
}

# Final success message
Write-ColorOutput Green "`n=== Build Complete! ===`n"
Write-Output "Debug build: $projectPath\bin\Debug\net6.0-windows\NotesTasks.exe"
Write-Output "Release build: $projectPath\bin\Release\net6.0-windows\NotesTasks.exe"
