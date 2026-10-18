# Continues the installation already downloaded from Unity's official release URLs.
# Safe to rerun: completed installations are detected, downloads are retained on E:.
param([string]$InstallerDirectory = 'E:\Unity\Installers', [string]$EditorRoot = 'E:\Unity\6000.3.25f1')
$ErrorActionPreference = 'Stop'
$workspace = Split-Path -Parent $PSScriptRoot
$statusFile = Join-Path $workspace 'TestResults\Local-Setup-Status.txt'
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $statusFile) | Out-Null
function Status([string]$Message) {
    $line = '[{0}] {1}' -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $Message
    $line | Set-Content -LiteralPath $statusFile
    Write-Output $line
}
function Install-Verified([string]$Installer) {
    $signature = Get-AuthenticodeSignature -LiteralPath $Installer
    if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch 'Unity Technologies') {
        throw "Unity installer publisher verification failed: $Installer ($($signature.Status))"
    }
    Status "Installing $(Split-Path -Leaf $Installer). Approve Windows elevation if requested."
    $process = Start-Process -FilePath $Installer -ArgumentList @('/S', ('/D=' + $EditorRoot)) -Verb RunAs -WindowStyle Hidden -Wait -PassThru
    if ($process.ExitCode -ne 0) { throw "Installer returned exit code $($process.ExitCode): $Installer" }
}
try {
    $editor = Join-Path $EditorRoot 'Editor\Unity.exe'
    $editorInstaller = Join-Path $InstallerDirectory 'UnitySetup64-6000.3.25f1.exe'
    $webInstaller = Join-Path $InstallerDirectory 'UnitySetup-WebGL-Support-for-Editor-6000.3.25f1.exe'
    $ready = Join-Path $InstallerDirectory 'downloads-ready.json'
    if (-not (Test-Path -LiteralPath $editor)) {
        Status 'Waiting for the resumable Unity installer downloads to complete.'
        while (-not (Test-Path -LiteralPath $ready)) {
            $editorDownload = Get-Item -LiteralPath $editorInstaller -ErrorAction SilentlyContinue
            $webDownload = Get-Item -LiteralPath $webInstaller -ErrorAction SilentlyContinue
            if ($editorDownload.Length -eq 4201959888 -and $webDownload.Length -eq 929948960) { break }
            if (Test-Path -LiteralPath (Join-Path $InstallerDirectory 'download-error.txt')) { throw (Get-Content -LiteralPath (Join-Path $InstallerDirectory 'download-error.txt') -Raw) }
            Start-Sleep -Seconds 10
        }
        if ((Get-FileHash -LiteralPath $editorInstaller -Algorithm MD5).Hash -ne 'F9ECE8BE816F34BD301DE62DFF1D4111') { throw 'Unity Editor installer checksum mismatch.' }
        Install-Verified $editorInstaller
    }
    if (-not (Test-Path -LiteralPath $editor)) { throw 'Editor installation did not produce Unity.exe.' }
    if (-not (Test-Path -LiteralPath (Join-Path $EditorRoot 'Editor\Data\PlaybackEngines\WebGLSupport'))) { Install-Verified $webInstaller }
    [Environment]::SetEnvironmentVariable('UNITY_EDITOR_PATH', $editor, 'User')
    $env:UNITY_EDITOR_PATH = $editor
    $cli = 'E:\Unity\CLI\bin\unity.exe'
    if (Test-Path -LiteralPath $cli) { & $cli editors add $editor --non-interactive }
    Status 'Unity and Web support installed. Building Cube Frontier; details in Unity-Web-Build.log.'
    & (Join-Path $PSScriptRoot 'Build-Web.ps1') -UnityPath $editor
    Status 'Build completed. Starting the local game server and opening your browser.'
    & (Join-Path $PSScriptRoot 'Start-Game.ps1')
} catch {
    Status ('Stopped: ' + $_.Exception.Message)
    Write-Error $_.Exception.Message
    exit 1
}
