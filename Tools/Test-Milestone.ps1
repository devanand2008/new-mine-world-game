param([string]$UnityPath='E:\Unity\6000.3.25f1\Editor\Unity.exe')
$ErrorActionPreference='Stop'
$gameRoot=Split-Path -Parent $PSScriptRoot
$project=Join-Path $gameRoot 'CubeFrontier'
$log=Join-Path $gameRoot 'TestResults\Unity-Milestone.log'
$started=Get-Date
$arguments=@('-batchmode','-projectPath',('"'+$project+'"'),'-buildTarget','Win64','-executeMethod','CubeFrontier.Editor.MilestoneVerification.Run','-logFile',('"'+$log+'"'))
$editorProcess=Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
if(-not $editorProcess.WaitForExit(300000)){Stop-Process -Id $editorProcess.Id;throw "Play Mode verification timed out. Read $log"}
$resultPath=Join-Path $gameRoot 'TestResults\PlayModeMilestone\result.json'
if($editorProcess.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $resultPath)){throw "Play Mode verification failed. Read $log"}
if((Get-Item -LiteralPath $resultPath).LastWriteTime -lt $started){throw 'No fresh verification result was produced.'}
$result=Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
if(-not $result.passed){throw $result.error}
Write-Output "$($result.checks) Play Mode checks passed. Saves are isolated under TestResults."
