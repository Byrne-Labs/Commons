using System.Collections.Generic;

namespace ByrneLabs.Commons.Mapping
{
    public interface IMapManager
    {
        void Map<TFrom, TTo>(TFrom fromSource, TTo toTarget);

        TTo Map<TFrom, TTo>(TFrom fromSource);

        IEnumerable<TTo> Map<TFrom, TTo>(IEnumerable<TFrom> fromSource);
    }
}
