using System.Data.SqlTypes;
using System.Security.Cryptography;
using Microsoft.SqlServer.Server;

namespace ByrneLabs.Commons.SqlServerUtilities
{
    public partial class UserDefinedFunctions
    {
        [SqlFunction]
        public static SqlBinary BigHashBytes(SqlString algorithmName, [SqlFacet(MaxSize = -1)] SqlBinary data)
        {
            SqlBinary hash;
            if (!data.IsNull)
            {
                var algorithm = HashAlgorithm.Create(algorithmName.Value);

                hash = new SqlBinary(algorithm.ComputeHash(data.Value));
            }
            else
            {
                hash = null;
            }

            return hash;
        }

        [SqlFunction]
        public static SqlBinary BigHashString(SqlString algorithmName, [SqlFacet(MaxSize = -1)] SqlString data)
        {
            SqlBinary hash;
            if (!data.IsNull)
            {
                var algorithm = HashAlgorithm.Create(algorithmName.Value);

                var bytes = data.GetUnicodeBytes();

                hash = new SqlBinary(algorithm.ComputeHash(bytes));
            }
            else
            {
                hash = null;
            }

            return hash;
        }
    }
}
