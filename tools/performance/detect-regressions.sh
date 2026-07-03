#!/usr/bin/env bash
#
# Performance regression detection for Stock Indicators benchmarks.
#
# Compares current BenchmarkDotNet results against committed baselines and
# reports regressions and improvements per benchmark method.
#
# Directory mode (default): pairs every "*-report-full.json" in the results
# directory with the same-named file in the baselines directory and compares
# them suite-by-suite. Only suites present in --current-dir are compared, so a
# filtered/spot run naturally compares just what it ran.
#
# Explicit-pair mode: compares a single --baseline-file against a --current-file.
#
# Paths default relative to this script (tools/performance), so it works from any
# working directory.
#
# Exit codes: 0 no regressions, 1 regressions found, 2 usage/IO error.
#
# Requires: jq
#
# Examples:
#   ./detect-regressions.sh
#   ./detect-regressions.sh --threshold 15
#   ./detect-regressions.sh \
#     --baseline-file baselines/Performance.StreamIndicators-report-full.json \
#     --current-file  BenchmarkDotNet.Artifacts/results/Performance.StreamIndicators-report-full.json

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

BASELINE_FILE=""
CURRENT_FILE=""
BASELINE_DIR="$SCRIPT_DIR/baselines"
CURRENT_DIR="$SCRIPT_DIR/BenchmarkDotNet.Artifacts/results"
THRESHOLD=10

usage() {
  sed -n '2,26p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --baseline-file) BASELINE_FILE="$2"; shift 2 ;;
    --current-file)  CURRENT_FILE="$2";  shift 2 ;;
    --baseline-dir)  BASELINE_DIR="$2";  shift 2 ;;
    --current-dir)   CURRENT_DIR="$2";   shift 2 ;;
    --threshold)     THRESHOLD="$2";     shift 2 ;;
    -h|--help)       usage; exit 0 ;;
    *) echo "Unknown argument: $1" >&2; usage; exit 2 ;;
  esac
done

if ! command -v jq >/dev/null 2>&1; then
  echo "Error: jq is required but not found on PATH." >&2
  echo "Install it (e.g. 'winget install jqlang.jq', 'apt-get install jq', or 'brew install jq')." >&2
  exit 2
fi

echo "Performance Regression Detection"
echo "================================="
echo "Threshold: ${THRESHOLD}%"
echo

# Emit one compact JSON object per significant change: {name, base, cur, pct}
compare_pair() {
  local base="$1" cur="$2"
  jq -c -n \
    --slurpfile b "$base" \
    --slurpfile c "$cur" \
    --argjson t "$THRESHOLD" '
      ($b[0].Benchmarks | map({key:(.Type+"."+.Method), value:.Statistics.Mean}) | from_entries) as $m
      | $c[0].Benchmarks[]
      | (.Type+"."+.Method) as $k
      | select($m[$k] != null and $m[$k] > 0)
      | (((.Statistics.Mean - $m[$k]) / $m[$k]) * 100) as $pct
      | select(($pct | fabs) > $t)
      | {name:$k, base:$m[$k], cur:.Statistics.Mean, pct:$pct}
    '
}

# Collect (baseline, current) pairs
pairs=()  # each entry: "baseline<TAB>current"
if [[ -n "$BASELINE_FILE" || -n "$CURRENT_FILE" ]]; then
  if [[ -z "$BASELINE_FILE" || -z "$CURRENT_FILE" ]]; then
    echo "Error: --baseline-file and --current-file must be used together." >&2
    exit 2
  fi
  [[ -f "$BASELINE_FILE" ]] || { echo "Error: baseline not found: $BASELINE_FILE" >&2; exit 2; }
  [[ -f "$CURRENT_FILE"  ]] || { echo "Error: current not found: $CURRENT_FILE" >&2; exit 2; }
  pairs+=("$BASELINE_FILE"$'\t'"$CURRENT_FILE")
else
  [[ -d "$CURRENT_DIR" ]] || { echo "Error: results directory not found: $CURRENT_DIR" >&2; echo "Run benchmarks first (e.g. 'perf.sh evaluate')." >&2; exit 2; }
  shopt -s nullglob
  found=0
  for cf in "$CURRENT_DIR"/*-report-full.json; do
    found=1
    name="$(basename "$cf")"
    bf="$BASELINE_DIR/$name"
    if [[ -f "$bf" ]]; then
      pairs+=("$bf"$'\t'"$cf")
    else
      echo "Skipping (no baseline): $name"
    fi
  done
  shopt -u nullglob
  [[ "$found" -eq 1 ]] || { echo "Error: no '*-report-full.json' files in $CURRENT_DIR" >&2; exit 2; }
  [[ "${#pairs[@]}" -gt 0 ]] || { echo "Error: no result files matched a baseline in $BASELINE_DIR" >&2; exit 2; }
fi

# Compare all pairs, accumulate significant changes
changes=""
for pair in "${pairs[@]}"; do
  bf="${pair%%$'\t'*}"
  cf="${pair##*$'\t'}"
  suite="$(basename "$cf")"; suite="${suite#Performance.}"; suite="${suite%-report-full.json}"
  echo "Comparing ${suite} ..."
  out="$(compare_pair "$bf" "$cf")"
  [[ -n "$out" ]] && changes+="${out}"$'\n'
done
echo

# Round to 2 decimals inside jq so we never feed floats to printf.
r2() { echo "($1*100|round)/100"; }

print_table() {
  # $1 = "reg" (slower, pct>0) or "imp" (faster, pct<0)
  local mode="$1"
  {
    printf 'Benchmark\tBaseline (ns)\tCurrent (ns)\tChange (%%)\n'
    printf '%s' "$changes" | jq -rs --arg mode "$mode" '
      map(select(if $mode == "reg" then .pct > 0 else .pct < 0 end))
      | sort_by(if $mode == "reg" then -.pct else .pct end)
      | .[]
      | [ .name,
          (('"$(r2 .base)"')|tostring),
          (('"$(r2 .cur)"')|tostring),
          (('"$(r2 .pct)"')|tostring) ]
      | @tsv'
  } | column -t -s $'\t'
}

count_changes() {
  # $1 = "reg" | "imp"
  [[ -z "$changes" ]] && { echo 0; return; }
  printf '%s' "$changes" | jq -rs --arg mode "$1" \
    '[.[] | select(if $mode == "reg" then .pct > 0 else .pct < 0 end)] | length'
}

reg_count=$(count_changes reg)
imp_count=$(count_changes imp)

if [[ "$reg_count" -gt 0 ]]; then
  echo "[!] Performance regressions detected ($reg_count)"
  echo "============================================"
  print_table reg
  echo
else
  echo "[OK] No performance regressions detected."
  echo
fi

if [[ "$imp_count" -gt 0 ]]; then
  echo "[+] Performance improvements detected ($imp_count)"
  echo "============================================"
  print_table imp
  echo
fi

if [[ "$reg_count" -gt 0 ]]; then
  echo "Performance regression check failed. Please review the results above."
  exit 1
fi

echo "Performance regression check passed."
exit 0
