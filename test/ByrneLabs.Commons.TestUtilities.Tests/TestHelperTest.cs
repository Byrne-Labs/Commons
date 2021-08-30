using System;
using System.Collections.Generic;
using System.Linq;
using ByrneLabs.Commons.Domain;
using ByrneLabs.Commons.Ioc;
using ByrneLabs.Commons.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ByrneLabs.Commons.TestUtilities.Tests
{
    public class TestHelperTest
    {
        [Fact]
        public void TestTestHelper()
        {
            var testHelper = new TestHelperA<IServiceA, ServiceA>();
            var serviceA = testHelper.TestedObject;

            // ReSharper disable once InconsistentNaming
            var entityA1s = serviceA.FindAllEntityA1();
            Assert.NotEmpty(entityA1s);

            var entityA2 = testHelper.TestData<IEntityA2>().RandomItem();
            var returnedEntityA2 = serviceA.FindEntityA2(entityA2.Name);
            Assert.Equal(entityA2, returnedEntityA2);

            var entityA3 = testHelper.TestData<IEntityA3>().RandomItem();
            entityA3.EntityId = null;
            serviceA.SaveEntityA3(entityA3);
            Assert.NotNull(entityA3.EntityId);
        }

        #region Hammer Mocks

        // ReSharper disable All
        public class ContainerConfig : IContainerRegistrar
        {
            public void RegisterComponents(IContainer container)
            {
                container.RegisterType<IEntityA1Repository, EntityA1Repository>(ServiceLifetime.Singleton);
                container.RegisterType<IEntityA2Repository, EntityA2Repository>(ServiceLifetime.Singleton);
                container.RegisterType<IEntityA3Repository, EntityA3Repository>(ServiceLifetime.Singleton);
                container.RegisterType<IServiceA, ServiceA>(ServiceLifetime.Singleton);
            }
        }

        public class EntityA1 : Entity<EntityA1>, IEntityA1
        {
            public string Name { get; set; }
        }

        public class EntityA1Repository : IEntityA1Repository
        {
            public void Delete(IEnumerable<IEntityA1> items)
            {
            }

            public void Delete(IEntityA1 entity)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
            }

            public IEntityA1 Find(Guid entityId) => throw new NotImplementedException();

            public IEnumerable<IEntityA1> Find(IEnumerable<Guid> entityIds) => throw new NotImplementedException();

            public IEnumerable<IEntityA1> FindAll() => Enumerable.Empty<IEntityA1>();

            public void Save(IEnumerable<IEntityA1> items)
            {
            }

            public void Save(IEntityA1 entity)
            {
                throw new NotImplementedException();
            }
        }

        public class EntityA2 : Entity<EntityA2>, IEntityA2
        {
            public string Name { get; set; }
        }

        public class EntityA2Repository : IEntityA2Repository
        {
            public void Delete(IEnumerable<IEntityA2> items)
            {
            }

            public void Delete(IEntityA2 entity)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
            }

            public IEntityA2 Find(Guid entityId) => throw new NotImplementedException();

            public IEnumerable<IEntityA2> Find(IEnumerable<Guid> entityIds) => throw new NotImplementedException();

            public IEnumerable<IEntityA2> FindAll() => Enumerable.Empty<IEntityA2>();

            public IEntityA2 FindByName(string name) => new EntityA2 { Name = name };

            public void Save(IEnumerable<IEntityA2> items)
            {
            }

            public void Save(IEntityA2 entity)
            {
                throw new NotImplementedException();
            }
        }

        public class EntityA3 : EntityA2, IEntityA3
        {
            public string Description { get; set; }
        }

        public class EntityA3Repository : IEntityA3Repository
        {
            public void Delete(IEnumerable<IEntityA3> items)
            {
            }

            public void Delete(IEntityA3 entity)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
            }

            public IEntityA3 Find(Guid entityId) => throw new NotImplementedException();

            public IEnumerable<IEntityA3> Find(IEnumerable<Guid> entityIds) => throw new NotImplementedException();

            public IEnumerable<IEntityA3> FindAll() => Enumerable.Empty<IEntityA3>();

            public void Save(IEnumerable<IEntityA3> items)
            {
            }

            public void Save(IEntityA3 entity)
            {
                throw new NotImplementedException();
            }
        }

        public class ServiceA : IServiceA
        {
            public readonly IContainer _container;

            public ServiceA(IContainer container)
            {
                _container = container;
            }

            public IEnumerable<IEntityA1> FindAllEntityA1()
            {
                var repository = _container.Resolve<IEntityA1Repository>();
                return repository.FindAll();
            }

            public IEntityA2 FindEntityA2(string name)
            {
                var repository = _container.Resolve<IEntityA2Repository>();
                return repository.FindByName(name);
            }

            public void SaveEntityA3(IEntityA3 entity)
            {
                var repository = _container.Resolve<IEntityA2Repository>();
                repository.Save(new[] { entity });
            }
        }

        public class TestDataProviderA : TestDataProvider
        {
            public static readonly Type[] _supportedTypes = { typeof(EntityA1), typeof(EntityA2), typeof(EntityA3) };

            public TestDataProviderA()
            {
                Initialize(_supportedTypes);
            }

            protected override object CreateTestObject(Type type)
            {
                object testData;

                if (type.CanBeCastAs<IEntityA1>())
                {
                    testData = new EntityA1
                    {
                        Name = BetterRandom.NextString(50, 100, BetterRandom.CharacterGroup.Alpha)
                    };
                }
                else if (type.CanBeCastAs<IEntityA3>())
                {
                    testData = new EntityA3
                    {
                        Name = BetterRandom.NextString(50, 100, BetterRandom.CharacterGroup.Alpha),
                        Description = BetterRandom.NextString(50, 100, BetterRandom.CharacterGroup.Alpha)
                    };
                }
                else if (type.CanBeCastAs<IEntityA2>())
                {
                    testData = new EntityA2
                    {
                        Name = BetterRandom.NextString(50, 100, BetterRandom.CharacterGroup.Alpha)
                    };
                }
                else
                {
                    throw new ArgumentException($"{type.FullName} is not supported", nameof(type));
                }

                return testData;
            }
        }

        public class TestHelperA : TestHelperA<object, object>
        {
        }

        public class TestHelperA<TInterface, TImplementation> : TestHelper<TInterface, TImplementation> where TInterface : class where TImplementation : class, TInterface
        {
            public TestHelperA() : base(DefaultContainer.CreateEmptyContainer(), new TestDataProviderA())
            {
                Container.ParentContainer.Register<ContainerConfig>();
                if (typeof(TInterface) != typeof(IEntityA1Repository))
                {
                    SetupMockEntityA1Repository();
                }

                if (typeof(TInterface) != typeof(IEntityA2Repository))
                {
                    SetupMockEntityA2Repository();
                }

                if (typeof(TInterface) != typeof(IEntityA3Repository))
                {
                    SetupMockEntityA3Repository();
                }

                if (typeof(TInterface) != typeof(IServiceA))
                {
                    SetupMockServiceA();
                }
            }

            public TestDataProviderA TestDomainEntities { get; }

            public void SetupMockEntityA1Repository()
            {
                MockRepository<IEntityA1, IEntityA1Repository>();
            }

            public void SetupMockEntityA2Repository()
            {
                var mockRepository = MockRepository<IEntityA2, IEntityA2Repository>();
                mockRepository.Setup(r => r.FindByName(It.IsAny<string>())).Returns<string>(name => TestData<IEntityA2>().Single(entity => entity.Name.Equals(name)));
            }

            public void SetupMockEntityA3Repository()
            {
                MockRepository<IEntityA3, IEntityA3Repository>();
            }

            public void SetupMockServiceA()
            {
                throw new NotImplementedException();
            }
        }

        public interface IEntityA1 : IEntity
        {
            string Name { get; set; }
        }

        public interface IEntityA1Repository : IRepository<IEntityA1>
        {
        }

        public interface IEntityA2 : IEntity
        {
            string Name { get; set; }
        }

        public interface IEntityA2Repository : IRepository<IEntityA2>
        {
            IEntityA2 FindByName(string name);
        }

        public interface IEntityA3 : IEntityA2
        {
            string Description { get; set; }
        }

        public interface IEntityA3Repository : IRepository<IEntityA3>
        {
        }

        public interface IServiceA
        {
            IEnumerable<IEntityA1> FindAllEntityA1();

            IEntityA2 FindEntityA2(string name);

            void SaveEntityA3(IEntityA3 entity);
        }

        #endregion
    }
}
