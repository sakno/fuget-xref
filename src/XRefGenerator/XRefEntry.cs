using System;

namespace XRefGenerator
{
    internal readonly struct XRefEntry
    {
        internal XRefEntry(Type type, string packageId, string packageVersion, string tfm)
        {
            Name = type.Name;
            FullName = type.FullName;
            Link = new Uri($"https://fuget.org/packages/{packageId}/{packageVersion}/lib/{tfm}/{type.Assembly.ManifestModule.ScopeName}/{type.Namespace}/{type.Name}");
        }

        internal string Name { get; }

        internal string FullName { get; }

        internal Uri Link { get; }
    }
}
