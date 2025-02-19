@echo off
setlocal EnableDelayedExpansion

:: Build script for Mouse Macro
:: Author: HaikalLLP
:: Description: Cleans and rebuilds both Debug and Release configurations

:: Set console colors
set "GREEN=0A"
set "CYAN=0B"
set "RED=0C"
set "YELLOW=0E"
set "WHITE=0F"

:: Store the project path
set "PROJECT_PATH=%~dp0"
cd /d "%PROJECT_PATH%"

:: Function to print colored text
:print_colored
set "COLOR=%~1"
set "MESSAGE=%~2"
powershell -Command Write-Host '%MESSAGE%' -ForegroundColor %COLOR%
exit /b

:: Function to print step header
:print_step
echo.
call :print_colored Green "=== %~1 ==="
echo.
exit /b

:: Display start message
echo.
call :print_colored Yellow "Starting Mouse Macro build process..."
echo.

:: Clean Solution
call :print_step "Cleaning solution"
dotnet clean
if errorlevel 1 (
    call :print_colored Red "Error: Failed to clean solution"
    exit /b 1
)
call :print_colored Cyan "✓ Clean completed successfully"

:: Remove build directories
call :print_step "Removing build directories"
if exist "bin" (
    rmdir /s /q "bin"
    if errorlevel 1 (
        call :print_colored Red "Error: Failed to remove bin directory"
        exit /b 1
    )
)
if exist "obj" (
    rmdir /s /q "obj"
    if errorlevel 1 (
        call :print_colored Red "Error: Failed to remove obj directory"
        exit /b 1
    )
)
call :print_colored Cyan "✓ Directories removed successfully"

:: Restore packages
call :print_step "Restoring packages"
dotnet restore
if errorlevel 1 (
    call :print_colored Red "Error: Failed to restore packages"
    exit /b 1
)
call :print_colored Cyan "✓ Restore completed successfully"

:: Build Debug
call :print_step "Building Debug configuration"
dotnet build -c Debug
if errorlevel 1 (
    call :print_colored Red "Error: Failed to build Debug configuration"
    exit /b 1
)
call :print_colored Cyan "✓ Debug build completed successfully"

:: Build Release
call :print_step "Building Release configuration"
dotnet build -c Release
if errorlevel 1 (
    call :print_colored Red "Error: Failed to build Release configuration"
    exit /b 1
)
call :print_colored Cyan "✓ Release build completed successfully"

:: Final success message
echo.
call :print_colored Green "=== Build Complete! ==="
echo.
echo Debug build: %PROJECT_PATH%bin\Debug\net6.0-windows\NotesTasks.exe
echo Release build: %PROJECT_PATH%bin\Release\net6.0-windows\NotesTasks.exe
echo.

exit /b 0
