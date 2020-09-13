using System.Collections.Generic;
using ByrneLabs.Commons.Ioc;
using JetBrains.Annotations;
using Xunit;

namespace ByrneLabs.Commons.Mapping.TestUtilities
{
    [PublicAPI]
    public abstract class BaseMappingTest<TMapper, TFrom, TTo> where TMapper : IMapper<TFrom, TTo>
    {
        protected abstract IContainer Container { get; }

        protected abstract IEnumerable<MappedPropertyValidator<TFrom, TTo>> MapValidators { get; }

        protected abstract IEnumerable<TFrom> CreateTestObjects();

        protected abstract void ValidateMapping(TFrom fromEntity, TTo toEntity);

        protected void TestMapping()
        {
            Container.RegisterType<IMapManager, MapManager>();
            Container.RegisterType<IMapper<TFrom, TTo>, TMapper>();
            var mappingManager = Container.Resolve<IMapManager>();

            var fromObjects = CreateTestObjects();
            foreach (var from in fromObjects)
            {
                var to = mappingManager.Map<TFrom, TTo>(from);
                Assert.NotNull(to);
                if (MapValidators != null)
                {
                    foreach (var mapValidator in MapValidators)
                    {
                        mapValidator.Validate(from, to);
                    }
                }

                ValidateMapping(from, to);
            }
        }
    }
}
