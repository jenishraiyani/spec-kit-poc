dotnet tool install --global dotnet-ef --version 8.0.0
$tools = Join-Path $env:USERPROFILE '.dotnet\tools'
if (-not ($env:Path -like "*$tools*")) { $env:Path = $env:Path + ';' + $tools }
$env:ASPNETCORE_ENVIRONMENT='Development'
Write-Host "Using PATH: $env:Path"
Write-Host "Running dotnet ef database update..."
dotnet ef database update --project src/Infrastructure --startup-project src/Api
