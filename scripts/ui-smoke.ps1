param([string]$ScreenshotDirectory)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
if (-not $ScreenshotDirectory) { $ScreenshotDirectory = Join-Path $repoRoot 'artifacts\ui-checks' }
$dotnetPath = Join-Path $repoRoot '.tools\dotnet\dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnetPath)) { $dotnetPath = (Get-Command dotnet -ErrorAction Stop).Source }
$project = Join-Path $repoRoot 'tests\AdbFileManager.UiChecks\AdbFileManager.UiChecks.csproj'
& $dotnetPath run --project $project --configuration Release -- (Join-Path $repoRoot 'src\AdbFileManager.WinForms') $ScreenshotDirectory
if ($LASTEXITCODE -ne 0) { throw 'UI smoke checks failed.' }
