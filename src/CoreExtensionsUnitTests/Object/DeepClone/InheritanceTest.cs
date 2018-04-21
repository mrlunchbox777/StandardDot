using System;
using StandardDot.CoreExtensions.Object;
using StandardDot.TestClasses;
using Xunit;

namespace StandardDot.CoreExtensions.UnitTests.Object.DeepClone
{
    public class InheritanceTest
    {
        [Fact]
        public void InheritedFieldsAreCopied()
        {
            Bar bar = new Bar() { Value = 42 };

            Bar copy = (Bar)bar.Copy();

            Assert.Equal(42, copy.Value);
        }

        [Fact]
        public void HumanIsCopied()
        {
            Human human = new Human();
            Human copy = (Human)human.Copy();

            Assert.NotSame(human, copy);
        }
    }
}
