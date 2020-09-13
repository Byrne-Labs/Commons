using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ByrneLabs.Commons.Domain;
using ByrneLabs.Commons.Ioc;
using ByrneLabs.Commons.Persistence;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ByrneLabs.Commons.TestUtilities
{
    [PublicAPI]
    public abstract class TestHelper<TInterface, TImplementation> : TestDataAggregator, ITestHelper<TInterface> where TInterface : class where TImplementation : class, TInterface
    {
        private TInterface _testedObject;

        protected TestHelper(IContainer container, params ITestDataProvider[] testDataProviders) : base(testDataProviders)
        {
            Container = container.CreateChildContainer();

            if (typeof(TInterface) != typeof(object) && !Container.RegisteredTypes.Contains(new KeyValuePair<Type, Type>(typeof(TInterface), typeof(TImplementation))))
            {
                Container.AddSingleton<TInterface, TImplementation>();
            }
        }

        public IContainer Container { get; }

        public TInterface TestedObject
        {
            get
            {
                if (typeof(TInterface) == typeof(object))
                {
                    throw new NotSupportedException("This operation is not valid when no tested type was specified");
                }

                return _testedObject ??= Container.GetRequiredService<TInterface>();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposedManaged)
        {
            if (disposedManaged)
            {
                Container.Dispose();
            }
        }

        [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed", Justification = "For the purposes of a mocked repository, we don't care if the removed entity was present")]
        protected Mock<TRepository> MockRepository<TEntity, TRepository>(MockBehavior mockBehavior = MockBehavior.Strict) where TEntity : class, IEntity where TRepository : class, IRepository<TEntity>
        {
            var mockEntities = TestData<TEntity>().ToList();
            var mockRepository = new Mock<TRepository>(mockBehavior);
#pragma warning disable CA1806 // Do not ignore method results
            mockRepository.Setup(r => r.Delete(It.IsAny<IEnumerable<TEntity>>())).Callback((IEnumerable<TEntity> entities) => entities.Select(mockEntities.Remove));
#pragma warning restore CA1806 // Do not ignore method results
            mockRepository.Setup(r => r.FindAll()).Returns(mockEntities.Select(mockEntity => mockEntity.Clone(CloneDepth.Shallow)).Cast<TEntity>().ToList().AsReadOnly());
            mockRepository.Setup(r => r.Save(It.IsAny<IEnumerable<TEntity>>())).Callback((IEnumerable<TEntity> entities) =>
            {
                var entitiesArray = entities as TEntity[] ?? entities.ToArray();
                foreach (var entity in entitiesArray.Where(entity => entity.EntityId == null))
                {
                    entity.EntityId = Guid.NewGuid();
                }

#pragma warning disable CA1806 // Do not ignore method results
                mockEntities.Where(mockEntity => entities.Any(entity => entity.EntityId == mockEntity.EntityId)).Select(mockEntities.Remove);
#pragma warning restore CA1806 // Do not ignore method results
                mockEntities.AddRange(entitiesArray);
            });
            Container.RegisterInstance(mockRepository.Object);

            return mockRepository;
        }
    }
}
