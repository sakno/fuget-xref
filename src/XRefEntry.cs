using System;
using System.Reflection.Metadata;

namespace XRefGenerator
{
    internal readonly struct XRefEntry
    {
        internal XRefEntry(TypeDefinition type, MetadataReader reader, string moduleName, string packageId, string packageVersion, string tfm)
        {
            var ns = reader.GetString(type.Namespace);
            var name = reader.GetString(type.Name);
            FullName = ns + '.' + name;
            Link = new Uri($"https://fuget.org/packages/{packageId}/{packageVersion}/lib/{tfm}/{moduleName}/{ns}/{name}");
            Name = name;
        }

        internal string Name { get; }

        internal string FullName { get; }

        internal Uri Link { get; }
    }
}
