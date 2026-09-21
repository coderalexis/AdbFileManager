param([ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$localDotnet = Join-Path $projectRoot '.tools\dotnet\dotnet.exe'
$dotnetCommand = if (Test-Path -LiteralPath $localDotnet) { $localDotnet } else { (Get-Command dotnet -ErrorAction Stop).Source }
& $dotnetCommand build (Join-Path $projectRoot 'AdbFileManager\AdbFileManager.csproj') --configuration $Configuration -p:Platform=x64 --nologo
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
$outputPath = Join-Path $projectRoot "AdbFileManager\bin\x64\$Configuration\net8.0-windows10.0.17763.0"
Push-Location -LiteralPath $outputPath
try {
    & $dotnetCommand (Join-Path $outputPath 'AdbFileManager.dll')
}
finally {
    Pop-Location
}
