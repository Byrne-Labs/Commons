using System;
using System.Collections.Generic;
using System.Linq;
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

        private class Daughter : Child, IEntity<Daughter>
        {
            public new Daughter Clone(CloneDepth depth = CloneDepth.Deep) => (Daughter)base.Clone(depth);
        }

        private class Parent : Entity<Parent>
        {
            public IList<Child> Children { get; } = new List<Child>();

            public string Name { get; set; }
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

            var child2 = new Daughter();
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
            Assert.IsType<Daughter>(parent.Children[1]);
        }

        [Fact]
        public void TestCloneInto()
        {
            var child = new Child();
            child.Name = "Child Name";
            child.Parent = new Parent();
            child.Parent.Name = "Parent Name";
            child.Parent.Children.Add(child);

            var daughterClone = DeepCloner.CloneInto<Daughter, Child>(child);

            Assert.Equal(child.Name, daughterClone.Name);
            Assert.NotSame(child.Parent, daughterClone.Parent);
            Assert.Equal(daughterClone, daughterClone.Parent.Children.Single());
        }

        private void AssertValidEntityClone(Entity original, Entity cloned)
        {
            Assert.NotSame(original, cloned);
            Assert.Equal(original, cloned);
            Assert.Equal(original.EntityId, cloned.EntityId);
        }
    }
}
