param([string]$UnityPath='E:\Unity\6000.3.25f1\Editor\Unity.exe')
$ErrorActionPreference='Stop'
$gameRoot=Split-Path -Parent $PSScriptRoot
$project=Join-Path $gameRoot 'CubeFrontier'
$log=Join-Path $gameRoot 'TestResults\Unity-Windows-Build.log'
$arguments=@('-batchmode','-quit','-projectPath',('"'+$project+'"'),'-buildTarget','Win64','-executeMethod','CubeFrontier.Editor.ProjectSetup.BuildWindows','-logFile',('"'+$log+'"'))
$editorProcess=Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
$editorProcess.WaitForExit()
if($editorProcess.ExitCode -ne 0){throw "Windows build failed. Read $log"}
Write-Output (Join-Path $project 'Builds\Windows\CubeFrontier.exe')
