using System.Collections.Generic;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Mapping
{
    [PublicAPI]
    public interface IMapManager
    {
        void Map<TFrom, TTo>(TFrom fromSource, TTo toTarget);

        TTo Map<TFrom, TTo>(TFrom fromSource);

        IEnumerable<TTo> Map<TFrom, TTo>(IEnumerable<TFrom> fromSource);
    }
}
