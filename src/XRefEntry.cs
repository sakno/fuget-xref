using System;
using System.Reflection.Metadata;

namespace XRefGenerator
{
    internal readonly struct XRefEntry
    {
        internal XRefEntry(in TypeDefinition type, MetadataReader reader, string moduleName, string packageId, string packageVersion, string tfm)
        {
            string ns, name;
            if (type.IsNested)
            {
                name = GetNestedTypeName(type, reader, out ns);
            }
            else
            {
                ns = reader.GetString(type.Namespace);
                name = reader.GetString(type.Name);
            }

            FullName = ns + '.' + name;
            Link = new Uri($"https://fuget.org/packages/{packageId}/{packageVersion}/lib/{tfm}/{moduleName}/{ns}/{name}");
            Name = name;
        }

        private static string GetNestedTypeName(TypeDefinition type, MetadataReader reader, out string ns)
        {
            var result = reader.GetString(type.Name);
            ns = string.Empty;
            for (TypeDefinitionHandle handle = type.GetDeclaringType(); !handle.IsNil; handle = type.GetDeclaringType())
            {
                type = reader.GetTypeDefinition(handle);
                result = reader.GetString(type.Name) + '.' + result;
                if (!type.IsNested)
                    ns = reader.GetString(type.Namespace);
            }

            return result;
        }

        internal string Name { get; }

        internal string FullName { get; }

        internal Uri Link { get; }
    }
}
