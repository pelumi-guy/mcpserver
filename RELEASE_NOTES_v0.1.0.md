# MCPServer v0.1.0 Release Notes (Draft)

Release date: TBD

## Summary

v0.1.0 is the first public release of MCPServer, a .NET 8 API and CLI for parsing Figma file structures and producing semantic JSON output for downstream UI and workflow automation.

## Highlights

- Added Figma parse endpoint and semantic transform endpoint.
- Added CLI support for parse and transform workflows.
- Added node-id scoped transform support from URL or explicit argument.
- Improved semantic parser heuristics to reduce false positives in labels, actions, and fields.
- Added parser-focused regression tests for scoped extraction and noise filtering.
- Renamed project/solution branding from FigmaMcpServer to MCPServer.
- Added open-source baseline files: LICENSE, CONTRIBUTING, SECURITY, and .gitignore.

## Included Components

- MCPServer.Api
  - Controllers for parse and transform flows.
  - Figma API client and resilience handling.
  - Semantic parser and transform service pipeline.
- MCPServer.Tests
  - Unit tests for parser behavior and transform assumptions.

## Notes on Reliability

- Figma API rate limiting (HTTP 429) can still occur during repeated transform operations.
- Recommended operational pattern:
  - Retry with exponential backoff.
  - Use cached or most recent successful semantic output when throttled.

## Breaking Changes

- Project and solution naming changed to MCPServer.
- Existing local scripts referencing old names should be updated.

## Upgrade Guidance

1. Pull latest code from main.
2. Restore and build:
   - dotnet restore MCPServer.sln
   - dotnet build MCPServer.sln
3. Run tests:
   - dotnet test MCPServer.sln
4. Update local automation scripts to use MCPServer naming.

## Known Limitations

- Semantic extraction quality depends on Figma node naming conventions and hierarchy consistency.
- Extremely dense design files may require screen-level node scoping for best results.

## Security

- No credentials or secrets should be committed.
- Use local environment variables for sensitive config.

## Acknowledgements

Author: Gbenga Ekundayo
