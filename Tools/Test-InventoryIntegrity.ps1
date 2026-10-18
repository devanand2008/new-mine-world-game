$ErrorActionPreference = 'Stop'
$workspace = Split-Path -Parent $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$outputDirectory = Join-Path $workspace 'TestResults'
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
$coreDirectory = Join-Path $workspace 'CubeFrontier\Assets\CubeFrontier\Core'
$sources = @((Join-Path $coreDirectory 'Blocks.cs'), (Join-Path $coreDirectory 'Inventory.cs'))
$executable = Join-Path $outputDirectory 'InventoryIntegrityTests.exe'
& $compiler /nologo /target:exe /optimize+ "/out:$executable" $sources (Join-Path $workspace 'Tests\InventoryIntegrityTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Inventory integrity C# compilation failed' }
& $executable
if ($LASTEXITCODE -ne 0) { throw 'Inventory integrity tests failed' }
