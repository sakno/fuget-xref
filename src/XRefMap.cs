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

        private void AddNestedTypes(IReadOnlyCollection<TypeDefinitionHandle> nestedTypes, MetadataReader reader, string moduleName)
        {
            // analyze nested types
            foreach (var handle in nestedTypes)
            {
                var nestedType = reader.GetTypeDefinition(handle);
                if (IsExternallyVisible(nestedType.Attributes))
                {
                    Add(nestedType, reader, moduleName);
                }
            }

            static bool IsExternallyVisible(TypeAttributes attributes)
            {
                attributes &= TypeAttributes.VisibilityMask;
                return attributes == TypeAttributes.NestedPublic ||
                    attributes == TypeAttributes.NestedFamORAssem ||
                    attributes == TypeAttributes.NestedFamily;
            }
        }

        private void Add(in TypeDefinition type, MetadataReader reader, string moduleName)
        {
            var entry = new XRefEntry(in type, reader, moduleName, packageId, packageVersion, tfm);
            Add(entry.FullName, entry);

            AddNestedTypes(type.GetNestedTypes(), reader, moduleName);
        }

        internal void LoadTypes(PEReader assembly)
        {
            var reader = assembly.GetMetadataReader();
            var moduleName = reader.GetString(reader.GetModuleDefinition().Name);
            foreach (var handle in reader.TypeDefinitions)
            {
                var type = reader.GetTypeDefinition(handle);
                if (IsExternallyVisible(type.Attributes) && !type.IsNested)
                    Add(in type, reader, moduleName);
            }

            static bool IsExternallyVisible(TypeAttributes attributes)
            {
                attributes &= TypeAttributes.VisibilityMask;
                return attributes == TypeAttributes.Public;
            }
        }
    }
}
