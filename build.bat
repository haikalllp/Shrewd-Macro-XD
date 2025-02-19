@echo off
:: Check for admin privileges
net session >nul 2>&1
if %errorLevel% == 0 (
    goto :admin
) else (
    echo Requesting administrative privileges...
    goto :UACPrompt
)

:UACPrompt
echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
echo UAC.ShellExecute "%~s0", "", "", "runas", 1 >> "%temp%\getadmin.vbs"
"%temp%\getadmin.vbs"
del "%temp%\getadmin.vbs"
exit /B

:admin
cd /d "%~dp0"

echo Starting Mouse Macro build process...
echo.

echo === Cleaning solution ===
dotnet clean
if errorlevel 1 goto error

echo === Removing build directories ===
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"
if errorlevel 1 goto error

echo === Restoring packages ===
dotnet restore
if errorlevel 1 goto error

echo === Building Debug configuration ===
dotnet build -c Debug
if errorlevel 1 goto error

echo === Building Release configuration ===
dotnet build -c Release
if errorlevel 1 goto error

echo.
echo === Build Complete! ===
echo.
echo Debug build: %~dp0bin\Debug\net6.0-windows\NotesTasks.exe
echo Release build: %~dp0bin\Release\net6.0-windows\NotesTasks.exe
echo.
goto end

:error
echo.
echo Build failed! See error message above.
pause
exit /b 1

:end
echo Press any key to exit...
pause >nul
