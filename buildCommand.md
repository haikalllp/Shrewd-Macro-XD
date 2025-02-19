# Batch (CMD)
dotnet clean && rmdir /s /q bin obj && dotnet restore && dotnet build -c Debug && dotnet build -c Release

# PowerShell
dotnet clean; Remove-Item -Recurse -Force bin,obj; dotnet restore; dotnet build -c Debug; dotnet build -c Release