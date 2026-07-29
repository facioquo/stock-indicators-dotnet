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
