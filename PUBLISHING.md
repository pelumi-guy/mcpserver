# MCPServer Publishing Guide

This guide helps you publish MCPServer to your GitHub profile:
https://github.com/Ekundayo-tech

## Recommended Repository Name

Primary choice:
- mcpserver-figma-semantic

Alternative:
- mcpserver

## Suggested First Release Version

- v0.1.0

Rationale:
- Core API and parser are functional.
- Semantic transform and tests are in place.
- Open-source project files are prepared.

## Pre-Publish Checklist

- Ensure no secrets exist in appsettings or commit history.
- Confirm the solution builds locally.
- Confirm tests pass locally.
- Review generated JSON files and decide whether to keep sample output in repo.
- Verify LICENSE, README, CONTRIBUTING, and SECURITY are present.

## Local Validation Commands

Run from project root:

dotnet build MCPServer.sln
dotnet test MCPServer.sln

## Create the Repository on GitHub

Option A: Create manually on GitHub UI, then connect local repo.

Option B: If GitHub CLI is installed:

gh repo create Ekundayo-tech/mcpserver-figma-semantic --public --source . --remote origin --push

## If Remote Is Not Yet Configured

git init
git branch -M main
git remote add origin https://github.com/Ekundayo-tech/mcpserver-figma-semantic.git

## Suggested First Commit Message Set

Commit 1:
- chore: rename solution and projects to MCPServer

Commit 2:
- docs: add comprehensive README and open source policies

Commit 3:
- feat: add semantic transform pipeline with node-id scope support

Commit 4:
- test: add parser and semantic regression tests

If you prefer a single initial commit:
- chore: open-source MCPServer initial release

## Example Initial Push Flow

git add .
git commit -m "chore: open-source MCPServer initial release"
git push -u origin main

## Tag and Create First Release

git tag -a v0.1.0 -m "MCPServer v0.1.0"
git push origin v0.1.0

Then create a GitHub Release with:
- Title: MCPServer v0.1.0
- Notes summary:
  - Figma parse endpoint
  - Semantic transform endpoint
  - CLI parse and transform support
  - Node-id scoped transform support
  - Test coverage for parser and semantic extraction

## Suggested GitHub Topics

- dotnet
- aspnetcore
- figma
- mcp
- parser
- design-to-code
- semantic-transform

## Author

Gbenga Ekundayo
