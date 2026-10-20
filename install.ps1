# Registers (or unregisters) the legend bar to start with Windows.
# Usage:
#   powershell -ExecutionPolicy Bypass -File install.ps1
#   powershell -ExecutionPolicy Bypass -File install.ps1 -Uninstall
param([switch]$Uninstall)

$root = Split-Path -Parent $PSCommandPath
$startup = [Environment]::GetFolderPath('Startup')
$lnkPath = Join-Path $startup 'ZoteroLegend.lnk'

if ($Uninstall) {
    if (Test-Path $lnkPath) {
        Remove-Item $lnkPath -Force
        Write-Host ('removed: ' + $lnkPath)
    } else {
        Write-Host 'nothing to remove'
    }
    exit 0
}

$exe = Join-Path $root 'dist\ZoteroLegend.exe'
if (-not (Test-Path $exe)) {
    throw ('not found: ' + $exe + '  (run build.ps1 first)')
}

$shell = New-Object -ComObject WScript.Shell
$lnk = $shell.CreateShortcut($lnkPath)
$lnk.TargetPath = $exe
$lnk.WorkingDirectory = Split-Path $exe
$lnk.Description = 'Zotero highlight legend bar'
$lnk.WindowStyle = 1
$lnk.Save()

Write-Host ('created: ' + $lnkPath)
Write-Host 'the bar starts with the next Windows sign-in; run it now:'
Write-Host ('  ' + $exe)
