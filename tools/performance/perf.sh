#!/usr/bin/env bash
#
# One entry point for indicator performance benchmarking.
#
#   ./perf.sh reset               Run the full baseline suite and replace the
#                                 committed baseline files in baselines/.
#   ./perf.sh reset --prune       ...also delete baseline files no longer in the set.
#   ./perf.sh evaluate            Run the full baseline suite and report
#                                 regressions/improvements vs baselines (exit 1 on regression).
#   ./perf.sh spot <indicator> [style]
#                                 Run one indicator (style = Series|Buffer|Stream|All,
#                                 default All) and compare to baselines. Fast dev-loop check.
#
# The BASELINE SET matches the no-argument `dotnet run -c Release` default in
# Program.cs: SeriesIndicators, BufferIndicators, StreamIndicators, Utility,
# UtilityNullMath, UtilityStdDev. StyleComparison, StreamExternal, and
# ManualTestDirect are diagnostics and are NOT baselined.
#
# reset/evaluate run the full suite (~1 hour). spot is quick.
# evaluate/spot require jq (used by detect-regressions.sh).
#
# Examples:
#   ./perf.sh reset
#   ./perf.sh evaluate
#   ./perf.sh spot Ema
#   ./perf.sh spot Adx Stream

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RESULTS_DIR="$SCRIPT_DIR/BenchmarkDotNet.Artifacts/results"
BASELINE_DIR="$SCRIPT_DIR/baselines"

# Baseline set. Keep in sync with the no-arg run list in Program.cs.
BASELINE_CLASSES=(
  Performance.SeriesIndicators
  Performance.BufferIndicators
  Performance.StreamIndicators
  Performance.Utility
  Performance.UtilityNullMath
  Performance.UtilityStdDev
)

usage() {
  cat <<'EOF'
Usage:
  ./perf.sh reset [--prune]
  ./perf.sh evaluate
  ./perf.sh spot <indicator> [Series|Buffer|Stream|All]

Baseline set: SeriesIndicators, BufferIndicators, StreamIndicators, Utility, UtilityNullMath, UtilityStdDev
EOF
}

clear_results() {
  if [[ -d "$RESULTS_DIR" ]]; then
    rm -f "$RESULTS_DIR"/*-report-*.json "$RESULTS_DIR"/*-report-*.md 2>/dev/null || true
  fi
}

run_full() {
  echo "Running full baseline suite: dotnet run -c Release"
  ( cd "$SCRIPT_DIR" && dotnet run -c Release )
}

run_filtered() {
  echo "Running: dotnet run -c Release -- --filter $*"
  ( cd "$SCRIPT_DIR" && dotnet run -c Release -- --filter "$@" )
}

cmd_reset() {
  local prune=0
  [[ "${1:-}" == "--prune" ]] && prune=1

  clear_results
  run_full

  echo
  echo "Copying baseline files ..."
  local copied=0
  for class in "${BASELINE_CLASSES[@]}"; do
    for ext in report-full.json report-github.md; do
      src="$RESULTS_DIR/$class-$ext"
      if [[ -f "$src" ]]; then
        cp -f "$src" "$BASELINE_DIR/$class-$ext"
        copied=$((copied + 1))
      else
        echo "  WARNING: expected result missing: $class-$ext"
      fi
    done
  done
  echo "Copied $copied baseline file(s) to $BASELINE_DIR"

  # Detect committed baseline files no longer in the set
  local known=()
  for class in "${BASELINE_CLASSES[@]}"; do
    known+=("$class-report-full.json" "$class-report-github.md")
  done
  shopt -s nullglob
  local stale=()
  for f in "$BASELINE_DIR"/Performance.*-report-*; do
    local name; name="$(basename "$f")"
    local is_known=0
    for k in "${known[@]}"; do [[ "$k" == "$name" ]] && is_known=1 && break; done
    [[ "$is_known" -eq 0 ]] && stale+=("$f")
  done
  shopt -u nullglob

  if [[ "${#stale[@]}" -gt 0 ]]; then
    echo
    if [[ "$prune" -eq 1 ]]; then
      for f in "${stale[@]}"; do rm -f "$f"; echo "  Pruned: $(basename "$f")"; done
    else
      echo "Stale baseline files NOT in the baseline set (re-run with --prune to remove):"
      for f in "${stale[@]}"; do echo "  $(basename "$f")"; done
    fi
  fi

  echo
  echo "Baseline reset complete. Review 'git diff' before committing."
}

cmd_evaluate() {
  clear_results
  run_full
  echo
  exec bash "$SCRIPT_DIR/detect-regressions.sh"
}

cmd_spot() {
  local indicator="${1:-}"
  local style="${2:-All}"
  if [[ -z "$indicator" ]]; then
    echo "Error: spot requires an indicator name (e.g. './perf.sh spot Ema')." >&2
    usage
    exit 2
  fi

  local classes=()
  case "$style" in
    Series) classes=(Performance.SeriesIndicators) ;;
    Buffer) classes=(Performance.BufferIndicators) ;;
    Stream) classes=(Performance.StreamIndicators) ;;
    All)    classes=(Performance.SeriesIndicators Performance.BufferIndicators Performance.StreamIndicators) ;;
    *) echo "Error: invalid style '$style' (Series|Buffer|Stream|All)." >&2; exit 2 ;;
  esac

  local filters=()
  for c in "${classes[@]}"; do filters+=("$c.*$indicator*"); done

  clear_results
  run_filtered "${filters[@]}"
  echo
  exec bash "$SCRIPT_DIR/detect-regressions.sh"
}

case "${1:-}" in
  reset)    shift; cmd_reset "$@" ;;
  evaluate) shift; cmd_evaluate "$@" ;;
  spot)     shift; cmd_spot "$@" ;;
  -h|--help|"") usage; exit 2 ;;
  *) echo "Unknown command: $1" >&2; usage; exit 2 ;;
esac
