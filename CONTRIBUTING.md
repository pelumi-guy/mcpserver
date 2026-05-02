# Contributing to MCPServer

Thanks for your interest in contributing.

## Code of Conduct

Be respectful, constructive, and professional in all interactions.

## How to Contribute

1. Fork the repository.
2. Create a branch from main.
3. Implement your change with focused commits.
4. Add or update tests where applicable.
5. Open a pull request with a clear summary.

## Development Setup

1. Install .NET 8 SDK.
2. Clone your fork.
3. Run restore, build, and tests:

```bash
dotnet restore MCPServer.sln
dotnet build MCPServer.sln
dotnet test MCPServer.sln
```

## Pull Request Guidelines

- Keep changes scoped.
- Include rationale and impact.
- Reference related issues when available.
- Ensure all tests pass.

## Commit Guidelines

Use clear commit messages. Prefer concise, imperative style, for example:
- Add node-id scope validation to transform endpoint
- Improve action classification for onboarding screens

## Reporting Bugs

When reporting issues, include:
- Expected behavior
- Actual behavior
- Steps to reproduce
- Environment details (.NET SDK version, OS)

## Suggesting Enhancements

Open an issue with:
- Problem statement
- Proposed approach
- Expected value to users

## Security Issues

Please do not open public issues for sensitive vulnerabilities.
See SECURITY.md for reporting instructions.
