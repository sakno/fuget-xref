using System.Collections.Generic;
using System.Reflection;

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

        internal void LoadTypes(Assembly target)
        {
            foreach (var type in target.ExportedTypes)
            {
                Add(type.FullName, new XRefEntry(type, packageId, packageVersion, tfm));
            }
        }
    }
}
