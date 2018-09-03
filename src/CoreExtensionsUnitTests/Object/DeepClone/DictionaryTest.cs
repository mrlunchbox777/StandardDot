using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Assert.NotSame(copy, dict);
            Assert.NotSame(copy[0], dict[0]);
            Assert.NotEqual(copy[0], dict[0]); 
            Assert.Equal(copy[0].Foo, dict[0].Foo); 
        }
        
        [Fact]
        public void ReferenceTypeDictionaryIsCopied()
        {
            Dictionary<int, FooBar> dict = new Dictionary<int, FooBar>();
            dict.Add(0, new FooBar(3));
            var stuffb1 = dict[0];

            Dictionary<int, FooBar> copy = dict.Copy();
            var stuff = copy[0];
            var stuff1 = dict[0];
            Assert.NotSame(copy, dict);
            Assert.NotEqual(copy.Single(), dict.Single());
            Assert.NotEqual(copy.Single(), dict.Single()); 
            // these should be equal because it's a copy (and the key is a value type)
            Assert.Equal(copy.Single().Key, dict.Single().Key);
            Assert.Equal(copy.Single().Key, dict.Single().Key); 
            Assert.NotSame(copy.Single().Value, dict.Single().Value);
            Assert.NotEqual(copy.Single().Value, dict.Single().Value); 
            Assert.Equal(copy.Single().Value.Foo, dict.Single().Value.Foo); 
        }
    }
}