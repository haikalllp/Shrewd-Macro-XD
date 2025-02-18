$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$PWD\MouseMacro.lnk")
$Shortcut.TargetPath = "$PWD\bin\Debug\net6.0-windows\MouseMacro.exe"
$Shortcut.WorkingDirectory = "$PWD\bin\Debug\net6.0-windows"
$Shortcut.Description = "Mouse Macro Application"
$Shortcut.Save()

# Create desktop shortcut if user wants it
$DesktopPath = [Environment]::GetFolderPath("Desktop")
$DesktopShortcut = $WshShell.CreateShortcut("$DesktopPath\MouseMacro.lnk")
$DesktopShortcut.TargetPath = "$PWD\bin\Debug\net6.0-windows\MouseMacro.exe"
$DesktopShortcut.WorkingDirectory = "$PWD\bin\Debug\net6.0-windows"
$DesktopShortcut.Description = "Mouse Macro Application"
$DesktopShortcut.Save()

Write-Host "Shortcuts created successfully!"
Write-Host "1. Project folder shortcut: $PWD\MouseMacro.lnk"
Write-Host "2. Desktop shortcut: $DesktopPath\MouseMacro.lnk"
