using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using StandardDot.CoreExtensions.Object;
using Xunit;

namespace StandardDot.CoreExtensions.UnitTests
{
  public class StreamExtensionsTest
    {
        [Fact]
        public void GetStringFromStream()
        {
            string original = "foobar";
            Stream testStream = original.ToStream();
            Assert.Equal(original, testStream.GetString());
        }

        [Fact]
        public void GetByteArrayFromStream()
        {
            string original = "foobar";
            byte[] originalByteArray = Encoding.UTF8.GetBytes(original);
            Stream testStream = original.ToStream();
            Assert.Equal(originalByteArray, testStream.ToByteArray());
        }
    }
}