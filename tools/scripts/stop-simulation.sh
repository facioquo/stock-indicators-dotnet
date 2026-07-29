#!/usr/bin/env bash
set -uo pipefail

# Stop any processes running the simulation tool (tools/simulate or Test.Simulation),
# including orphaned "dotnet.exe" hosts left behind when a VS Code task is force-terminated
# (killing the outer `dotnet run` shell does not always kill its apphost child on Windows).

if command -v powershell.exe >/dev/null 2>&1; then
  powershell.exe -NoProfile -Command '
    $procs = @(Get-CimInstance Win32_Process | Where-Object {
      ($_.Name -eq "Test.Simulation.exe" -or $_.Name -eq "dotnet.exe") -and
      ($_.CommandLine -match "Test\.Simulation" -or $_.CommandLine -match "tools[\\/]simulate")
    })
    if ($procs) {
      $procs | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
      Start-Sleep -Milliseconds 500
      exit 0
    }
    exit 1
  ' && echo '[OK] Simulation hosts successfully stopped' || echo '[INFO] No Simulation server processes found'
else
  pkill -9 -f 'dotnet.*(Test\.Simulation|tools/simulate)' 2>/dev/null \
    && echo '[OK] Simulation hosts successfully stopped' \
    || echo '[INFO] No Simulation server processes found'
fi
