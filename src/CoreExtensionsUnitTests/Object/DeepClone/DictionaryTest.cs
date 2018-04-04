using System;
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
            Dictionary<int, Exception> dict = new Dictionary<int, Exception>();
            dict.Add(0, new Exception("Test"));

            Dictionary<int, Exception> copy = (Dictionary<int, Exception>) dict.Copy();
            Assert.NotSame(copy[0], dict[0]);
            Assert.Equal(copy[0].Message, dict[0].Message); 
        }
    }
}
