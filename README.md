# MCPServer

MCPServer is a .NET 8 API service that transforms Figma design files into structured semantic JSON for downstream use cases such as:
- UI workflow analysis
- rapid backend scaffolding
- AI-assisted product prototyping
- design-to-data pipelines

## Author

Gbenga Ekundayo

## Maintainer Profile

GitHub: https://github.com/Ekundayo-tech

## Project Overview

This repository exposes two main capabilities:
- Parse mode: extracts detailed screen and element data from a Figma file.
- Transform mode: outputs a compact semantic structure focused on screen intent.

The transform output is intentionally constrained to a stable shape:
- screen
- fields
- labels
- actions

## Repository Structure

- MCPServer.sln
- MCPServer.Api/
  - ASP.NET Core API project
  - Figma client, parser, semantic transformer, controllers
- MCPServer.Tests/
  - xUnit test project for parser and semantic extraction behavior
- logistic.json
  - Generated semantic output sample

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- xUnit
- Polly (retry policies)
- Swashbuckle (Swagger/OpenAPI)

## Requirements

- .NET SDK 8.x
- A valid Figma personal access token

## Configuration

Set your token in:
- MCPServer.Api/appsettings.json
- or environment variables used by your configuration source

If your current setup reads from a specific key in appsettings, keep using that key.

## Build and Test

From repository root:

```bash
dotnet build MCPServer.sln
dotnet test MCPServer.sln
```

## Run the API

```bash
dotnet run --project MCPServer.Api/MCPServer.Api.csproj
```

Swagger is available in development mode.

## CLI Usage

Run from repository root.

### Parse a file

```bash
dotnet run --project MCPServer.Api/MCPServer.Api.csproj -- parse <fileKey>
```

Optional filters:

```bash
dotnet run --project MCPServer.Api/MCPServer.Api.csproj -- parse <fileKey> --page "Page 1" --frame "Signup"
```

### Transform to semantic JSON

Using file key:

```bash
dotnet run --project MCPServer.Api/MCPServer.Api.csproj -- transform <fileKey> --out logistic.json
```

Using full Figma URL:

```bash
dotnet run --project MCPServer.Api/MCPServer.Api.csproj -- transform "https://www.figma.com/design/<fileKey>/<name>?node-id=0-1" --out logistic.json
```

Optional node scope override:

```bash
dotnet run --project MCPServer.Api/MCPServer.Api.csproj -- transform <fileKey> --node-id 0:1 --out logistic.json
```

## HTTP Endpoints

- GET /api/figma/parse
- GET /api/figma/transform

Example:

```text
GET /api/figma/transform?fileKey=<fileKey>&nodeId=0:1
```

## Example Output

The generated transform JSON is an array like:

```json
[
  {
    "screen": "Signup",
    "fields": ["email_address", "password"],
    "labels": ["Already have an account?"],
    "actions": ["signup", "login"]
  }
]
```

## Known Operational Notes

- Figma API can return HTTP 429 when rate limits are exceeded.
- The project already includes retry behavior for transient failures.
- For repeated runs, allow cool-down time if you hit rate limits.

## Open Source Publishing Checklist

For an exact first-release workflow (repository naming, commit messages, push and tagging commands), see [PUBLISHING.md](PUBLISHING.md).

Before publishing publicly:
- LICENSE added (MIT).
- CONTRIBUTING.md added.
- SECURITY.md added.
- .gitignore added for .NET artifacts and generated outputs.
- Remove any secrets from config or commit history.
- Confirm no generated build artifacts are committed.

## Naming Recommendation

Best project name to use publicly:
- MCPServer

Alternative names if you want stronger discoverability:
- Figma Design Semantic MCP Server
- Design2JSON MCP Server
- Figma Semantic Transform Server

## Roadmap Ideas

- Add deterministic deduplication for repeated frames
- Add optional output profile for backend contract generation
- Add richer node-id scoping and frame selection controls
- Add CI workflow for build and tests
- Add Dockerfile and dev container
