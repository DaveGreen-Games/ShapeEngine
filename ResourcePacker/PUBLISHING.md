# Publishing `DaveGreen.ShapeEngine.ResourcePacker`

This document is for maintainers of `ResourcePacker`.

The package README should stay consumer-focused:

- what the tool does
- how to install it
- how to use it

This file keeps the release workflow separate.

## Build and pack locally

```bash
cd /tmp
mkdir -p shapeengine-resourcepacker-artifacts

dotnet build "/Users/davegreen/Developer/repos/ShapeEngine/ResourcePacker/ResourcePacker.csproj" -c Release

dotnet pack "/Users/davegreen/Developer/repos/ShapeEngine/ResourcePacker/ResourcePacker.csproj" -c Release -o "/tmp/shapeengine-resourcepacker-artifacts"
```

Expected output package:

```text
/tmp/shapeengine-resourcepacker-artifacts/DaveGreen.ShapeEngine.ResourcePacker.<version>.nupkg
```

## Test as a local tool before publishing

```bash
cd /path/to/a/test/repo

dotnet new tool-manifest

dotnet tool install --local DaveGreen.ShapeEngine.ResourcePacker \
  --add-source /tmp/shapeengine-resourcepacker-artifacts \
  --version <version>

dotnet tool restore

dotnet tool run shapeengine-resourcepacker -- --help
```

## Publish to NuGet.org

```bash
dotnet nuget push /absolute/path/to/DaveGreen.ShapeEngine.ResourcePacker.<version>.nupkg \
  --api-key <NUGET_API_KEY> \
  --source https://api.nuget.org/v3/index.json
```

## Pre-publish checklist

- confirm `PackageId` is `DaveGreen.ShapeEngine.ResourcePacker`
- confirm `ToolCommandName` is `shapeengine-resourcepacker`
- update the package version in `ResourcePacker.csproj`
- verify `README.md` is consumer-focused and current
- verify `dotnet pack` succeeds
- verify local tool install/restore works from the produced package
- verify repository URL and license metadata are correct


