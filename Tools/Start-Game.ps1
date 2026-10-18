param(
    [Parameter(Position=0)][string]$UnityPath,
    [switch]$Rebuild,
    [switch]$NoBrowser,
    [switch]$CheckOnly
)
$ErrorActionPreference = 'Stop'
$workspace = Split-Path -Parent $PSScriptRoot
$project = Join-Path $workspace 'CubeFrontier'
$build = Join-Path $project 'Builds\Web'
$versionLine = Get-Content -LiteralPath (Join-Path $project 'ProjectSettings\ProjectVersion.txt') | Where-Object { $_ -match '^m_EditorVersion: ' } | Select-Object -First 1
$editorVersion = ($versionLine -replace '^m_EditorVersion: ', '').Trim()

function Test-UnityEditor([string]$Path) {
    if (-not $Path -or -not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $false }
    # The Unity CLI also uses unity.exe. Only the Editor has this adjacent Data folder.
    return (Split-Path -Leaf $Path) -ieq 'Unity.exe' -and (Test-Path -LiteralPath (Join-Path (Split-Path -Parent $Path) 'Data\Managed') -PathType Container)
}

function Find-UnityEditor {
    if ($UnityPath) {
        if (-not (Test-UnityEditor $UnityPath)) {
            throw "The supplied path is not a Unity Editor installation: $UnityPath. Select Editor\Unity.exe, not the Unity CLI."
        }
        return (Resolve-Path -LiteralPath $UnityPath).Path
    }
    if ($env:UNITY_EDITOR_PATH) {
        if (Test-UnityEditor $env:UNITY_EDITOR_PATH) { return (Resolve-Path -LiteralPath $env:UNITY_EDITOR_PATH).Path }
    }
    $command = Get-Command Unity.exe -ErrorAction SilentlyContinue
    if ($command -and (Test-UnityEditor $command.Source)) { return $command.Source }
    $candidates = [System.Collections.Generic.List[string]]::new()
    if ($env:ProgramFiles) { $candidates.Add((Join-Path $env:ProgramFiles "Unity\Hub\Editor\$editorVersion\Editor\Unity.exe")) }
    foreach ($drive in Get-PSDrive -PSProvider FileSystem) {
        foreach ($suffix in @("Unity\Hub\Editor\$editorVersion\Editor\Unity.exe", "Unity\$editorVersion\Editor\Unity.exe", "Unity\Editor\Unity.exe", "Unity\Editor\$editorVersion\Editor\Unity.exe", "Program Files\Unity\Hub\Editor\$editorVersion\Editor\Unity.exe")) {
            $candidates.Add((Join-Path $drive.Root $suffix))
        }
    }
    # Unity Hub can record custom installation locations; read path strings only.
    $hubFile = Join-Path $env:APPDATA 'UnityHub\editors.json'
    if (Test-Path -LiteralPath $hubFile) {
        try {
            function Add-HubPaths($value) {
                if ($value -is [string]) {
                    if ([System.IO.Path]::IsPathRooted($value)) {
                        if ($value.EndsWith('Unity.exe', [System.StringComparison]::OrdinalIgnoreCase)) { $candidates.Add($value) }
                        else { $candidates.Add((Join-Path $value 'Editor\Unity.exe')); $candidates.Add((Join-Path $value 'Unity.exe')) }
                    }
                } elseif ($value -is [System.Collections.IEnumerable] -and $value -isnot [pscustomobject]) {
                    foreach ($entry in $value) { Add-HubPaths $entry }
                } elseif ($null -ne $value -and $value -is [pscustomobject]) {
                    foreach ($property in $value.PSObject.Properties) { Add-HubPaths $property.Value }
                }
            }
            Add-HubPaths (Get-Content -LiteralPath $hubFile -Raw | ConvertFrom-Json)
        } catch { Write-Warning 'Could not read Unity Hub editor locations; checking standard paths instead.' }
    }
    foreach ($candidate in $candidates) { if (Test-UnityEditor $candidate) { return $candidate } }
    return $null
}

