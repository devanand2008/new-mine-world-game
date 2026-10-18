$ErrorActionPreference = 'Stop'
$workspace = Split-Path -Parent $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$outputDirectory = Join-Path $workspace 'TestResults'
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
$sources = @(Get-ChildItem -LiteralPath (Join-Path $workspace 'CubeFrontier\Assets\CubeFrontier\Core') -Filter '*.cs' | ForEach-Object FullName)
$executable = Join-Path $outputDirectory 'CoreTests.exe'
& $compiler /nologo /target:exe /optimize+ "/out:$executable" $sources (Join-Path $workspace 'Tests\CoreTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Core C# compilation failed' }
& $executable
if ($LASTEXITCODE -ne 0) { throw 'Core tests failed' }
