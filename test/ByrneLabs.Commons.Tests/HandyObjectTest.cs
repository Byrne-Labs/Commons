using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace ByrneLabs.Commons.Tests
{
    public class HandyObjectTests
    {
        private class Child : HandyObject<Child>
        {
            public string Name { get; set; }

            public Parent Parent { get; set; }
        }

        private sealed class Daughter : Child, ICloneable<Daughter>
        {
            public new Daughter Clone(CloneDepth depth = CloneDepth.Deep) => (Daughter) base.Clone(depth);
        }

        private sealed class Parent : HandyObject<Parent>
        {
            public IList<Child> Children { get; } = new List<Child>();

            public string Name { get; set; }
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
        private static void AssertValidObjectClone(HandyObject original, HandyObject cloned)
        {
            Assert.NotSame(original, cloned);
            Assert.True(new HandyObjectReflectionEquivalencyComparer().Equals(original, cloned));
        }

        [Fact]
        public void TestClone()
        {
            var parent = new Parent
            {
                Name = "parent name 1"
            };

            var child1 = new Child
            {
                Parent = parent,
                Name = "child name 1"
            };
            parent.Children.Add(child1);

            var child2 = new Daughter
            {
                Parent = parent,
                Name = "child name 2"
            };
            parent.Children.Add(child2);

            var clonedParent = parent.Clone();

            AssertValidObjectClone(parent, clonedParent);
            Assert.Equal(parent.Name, clonedParent.Name);
            Assert.Equal(parent.Children.Count, clonedParent.Children.Count);
            Assert.Equal(2, clonedParent.Children.Count);

            AssertValidObjectClone(parent.Children[0], clonedParent.Children[0]);
            Assert.Equal(parent.Children[0].Name, clonedParent.Children[0].Name);

            AssertValidObjectClone(parent.Children[1], clonedParent.Children[1]);
            Assert.Equal(parent.Children[1].Name, clonedParent.Children[1].Name);
            Assert.IsType<Daughter>(parent.Children[1]);
        }

        [Fact]
        public void TestCloneInto()
        {
            var child = new Child
            {
                Name = "Child Name",
                Parent = new Parent
                {
                    Name = "Parent Name"
                }
            };
            child.Parent.Children.Add(child);

            var daughterClone = DeepCloner.CloneInto<Daughter, Child>(child);

            Assert.Equal(child.Name, daughterClone.Name);
            Assert.NotSame(child.Parent, daughterClone.Parent);
            Assert.Equal(daughterClone, daughterClone.Parent.Children.Single());
        }
    }
}
