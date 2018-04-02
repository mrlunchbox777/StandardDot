using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using shoellibraries.CoreExtensions.Object;
using Xunit;

namespace shoellibraries.CoreExtensions.UnitTests
{
  public class StringExtensionsTest
    {
        [Fact]
        public void DeserializeJson()
        {
            string originalString = "{\"Foo\":4, \"Bar\":3}";
            Foobar original = new Foobar
                {
                    Foo = 4
                };
            
            Assert.NotNull(originalString.DeserializeJson<Foobar>());
            Assert.NotEqual(original, originalString.DeserializeJson<Foobar>());
            Assert.Equal(original.Foo, originalString.DeserializeJson<Foobar>().Foo);
            Assert.Equal(original.Bar, originalString.DeserializeJson<Foobar>().Bar);
        }

        [Fact]
        public void GetStreamFromStringWhiteSpace()
        {
            Assert.Equal(0, " ".GetStreamFromString().GetByteArrayFromStream().Length);
        }

        [Fact]
        public void GetStreamFromString()
        {
            string originalString = "foobar";
            Assert.Equal(originalString, originalString.GetStreamFromString().GetStringFromStream());
        }

        [Fact]
        public void GetBytes()
        {
            string originalString = "foobar";
            byte[] originalBytes = Encoding.UTF8.GetBytes(originalString);
            Assert.Equal(originalBytes, originalString.GetBytes());
        }

        [Fact]
        public void GetString()
        {
            string originalString = "foobar";
            byte[] originalBytes = Encoding.UTF8.GetBytes(originalString);
            Assert.Equal(originalString, originalBytes.GetString());
        }

        [Fact]
        public void GetArbitraryString()
        {
            byte[] originalBytes = {11, 32, 69};
            string originalString = Convert.ToBase64String(originalBytes);
            Assert.Equal(originalString, originalBytes.GetArbitraryString());
        }

        [Fact]
        public void Base64Decode()
        {
            string originalString = "foobar";
            string originalStringB64 = Convert.ToBase64String(originalString.GetBytes());
            Assert.Equal(originalString, originalStringB64.Base64Decode());
        }

        [Fact]
        public void Base64Encode()
        {
            string originalString = "foobar";
            string originalStringB64 = Convert.ToBase64String(originalString.GetBytes());
            Assert.Equal(originalStringB64, originalString.Base64Encode());
        }
    }
}