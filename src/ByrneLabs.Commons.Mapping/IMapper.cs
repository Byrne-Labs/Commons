using System.Collections.Generic;

namespace ByrneLabs.Commons.Mapping
{
    public interface IMapper<in TFrom, TTo>
    {
        void Map(TFrom fromSource, TTo toTarget);

        TTo Map(TFrom fromSource);

        IEnumerable<TTo> Map(IEnumerable<TFrom> fromSource);
    }
}
