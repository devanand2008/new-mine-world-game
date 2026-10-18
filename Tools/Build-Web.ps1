param([Parameter(Mandatory=$true)][string]$UnityPath)
$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $UnityPath -PathType Leaf)) { throw 'Unity.exe path does not exist.' }
$workspace = Split-Path -Parent $PSScriptRoot
$project = Join-Path $workspace 'CubeFrontier'
$logDirectory = Join-Path $workspace 'TestResults'
New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null
$log = Join-Path $logDirectory 'Unity-Web-Build.log'
$arguments = @('-batchmode', '-quit', '-projectPath', ('"' + $project + '"'), '-buildTarget', 'WebGL', '-executeMethod', 'CubeFrontier.Editor.ProjectSetup.BuildWeb', '-logFile', ('"' + $log + '"'))
$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
# Unity may leave its licensing service running. Wait for the Editor itself,
# rather than Start-Process -Wait's entire descendant process tree.
$process.WaitForExit()
if ($process.ExitCode -ne 0) { throw "Unity build failed. Read $log" }
$build = Join-Path $project 'Builds\Web\index.html'
if (-not (Test-Path -LiteralPath $build)) { throw "Unity did not produce a build. Read $log" }
Write-Output "Built: $build"
