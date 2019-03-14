using System;
using System.Collections.Generic;
using ByrneLabs.Commons.Domain;
using Xunit;

namespace ByrneLabs.Commons.Domain.Tests
{
    public class EntityTests
    {
        private class Child : Entity<Child>
        {
            public string Name { get; set; }

            public Parent Parent { get; set; }
        }

        private class Parent : Entity<Parent>
        {
            public IList<Child> Children { get; } = new List<Child>();

            public string Name { get; set; }
        }

        private void AssertValidEntityClone(Entity original, Entity cloned)
        {
            Assert.NotSame(original, cloned);
            Assert.Equal(original, cloned);
            Assert.Equal(original.EntityId, cloned.EntityId);
        }

        [Fact]
        public void TestClone()
        {
            var parent = new Parent();
            parent.Name = "parent name 1";
            parent.EntityId = Guid.NewGuid();

            var child1 = new Child();
            child1.Parent = parent;
            child1.Name = "child name 1";
            child1.EntityId = Guid.NewGuid();
            parent.Children.Add(child1);

            var child2 = new Child();
            child2.Parent = parent;
            child2.Name = "child name 2";
            child2.EntityId = Guid.NewGuid();
            parent.Children.Add(child2);

            var clonedParent = parent.Clone();

            AssertValidEntityClone(parent, clonedParent);
            Assert.Equal(parent.Name, clonedParent.Name);
            Assert.Equal(parent.Children.Count, clonedParent.Children.Count);
            Assert.Equal(2, clonedParent.Children.Count);

            AssertValidEntityClone(parent.Children[0], clonedParent.Children[0]);
            Assert.Equal(parent.Children[0].Name, clonedParent.Children[0].Name);

            AssertValidEntityClone(parent.Children[1], clonedParent.Children[1]);
            Assert.Equal(parent.Children[1].Name, clonedParent.Children[1].Name);
        }
    }
}
