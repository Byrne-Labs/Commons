using Xunit;

namespace ByrneLabs.Commons.Ioc.DotNetCore.Tests
{
    [Trait("Category", "Unit Test")]
    public class ContainerTest
    {
        [Fact]
        public void TestRegisterSingleton()
        {
            var container = new DotNetCoreContainerProvider(false);

            container.RegisterType<object, object>(ObjectLifetime.PerContainer);

            var firstReference = container.Resolve<object>();
            var secondReference = container.Resolve<object>();

            Assert.NotNull(firstReference);
            Assert.Same(firstReference, secondReference);
        }

        [Fact]
        public void TestRegisterTransient()
        {
            var container = new DotNetCoreContainerProvider(false);

            container.RegisterType<object, object>();

            var firstReference = container.Resolve<object>();
            var secondReference = container.Resolve<object>();

            Assert.NotNull(firstReference);
            Assert.NotNull(secondReference);
            Assert.NotSame(firstReference, secondReference);
        }
    }
}