try {
    Write-Host 'Cube Frontier: Eclipse - local prototype' -ForegroundColor Cyan
    Write-Host "Project: $project"
    $node = Get-Command node.exe -ErrorAction SilentlyContinue
    if (-not $node) { throw 'Node.js is required to serve the local Web build. Install Node.js, reopen this launcher, and try again.' }
    $serverScript = Join-Path $PSScriptRoot 'Serve-Web.mjs'
    # Check mode writes its diagnostics to stdout for PowerShell 5.1 compatibility.
    $buildProblem = & $node.Source $serverScript --check
    $needsBuild = $LASTEXITCODE -ne 0
    if ($needsBuild -or $Rebuild) {
        $editor = Find-UnityEditor
        if (-not $editor) {
            $downloadTask = Get-CimInstance Win32_Process -Filter "Name = 'node.exe'" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -like '*download-unity.mjs*' } | Select-Object -First 1
            if ($downloadTask) {
                Write-Host ''
                Write-Host 'Unity installation files are still downloading on this laptop.' -ForegroundColor Yellow
                Write-Host 'The setup runner will install Unity and attempt the game build when the download finishes.'
                Write-Host "Setup status: $(Join-Path $workspace 'TestResults\Local-Setup-Status.txt')"
                throw 'Local play is not ready yet. Keep the laptop online while setup continues.'
            }
            Write-Host ''
            Write-Host "A Unity Web build is not ready at $build" -ForegroundColor Yellow
            Write-Host "Install Unity $editorVersion with Web Build Support and activate its license in Unity Hub."
            Write-Host 'Then double-click Start-Game.bat again.'
            Write-Host 'For a custom Editor location, run:'
            Write-Host '  Start-Game.bat "E:\your-installation\Editor\Unity.exe"'
            Write-Host 'You can also set UNITY_EDITOR_PATH to the full Unity.exe path.'
            throw 'Unity Editor was not found. The game cannot be compiled or played from C# source alone.'
        }
        Write-Host "Editor: $editor"
        if ($CheckOnly) { Write-Host 'Build required. Editor found; compilation and license have not been verified.'; exit 0 }
        if (Test-Path -LiteralPath (Join-Path $project 'Temp\UnityLockfile')) {
            throw 'Close this project in the Unity Editor before the launcher builds it, or use Cube Frontier > Build Web inside the Editor.'
        }
        Write-Host 'Building the game. First import/build can take several minutes.' -ForegroundColor Cyan
        & (Join-Path $PSScriptRoot 'Build-Web.ps1') -UnityPath $editor
        & $node.Source $serverScript --check
        if ($LASTEXITCODE -ne 0) { throw 'Unity did not produce a complete Web build. Inspect TestResults\Unity-Web-Build.log.' }
    }
    if ($CheckOnly) { Write-Host 'Local Web build and Node.js are present. Ready to launch.'; exit 0 }
    $port = 8080
    if ($env:PORT -and (-not [int]::TryParse($env:PORT, [ref]$port) -or $port -lt 1 -or $port -gt 65535)) { throw 'PORT must be a number between 1 and 65535.' }
    $url = "http://127.0.0.1:$port"
    # Reuse only this project's server; never stop an unrelated service on this port.
    $existing = $null
    try { $existing = Invoke-RestMethod -Uri "$url/__frontier_health" -TimeoutSec 2 } catch { }
    if ($existing -and $existing.game -eq 'CubeFrontierEclipse' -and $existing.root -eq $build) {
        Write-Host "The local game server is already running: $url"
        if (-not $NoBrowser) { Start-Process -FilePath $url }
        exit 0
    }
    Write-Host 'Keep this window open while playing. Press Ctrl+C to stop the local server.'
    Write-Host 'Use the same address/port each time so the browser can find your local saves.'
    $serverArguments = @($serverScript)
    if (-not $NoBrowser) { $serverArguments += '--open-browser' }
    & $node.Source @serverArguments
    if ($LASTEXITCODE -ne 0) { throw 'The local game server stopped with an error. See its message above.' }
} catch {
    Write-Host ''
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
