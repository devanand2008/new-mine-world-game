@echo off
setlocal
title Cube Frontier - Local Game
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0Tools\Start-Game.ps1" %*
set "gameExitCode=%errorlevel%"
if not "%gameExitCode%"=="0" (
  echo.
  echo The game did not start. See the message above.
  if not "%FRONTIER_NO_PAUSE%"=="1" pause
)
exit /b %gameExitCode%
