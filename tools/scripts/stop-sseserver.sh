#!/usr/bin/env bash
set -uo pipefail

# Stop any processes running the SSE server (tools/sse-server or Test.SseServer):
#   1. Kill whatever is bound to port 5001
#   2. Kill by executable name / command line, including orphaned "dotnet.exe" hosts left
#      behind when a VS Code task is force-terminated (killing the outer `dotnet run` shell
#      does not always kill its apphost child on Windows)

stopped=0

if command -v powershell.exe >/dev/null 2>&1; then
  netstat -ano 2>/dev/null | grep -E ':5001\s' | awk '{print $NF}' | sort -u \
    | xargs -r -I {} taskkill //PID {} //F 2>/dev/null && stopped=1

  powershell.exe -NoProfile -Command '
    $procs = @(Get-CimInstance Win32_Process | Where-Object {
      ($_.Name -eq "Test.SseServer.exe" -or $_.Name -eq "dotnet.exe") -and
      ($_.CommandLine -match "Test\.SseServer" -or $_.CommandLine -match "tools[\\/]sse-server")
    })
    if ($procs) {
      $procs | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
      Start-Sleep -Milliseconds 500
      exit 0
    }
    exit 1
  ' && stopped=1
else
  lsof -ti:5001 2>/dev/null | xargs -r kill -9 2>/dev/null && stopped=1
  pkill -9 -f 'dotnet.*(Test\.SseServer|tools/sse-server)' 2>/dev/null && stopped=1
fi

if [ "$stopped" -eq 1 ]; then
  echo '[OK] SSE Server hosts successfully stopped'
else
  echo '[INFO] No SSE Server server processes found'
fi
