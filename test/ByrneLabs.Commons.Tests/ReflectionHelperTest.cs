using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ByrneLabs.Commons.Tests
{
    public class ReflectionHelperTest
    {

        private class ClassA : IInterfaceA
        {

        }

        private class ClassB : ClassA, IInterfaceB
        {

        }

        private class ClassC : IInterfaceC
        {

        }

        private interface IInterfaceA
        {

        }

        private interface IInterfaceB
        {

        }

        private interface IInterfaceC : IInterfaceB
        {

        }

        [Fact]
        public void TestCanBeCastAs()
        {
            Assert.True(typeof(ClassA).CanBeCastAs<ClassA>());
            Assert.False(typeof(ClassA).CanBeCastAs<ClassB>());
            Assert.False(typeof(ClassA).CanBeCastAs<ClassC>());
            Assert.True(typeof(ClassA).CanBeCastAs<IInterfaceA>());
            Assert.False(typeof(ClassA).CanBeCastAs<IInterfaceB>());
            Assert.False(typeof(ClassA).CanBeCastAs<IInterfaceC>());

            Assert.True(typeof(ClassB).CanBeCastAs<ClassA>());
            Assert.True(typeof(ClassB).CanBeCastAs<ClassB>());
            Assert.False(typeof(ClassB).CanBeCastAs<ClassC>());
            Assert.True(typeof(ClassB).CanBeCastAs<IInterfaceA>());
            Assert.True(typeof(ClassB).CanBeCastAs<IInterfaceB>());
            Assert.False(typeof(ClassA).CanBeCastAs<IInterfaceC>());

            Assert.False(typeof(ClassC).CanBeCastAs<ClassA>());
            Assert.False(typeof(ClassC).CanBeCastAs<ClassB>());
            Assert.True(typeof(ClassC).CanBeCastAs<ClassC>());
            Assert.False(typeof(ClassC).CanBeCastAs<IInterfaceA>());
            Assert.True(typeof(ClassC).CanBeCastAs<IInterfaceB>());
            Assert.True(typeof(ClassC).CanBeCastAs<IInterfaceC>());

            Assert.False(typeof(IInterfaceA).CanBeCastAs<ClassA>());
            Assert.False(typeof(IInterfaceA).CanBeCastAs<ClassB>());
            Assert.False(typeof(IInterfaceA).CanBeCastAs<ClassC>());
            Assert.True(typeof(IInterfaceA).CanBeCastAs<IInterfaceA>());
            Assert.False(typeof(IInterfaceA).CanBeCastAs<IInterfaceB>());
            Assert.False(typeof(IInterfaceA).CanBeCastAs<IInterfaceC>());

            Assert.False(typeof(IInterfaceB).CanBeCastAs<ClassA>());
            Assert.False(typeof(IInterfaceB).CanBeCastAs<ClassB>());
            Assert.False(typeof(IInterfaceB).CanBeCastAs<ClassC>());
            Assert.False(typeof(IInterfaceB).CanBeCastAs<IInterfaceA>());
            Assert.True(typeof(IInterfaceB).CanBeCastAs<IInterfaceB>());
            Assert.False(typeof(IInterfaceB).CanBeCastAs<IInterfaceC>());

            Assert.False(typeof(IInterfaceC).CanBeCastAs<ClassA>());
            Assert.False(typeof(IInterfaceC).CanBeCastAs<ClassB>());
            Assert.False(typeof(IInterfaceC).CanBeCastAs<ClassC>());
            Assert.False(typeof(IInterfaceC).CanBeCastAs<IInterfaceA>());
            Assert.True(typeof(IInterfaceC).CanBeCastAs<IInterfaceB>());
            Assert.True(typeof(IInterfaceC).CanBeCastAs<IInterfaceC>());
        }
    }
}
