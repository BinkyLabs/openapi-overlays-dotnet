# Generation Test Data

This directory contains test data files for overlay generation tests.

## File Organization

### Source Overlays
- `test-overlay*.json` - Sample overlay documents used as input for generation tests
- `test-simple-overlay.json` - Simple overlay example

### Generated Results (Git Ignored)
- `test-result*.json` - Generated OpenAPI documents after applying overlays
- `test-simple-result.json` - Result from simple overlay application

## Usage

These files are used by:
- `OverlayGeneratorEndToEndTests.cs`
- `OverlayGeneratorJsonPathTests.cs`
- `OverlayGeneratorTests.cs`

## Note

Result files (`test-result*.json`) are automatically generated during tests and are excluded from git via `.gitignore` to avoid committing generated artifacts.
