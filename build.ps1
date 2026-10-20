#requires -Version 5.1
# Build ZoteroLegend.exe (.NET Framework 4.8 + WPF, single exe, no Visual Studio needed)
# Keep this file ASCII-only: Windows PowerShell 5.1 reads BOM-less files as ANSI.
$ErrorActionPreference = 'Stop'

if ($PSVersionTable.PSVersion.Major -ge 6) {
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath
    exit $LASTEXITCODE
}

$root   = Split-Path -Parent $PSCommandPath
$src    = Join-Path $root 'src\Program.cs'
$man    = Join-Path $root 'src\app.manifest'
$cfgIn  = Join-Path $root 'src\ZoteroLegend.exe.config'
$jsonIn = Join-Path $root 'colors.json'
$outDir = Join-Path $root 'dist'
$outExe = Join-Path $outDir 'ZoteroLegend.exe'

$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) { throw "csc.exe not found: $csc" }

function Get-AsmPath([string]$name) {
    $a = [System.Reflection.Assembly]::LoadWithPartialName($name)
    if (-not $a) { throw "assembly not found: $name" }
    return $a.Location
}

# csc.rsp already auto-references System.Windows.Forms / System.Drawing /
# System.Web.Extensions / System.Core from the framework dir; adding them again
# triggers CS1703. Only the WPF assemblies need explicit /r.
$refNames = @(
    'PresentationFramework', 'PresentationCore', 'WindowsBase', 'System.Xaml'
)
$refArgs = @()
foreach ($n in $refNames) { $refArgs += ('/r:' + (Get-AsmPath $n)) }

New-Item -ItemType Directory -Force -Path $outDir | Out-Null

# the running instance locks the exe; stop it before writing
Get-Process ZoteroLegend -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 250

$compileArgs = @(
    '/nologo', '/target:winexe', '/platform:x64', '/optimize+', '/codepage:65001',
    ('/win32manifest:' + $man),
    ('/out:' + $outExe)
) + $refArgs + @($src)

& $csc $compileArgs
if ($LASTEXITCODE -ne 0) { throw "compile failed, exit code $LASTEXITCODE" }

Copy-Item $cfgIn (Join-Path $outDir 'ZoteroLegend.exe.config') -Force

$jsonOut = Join-Path $outDir 'colors.json'
if (-not (Test-Path $jsonOut)) {
    Copy-Item $jsonIn $jsonOut
    Write-Host 'wrote dist\colors.json (first build)'
} else {
    Write-Host 'kept dist\colors.json (user config is not overwritten)'
}

$size = (Get-Item $outExe).Length / 1KB
Write-Host ('OK: ' + $outExe + '  (' + [math]::Round($size, 1) + ' KB)')
