param([string]$Serial)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path $PSScriptRoot -Parent
$adbPath = Join-Path $repoRoot "AdbFileManager\adb.exe"
$dotnetPath = Join-Path $repoRoot ".tools\dotnet\dotnet.exe"
if (-not (Test-Path $dotnetPath)) { $dotnetPath = "dotnet" }

if ([string]::IsNullOrWhiteSpace($Serial)) {
    $devices = @(& $adbPath devices | Select-Object -Skip 1 | Where-Object { $_ -match "\sdevice$" })
    if ($devices.Count -ne 1) {
        throw "Connect and authorize exactly one Android device, or pass -Serial <adb-serial>."
    }
    $Serial = ($devices[0] -split "\s+")[0]
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$evidenceDirectory = Join-Path $repoRoot "artifacts\device-smoke"
$transcriptPath = Join-Path $evidenceDirectory "$timestamp.txt"
New-Item -ItemType Directory -Force -Path $evidenceDirectory | Out-Null

$env:AFM_DEVICE_SERIAL = $Serial
$env:AFM_ADB_PATH = $adbPath
$maskedSerial = if ($Serial.Length -le 4) { "****" } else { "…" + $Serial.Substring($Serial.Length - 4) }

@(
    "AdbFileManager physical device smoke test"
    "UTC: $([DateTime]::UtcNow.ToString('yyyy-MM-dd HH:mm:ss'))"
    "Device: $maskedSerial"
    "Command: dotnet test --filter Category=PhysicalDevice"
    ""
) | Set-Content -Encoding UTF8 $transcriptPath

& $dotnetPath test (Join-Path $repoRoot "tests\AdbFileManager.Tests\AdbFileManager.Tests.csproj") `
    --configuration Release --filter "Category=PhysicalDevice" `
    --logger "console;verbosity=detailed" 2>&1 | Tee-Object -FilePath $transcriptPath -Append
$testExitCode = $LASTEXITCODE

Write-Host ""
Write-Host "Evidence transcript: $transcriptPath"
if ($testExitCode -ne 0) { throw "Physical device smoke test failed with exit code $testExitCode." }
