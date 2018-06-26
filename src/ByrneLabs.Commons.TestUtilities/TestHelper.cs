using System;
using ByrneLabs.Commons.Ioc;
using Microsoft.Extensions.DependencyInjection;

namespace ByrneLabs.Commons.TestUtilities
{
    public abstract class TestHelper<TInterface, TImplementation> : TestDataAggregator, ITestHelper<TInterface> where TInterface : class where TImplementation : class, TInterface
    {
        private TInterface _testedObject;

        protected TestHelper(IContainer container, params ITestDataProvider[] domainEntityTestDomainEntities) : base(domainEntityTestDomainEntities)
        {
            if (typeof(TInterface) != typeof(object))
            {
                container.AddSingleton<TInterface, TImplementation>();
            }

            Container = container;
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

                return _testedObject ?? (_testedObject = Container.GetRequiredService<TInterface>());
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
    }
}
