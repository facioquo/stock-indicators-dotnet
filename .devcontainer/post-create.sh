#!/bin/bash

# Dev Container Startup Script for Stock Indicators
# Handles initialization of development dependencies

echo "🚀 Starting environment setup..."

# Verify .NET is available
echo "🔍 Verifying .NET environment..."
dotnet --version

# Verify pnpm is available (installed via devcontainer feature)
echo "🔍 Verifying pnpm..."
pnpm --version

echo "🧰 Installing .NET-based tools..."
dotnet tool restore

# Agent code intelligence. Installed globally, not from the manifest, because
# LSP clients spawn the bare `csharp-ls` binary from PATH.
echo "🧠 Installing C# language server..."
dotnet tool install --global csharp-ls || dotnet tool update --global csharp-ls

# Claude Code only discovers skills under .claude/skills. Restore the symlink
# when a clone lands on a filesystem that materialized it as a plain file.
if [ ! -L .claude/skills ]; then
  echo "🔗 Linking .claude/skills to .agents/skills..."
  mkdir -p .claude
  rm -rf .claude/skills
  ln -s ../.agents/skills .claude/skills
fi

# Refresh git repo
echo "🗂️  Fetch and pull from git..."
git fetch && git pull

# Restore .NET packages
echo "📦 Restoring .NET packages..."
dotnet restore

# Write token to your user-level ~/.npmrc (not the project .npmrc)
echo "📦 Set user scope .npmrc auth..."
pnpm config set "//npm.pkg.github.com/:_authToken=${GITHUB_TOKEN}"

echo "✅ Dev environment setup complete!"
