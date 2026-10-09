param([string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $projectRoot 'artifacts' }
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
$localDotnet = Join-Path $projectRoot '.tools\dotnet\dotnet.exe'
$dotnetCommand = if (Test-Path -LiteralPath $localDotnet) { $localDotnet } else { (Get-Command dotnet -ErrorAction Stop).Source }
$packageName = 'AdbFileManager-win-x64'
# Each publish uses a new directory so stale DLLs cannot enter the package.
$publishDirectory = Join-Path $OutputDirectory ($packageName + '-' + [Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $publishDirectory -Force | Out-Null
& $dotnetCommand publish (Join-Path $projectRoot 'src\AdbFileManager.WinForms\AdbFileManager.WinForms.csproj') `
    --configuration Release --runtime win-x64 --self-contained true -p:Platform=x64 `
    -p:PublishSingleFile=false -p:PublishTrimmed=false --output $publishDirectory --nologo
if ($LASTEXITCODE -ne 0) { throw 'Portable publish failed.' }
foreach ($requiredFile in 'AdbFileManager.exe', 'AdbFileManager.Core.dll', 'AdbFileManager.Infrastructure.dll', 'adb.exe', 'AdbWinApi.dll', 'AdbWinUsbApi.dll', 'coreclr.dll', 'hostpolicy.dll') {
    if (-not (Test-Path -LiteralPath (Join-Path $publishDirectory $requiredFile))) { throw "Missing portable dependency: $requiredFile" }
}
foreach ($iconDirectory in 'icons', 'iconsW11') {
    if (-not (Test-Path -LiteralPath (Join-Path $publishDirectory $iconDirectory))) { throw "Missing icons: $iconDirectory" }
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Iniciar-portable.cmd') -Destination (Join-Path $publishDirectory 'Iniciar.cmd')
Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination $publishDirectory
$archive = Join-Path $OutputDirectory ($packageName + '.zip')
Compress-Archive -Path (Join-Path $publishDirectory '*') -DestinationPath $archive -Force
$hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText($archive + '.sha256', $hash + '  ' + [IO.Path]::GetFileName($archive) + [Environment]::NewLine)
Write-Output "Portable directory: $publishDirectory"
Write-Output "ZIP: $archive"
Write-Output "SHA-256: $hash"
