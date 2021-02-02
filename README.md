# XREF Generator for FuGet
This tool allows to generate a file with cross-references for the specific NuGet package available on public feed. This file can be used in DocFX project. As a result, you can use cross-references to API related to your package in the documentation without buggy built-in API documentation generator from DocFX.

Usage:
```bash
cd ./src/
dotnet run -c Release -- <PackageName> <PackageVersion> <TargetFramework> <OutputFile>
```

Example:
```csharp
cd ./src/
dotnet run -c Release -- DotNext.Reflection 3.0.0 net5.0 DotNext.Reflection.xref
```

The produced file is in YAML format. It can be added to DocFX JSON configuration for your project as a source of cross-references. Examine [this](https://dotnet.github.io/docfx/tutorial/links_and_cross_references.html) article for more info.
