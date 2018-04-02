using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using shoellibraries.CoreExtensions.Object;
using Xunit;

namespace shoellibraries.CoreExtensions.UnitTests
{
  public class StreamExtensionsTest
    {
        [Fact]
        public void GetStringFromStream()
        {
            string original = "foobar";
            Stream testStream = original.GetStreamFromString();
            Assert.Equal(original, testStream.GetStringFromStream());
        }

        [Fact]
        public void GetByteArrayFromStream()
        {
            string original = "foobar";
            byte[] originalByteArray = Encoding.UTF8.GetBytes(original);
            Stream testStream = original.GetStreamFromString();
            Assert.Equal(originalByteArray, testStream.GetByteArrayFromStream());
        }
    }
}