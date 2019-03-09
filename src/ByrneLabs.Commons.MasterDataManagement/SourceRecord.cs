using JetBrains.Annotations;

namespace ByrneLabs.Commons.MasterDataManagement
{
    [PublicAPI]
    public class SourceRecord
    {
        public object Data { get; set; }

        public DataSource DataSource { get; set; }
    }
}
