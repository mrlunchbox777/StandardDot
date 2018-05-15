using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using StandardDot.CoreExtensions.Object;
using Xunit;

namespace StandardDot.CoreExtensions.UnitTests.Object.DeepClone
{
  public class DictionaryTest
    {
        [Fact]
        public void DictionaryIsCopied()
        {
            Dictionary<int, FooBar> dict = new Dictionary<int, FooBar>();
            dict.Add(0, new FooBar(3));

            Dictionary<int, FooBar> copy = dict.Copy();
            Assert.NotSame(copy[0], dict[0]);
            Assert.Equal(copy[0], dict[0]); 
        }
    }
}
