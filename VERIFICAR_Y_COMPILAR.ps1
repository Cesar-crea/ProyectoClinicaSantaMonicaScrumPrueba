$ErrorActionPreference = 'Stop'
$solution = Join-Path $PSScriptRoot 'ProyectoSantaMonica_Cesar.sln'

Write-Host '=== Entorno .NET ===' -ForegroundColor Cyan
dotnet --version
dotnet --info

Write-Host '=== Restore ===' -ForegroundColor Cyan
dotnet restore $solution

Write-Host '=== Build Release ===' -ForegroundColor Cyan
dotnet build $solution --configuration Release --no-restore

Write-Host '=== Tests ===' -ForegroundColor Cyan
dotnet test $solution --configuration Release --no-build

Write-Host '=== OK: restore, build y tests finalizados ===' -ForegroundColor Green
