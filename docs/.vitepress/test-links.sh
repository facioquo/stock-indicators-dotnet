#!/bin/bash
#
# Broken-link check over the built site.
#
# This script is the single definition of the check: the "Website URLs"
# workflow runs it too, so the ignore lists below cannot drift between local
# runs and CI the way they did when both kept their own copy.
#
# html-proofer is Ruby. It runs natively when a `htmlproofer` binary is on PATH
# (CI installs one via ruby/setup-ruby) and falls back to a pinned container
# otherwise, so Windows and macOS contributors need no local Ruby.
#
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "${SCRIPT_DIR}/.."

# Pinned so a new html-proofer release cannot change the result of a rerun.
HTMLPROOFER_VERSION="${HTMLPROOFER_VERSION:-5.0.10}"
RUBY_IMAGE="ruby:3.4"
GEM_VOLUME="stock-indicators-htmlproofer-gems"
DIST=".vitepress/dist"

# Hosts that rate-limit or bot-block automated checkers. These are reachability
# false negatives, not broken links.
IGNORE_URLS='/fonts.gstatic.com/,/github\.com\/(DaveSkender\/Stock\.Indicators|facioquo\/stock-indicators-dotnet)\/(edit|blob|tree|discussions)\//,/((www\.)?google\.[^\/]+\/search\?)/,/(googletagmanager\.com|google-analytics\.com|analytics\.google\.com)/'

# 302 redirect, 401/402/403 auth-walled, 406/429 bot-throttled, 999 LinkedIn.
IGNORE_STATUS_CODES='302,401,402,403,406,429,999'

PROOFER_ARGS=(
  --no-enforce-https
  --no-check-external-hash
  --ignore-status-codes "${IGNORE_STATUS_CODES}"
  --ignore-urls "${IGNORE_URLS}"
)

# ANALYTICS_ENABLED is deliberately not set: config.mts only injects the gtag
# snippet when it is exactly "true", so the site under test carries no Google
# Analytics for html-proofer to resolve or report to. The analytics hosts stay
# in IGNORE_URLS as a backstop in case the build is ever run with it on.
pnpm run docs:build

run_native() {
  htmlproofer "${DIST}" "${PROOFER_ARGS[@]}"
}

run_docker() {
  # Stream dist into the container rather than bind-mounting: avoids Windows
  # path translation and keeps the container read-only against the workspace.
  #
  # The gem home is a named volume so the ~21-gem install happens on the first
  # run only; later runs and the retry below reuse it. `gem install` writes its
  # binstubs to /usr/local/bundle/bin, which is not on PATH in this image, so
  # invoke htmlproofer by absolute path.
  tar -C "${DIST}" -cf - . |
    docker run --rm -i \
      -v "${GEM_VOLUME}:/usr/local/bundle" \
      "${RUBY_IMAGE}" bash -lc "
        set -euo pipefail
        mkdir -p /site
        tar -xf - -C /site
        gem list -i html-proofer -v '${HTMLPROOFER_VERSION}' >/dev/null 2>&1 ||
          gem install --no-document html-proofer -v '${HTMLPROOFER_VERSION}'
        /usr/local/bundle/bin/htmlproofer /site $(printf '%q ' "${PROOFER_ARGS[@]}")"
}

# Probe by running it, not with `command -v`: Windows ships an App Execution
# Alias shim named `htmlproofer` that resolves on PATH but fails to launch, and
# a PATH-only check would pick that over a working Docker fallback.
if htmlproofer --version >/dev/null 2>&1; then
  echo "Using native htmlproofer: $(htmlproofer --version 2>&1 | head -n 1)"
  run_check() { run_native; }
elif command -v docker >/dev/null 2>&1; then
  echo "No native htmlproofer; using ${RUBY_IMAGE} with html-proofer ${HTMLPROOFER_VERSION}"
  run_check() { run_docker; }
else
  echo "Error: needs either a 'htmlproofer' binary on PATH or Docker." >&2
  echo "  gem install html-proofer -v ${HTMLPROOFER_VERSION}" >&2
  exit 1
fi

# External hosts fail intermittently; one retry separates a flaky response from
# a genuinely broken link.
max_attempts=2
for attempt in $(seq 1 $max_attempts); do
  if run_check; then
    exit 0
  fi

  if [ "$attempt" -lt "$max_attempts" ]; then
    echo "htmlproofer failed on attempt ${attempt}/${max_attempts}; retrying in 5 seconds..."
    sleep 5
  fi
done

echo "htmlproofer failed after ${max_attempts} attempts." >&2
exit 1
