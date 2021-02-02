using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace XRefGenerator
{
    static class Program
    {
        private const string NuGetPublicFeed = "https://api.nuget.org/v3/index.json";

        private static string GetRandomPath() => Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        private static async Task<FileStream> DownloadPackageAsync(string packageId, NuGetVersion packageVersion)
        {
            // inialize NuGet client
            var cache = new SourceCacheContext();
            var source = Repository.CreateSource(Repository.Provider.GetCoreV3(), NuGetPublicFeed);
            var resource = await source.GetResourceAsync<FindPackageByIdResource>();

            // create temp file for package
            var packageFile = new FileStream(GetRandomPath(), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous | FileOptions.DeleteOnClose);

            // download NuGet package
            await resource.CopyNupkgToStreamAsync(packageId, packageVersion, packageFile, cache, NullLogger.Instance, default);
            await packageFile.FlushAsync();
            return packageFile;
        }

        private static async IAsyncEnumerable<Assembly> LoadPackageAssemblies(Stream package, AssemblyLoadContext context, NuGetFramework tfm)
        {
            using var reader = new PackageArchiveReader(package);
            var libs = await reader.GetLibItemsAsync(default);
            foreach (var lib in libs)
            {
                if (lib.TargetFramework == tfm)
                {
                    foreach(var path in lib.Items)
                    {
                        var extension = Path.GetExtension(path);
                        if (string.Equals(".dll", extension, StringComparison.OrdinalIgnoreCase))
                        {
                            await using var content = await reader.GetStreamAsync(path, default);
                            yield return LoadFromPackage(content, context);
                        }
                    }
                }
            }

            static Assembly LoadFromPackage(Stream stream, AssemblyLoadContext context)
            {
                using var tempFile = new FileStream(GetRandomPath(), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose | FileOptions.SequentialScan);
                stream.CopyTo(tempFile);
                tempFile.Position = 0L;
                return context.LoadFromStream(tempFile);
            }
        }

        private static async Task SaveXRefMapAsync(XRefMap map, string outputFileName)
        {
            await using var output = new FileStream(outputFileName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            await using var writer = new StreamWriter(output, Encoding.UTF8);
            await writer.WriteLineAsync("references:");
            foreach (var (uuid, entry) in map)
            {
                await writer.WriteLineAsync("- uid: " + uuid);
                await writer.WriteLineAsync("  name: " + entry.Name);
                await writer.WriteLineAsync("  href: " + entry.Link.AbsoluteUri);
                await writer.WriteLineAsync("  fullName: " + entry.FullName);
            }

            await writer.FlushAsync();
        }

        private static async Task ExecuteAsync(string packageId, string packageVersion, string tfm, string outputFileName)
        {
            // download NuGet package
            await using var package = await DownloadPackageAsync(packageId, new NuGetVersion(packageVersion));
            package.Position = 0L;

            // extract DLLs
            var context = new AssemblyLoadContext(null, true);
            var map = new XRefMap(packageId, packageVersion, tfm);
            await foreach (var assembly in LoadPackageAssemblies(package, context, NuGetFramework.Parse(tfm)))
            {
                // load xrefs
                map.LoadTypes(assembly);
            }

            // dump yml file
            await SaveXRefMapAsync(map, outputFileName);
        }

        static async Task Main(string[] args)
        {
            // 0 - package name, 1 - package version, 2 - TFM, 3 - output file name
            switch(args.Length)
            {
                case 0:
                    Console.WriteLine("Arguments:");
                    Console.WriteLine("\tPackage Name on NuGet Feed");
                    Console.WriteLine("\tPackage Version on NuGet Feed");
                    Console.WriteLine("\tTarget Framework Moniker (net5.0, netstandard2.1)");
                    Console.WriteLine("\tThe path to output YML file");
                    break;
                case 1:
                    Console.WriteLine("Missing configuration");
                    break;
                case 2:
                    Console.WriteLine("Missing Target Framework Moniker");
                    break;
                case 3:
                    Console.WriteLine("Missing output file name");
                    break;
                case 4:
                    await ExecuteAsync(args[0], args[1], args[2], args[3]);
                    break;
                default:
                    Console.WriteLine("Invalid number of arguments");
                    break;
            }
        }
    }
}
