#!/usr/bin/env bash
set -euo pipefail

# Regenerates the downloadable examples ZIP from docs/examples.
# Includes only source files (tracked or untracked-but-not-ignored),
# so bin/, obj/, .vs/ and other ignored build artifacts are excluded.
# Docs-site-only files (Vue components, chart scripts) are also excluded.

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
SRC_DIR="$ROOT_DIR/docs/examples"
OUT_FILE="$ROOT_DIR/docs/.vitepress/public/FacioQuo.Stock.Indicators-Examples.zip"

# docs-site files that are not part of the Examples solution
EXCLUDES=(
  "*.vue"
  "*.ts"
  "custom-chart.md"
)

echo "📦 Building $(basename "$OUT_FILE") from docs/examples..."

cd "$SRC_DIR"

FILES=()
while IFS= read -r f; do
  skip=false
  for pattern in "${EXCLUDES[@]}"; do
    # shellcheck disable=SC2053
    if [[ "$(basename "$f")" == $pattern ]]; then
      skip=true
      break
    fi
  done
  if [ "$skip" = false ] && [ -f "$f" ]; then
    FILES+=("$f")
  fi
done < <(git ls-files --cached --others --exclude-standard . | sort -u)

if [ ${#FILES[@]} -eq 0 ]; then
  echo "❌ No files found in $SRC_DIR" >&2
  exit 1
fi

TMP_FILE="$(mktemp -u).zip"

if command -v zip >/dev/null 2>&1; then
  printf '%s\n' "${FILES[@]}" | zip -q -X "$TMP_FILE" -@
elif command -v bsdtar >/dev/null 2>&1; then
  bsdtar -a -cf "$TMP_FILE" "${FILES[@]}"
elif [ -x /c/Windows/System32/tar.exe ]; then
  # Windows built-in tar is bsdtar; needs a Windows-style output path
  /c/Windows/System32/tar.exe -a -cf "$(cygpath -w "$TMP_FILE")" "${FILES[@]}"
else
  echo "❌ Requires 'zip' or 'bsdtar' to create the archive" >&2
  exit 1
fi

mv -f "$TMP_FILE" "$OUT_FILE"

printf '  %s\n' "${FILES[@]}"
echo ""
echo "✅ Wrote ${#FILES[@]} files to ${OUT_FILE#"$ROOT_DIR"/}"
