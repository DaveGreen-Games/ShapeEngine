# ShapeEngine ResourcePacker

`ResourcePacker` is a .NET CLI tool for packing and unpacking ShapeEngine resource directories.

- Package ID: `DaveGreen.ShapeEngine.ResourcePacker`
- Tool command: `shapeengine-resourcepacker`
- Target framework: `net10.0`

It supports two formats:

- binary resource packs for compact distribution
- text-based packs when you use a `.txt` extension

## Get the tool

Install it as a local tool inside a consuming repository:

```bash
dotnet new tool-manifest

dotnet tool install --local DaveGreen.ShapeEngine.ResourcePacker
```

Then restore it on any machine that has the manifest:

```bash
dotnet tool restore
```

You can also install it globally if you prefer:

```bash
dotnet tool install --global DaveGreen.ShapeEngine.ResourcePacker
```

## Use the tool

The CLI keeps the current command shape:

```text
shapeengine-resourcepacker pack <sourceDirectoryPath> <outputFilePath> [--exceptions <.ext1> <.ext2> ...] [--debug] [--parallel]
shapeengine-resourcepacker unpack <sourceFilePath> <outputDirectoryPath> [--exceptions <.ext1> <.ext2> ...] [--debug] [--parallel]
shapeengine-resourcepacker help
shapeengine-resourcepacker --help
```

Notes:

- `.txt` output/input uses the text-based format.
- any other file extension uses the binary format.
- running the tool with no arguments still opens the interactive prompt.

## Common examples

Pack a `Resources` folder into a binary resource pack:

```bash
dotnet tool run shapeengine-resourcepacker -- pack Resources ./build/resources.res
```

Pack into a text-based resource pack:

```bash
dotnet tool run shapeengine-resourcepacker -- pack Resources ./build/resources.txt
```

Unpack a resource pack:

```bash
dotnet tool run shapeengine-resourcepacker -- unpack ./build/resources.res ./build/unpacked-resources
```

Exclude some file extensions while packing:

```bash
dotnet tool run shapeengine-resourcepacker -- pack Resources ./build/resources.res --exceptions .psd .aseprite
```

Enable debug logging and parallel processing:

```bash
dotnet tool run shapeengine-resourcepacker -- pack Resources ./build/resources.res --debug --parallel
```

## Recommended workflow for other repos

If another repo currently does this:

```bash
dotnet run --project ../ResourcePacker/ResourcePacker.csproj -- pack Resources <output>
```

replace it with:

```bash
dotnet tool restore

dotnet tool run shapeengine-resourcepacker -- pack Resources <output>
```

This removes the dependency on a sibling checkout path and makes the tool version explicit in the tool manifest.

## Develop and test from source

If you are working on `ResourcePacker` itself, you can still run it directly from source:

```bash
cd /tmp

dotnet run --project "/Users/davegreen/Developer/repos/ShapeEngine/ResourcePacker/ResourcePacker.csproj" -- help

dotnet run --project "/Users/davegreen/Developer/repos/ShapeEngine/ResourcePacker/ResourcePacker.csproj" -- pack "/absolute/path/to/Resources" "/absolute/path/to/output/resources.res"
```

For packing and publishing the NuGet tool package, see `PUBLISHING.md`.
