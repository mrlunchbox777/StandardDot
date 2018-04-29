using System;
using System.Collections.Generic;
using StandardDot.CoreExtensions.Object;
using StandardDot.TestClasses;
using Xunit;

namespace StandardDot.CoreExtensions.UnitTests.Object.DeepClone
{
    public class ObjectHierarchyTest
    {
        [Fact]
        public void TestObjectHierarchyClone()
        {
            Node n1 = new Node();
            n1.Name = "Node 1";
            Node n2 = new Node();
            n2.Name = "Node 2";
            
            n1.Next = n2;
            Node n1c = (Node)n1.Copy();

            Assert.NotSame(n1, n1c);
            Assert.NotSame(n2, n1c.Next);
            Assert.NotNull(n1c.Next);
            Assert.Equal(n1.Name, n1c.Name);
            Assert.Equal(n2.Name, n1c.Next.Name);
        }

        [Fact]
        public void CyclicObjectIsCopiedWithSemanticsIntact()
        {
            Node n1 = new Node();
            n1.Name = "Node 1";
            Node n2 = new Node();
            n2.Name = "Node 2";

            n1.Next = n2;
            n2.Prev = n1;
            Node n1c = (Node)n1.Copy();

            Assert.NotSame(n1, n1c);
            Assert.NotSame(n2, n1c.Next);
            Assert.NotNull(n1c.Next);
            Assert.Equal(n1.Name, n1c.Name);
            Assert.Equal(n2.Name, n1c.Next.Name);
            Assert.Same(n1c, n1c.Next.Prev);
        }

        [Fact]
        public void HumanHierarchyIsCloned()
        {
            Human father = new Human();
            father.Gender = Gender.Male;
            father.Name = "Dad";
            father.Children = new List<Human>();

            Human son = new Human();
            son.Gender = Gender.Male;
            son.Name = "Sonny";

            father.Children.Add(son);

            // Crazy science
            Human sensation = (Human)father.Copy();

            Assert.NotSame(father, sensation);
            Assert.NotSame(father.Children, sensation.Children);
            Assert.NotNull(sensation.Children);
            Assert.Single(sensation.Children);
            Assert.NotSame(father.Children[0], sensation.Children[0]);
            Assert.Equal(father.Name, sensation.Name);
            Assert.Equal(father.Children[0].Name, sensation.Children[0].Name);
        }
    }
}