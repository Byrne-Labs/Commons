using JetBrains.Annotations;

namespace ByrneLabs.Commons.MasterDataManagement
{
    [PublicAPI]
    public class DataSource
    {
        public int DefaultPriority { get; set; }

        public string Name { get; set; }
    }
}
