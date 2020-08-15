using System;
using System.Collections.Generic;
using ByrneLabs.Commons.Ioc;
using ByrneLabs.Commons.Mapping;
using ByrneLabs.Commons.Mapping.TestUtilities;
using ByrneLabs.Commons.TestUtilities;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Persistence.TestUtilities
{
    [PublicAPI]
    public abstract class BaseDataEntityMappingTest<TMapDefinition, TFrom, TTo> : BaseMappingTest<TMapDefinition, TFrom, TTo>, IDisposable where TMapDefinition : IMapper<TFrom, TTo>
    {
        private IContainer _container;

        protected BaseDataEntityMappingTest()
        {
            _container = DefaultContainer.CreateEmptyContainer();
        }

        protected override IContainer Container => _container;

        protected override IEnumerable<MappedPropertyValidator<TFrom, TTo>> MapValidators => null;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override IEnumerable<TFrom> CreateTestObjects() => DataGenerator.Generate<TFrom>(10);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _container.Dispose();
            }
        }
    }
}
