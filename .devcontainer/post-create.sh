#!/bin/bash

# Dev Container Startup Script for Stock Indicators
# Handles initialization of development dependencies

echo "🚀 Starting environment setup..."

echo "🔍 Verifying .NET environment..."
dotnet --version

echo "🔍 Verifying pnpm..."
pnpm --version

echo "🧰 Installing .NET-based tools..."
dotnet tool restore

# Global, not from the manifest: LSP clients spawn the bare binary from PATH.
echo "🧠 Installing C# language server..."
dotnet tool install --global csharp-ls || dotnet tool update --global csharp-ls

# Claude Code only discovers skills under .claude/skills, and some filesystems
# materialize the symlink as a plain file on clone. Compare the target rather
# than the type, so a link pointing somewhere stale is also repaired.
if [ "$(readlink .claude/skills 2>/dev/null)" != "../.agents/skills" ]; then
  echo "🔗 Linking .claude/skills to .agents/skills..."
  mkdir -p .claude
  rm -rf .claude/skills
  ln -sn ../.agents/skills .claude/skills
fi

echo "🗂️  Fetch and pull from git..."
git fetch && git pull

echo "📦 Restoring .NET packages..."
dotnet restore

# Writes to pnpm's global auth.ini; pnpm ignores ${ENV_VAR} in a project .npmrc.
echo "📦 Set user scope .npmrc auth..."
pnpm config set "//npm.pkg.github.com/:_authToken=${GITHUB_TOKEN}"

echo "✅ Dev environment setup complete!"
