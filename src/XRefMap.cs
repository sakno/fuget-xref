using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace XRefGenerator
{
    internal sealed class XRefMap : Dictionary<string, XRefEntry>
    {
        private readonly string packageId, packageVersion, tfm;

        internal XRefMap(string packageId, string packageVersion, string tfm)
        {
            this.packageId = packageId;
            this.packageVersion = packageVersion;
            this.tfm = tfm;
        }

        private void Add(TypeDefinition type, MetadataReader reader, string moduleName)
        {
            var entry = new XRefEntry(type, reader, moduleName, packageId, packageVersion, tfm);
            Add(entry.FullName, entry);
        }

        internal void LoadTypes(PEReader assembly)
        {
            var reader = assembly.GetMetadataReader();
            var moduleName = reader.GetString(reader.GetModuleDefinition().Name);
            foreach (var handle in reader.TypeDefinitions)
            {
                var type = reader.GetTypeDefinition(handle);
                if (IsExternallyVisible(type.Attributes))
                    Add(type, reader, moduleName);
            }

            static bool IsExternallyVisible(TypeAttributes attributes) => (attributes & TypeAttributes.Public) != 0 ||
                (attributes & TypeAttributes.NestedPublic) != 0 ||
                (attributes & TypeAttributes.NestedFamily) != 0 ||
                (attributes & TypeAttributes.NestedFamORAssem) != 0;
        }
    }
}
