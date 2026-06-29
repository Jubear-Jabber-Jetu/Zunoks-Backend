# Builds a Release publish folder at ./publish for IIS or server copy-deploy.
# Usage: .\publish.ps1

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

Write-Host "Publishing Zunoks-Backend (Release)..." -ForegroundColor Cyan

dotnet publish ZunoksBackend.csproj `
  -c Release `
  -o ./publish `
  --self-contained false `
  -r win-x64

if ($LASTEXITCODE -ne 0) {
  throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Write-Host ""
Write-Host "Publish complete: $PSScriptRoot\publish" -ForegroundColor Green
Write-Host "Copy the publish folder to your server, then set ASPNETCORE_ENVIRONMENT=Production." -ForegroundColor Yellow
Write-Host "Ensure appsettings.Production.json on the server includes ConnectionStrings, Jwt, and ReComAdmin." -ForegroundColor Yellow
