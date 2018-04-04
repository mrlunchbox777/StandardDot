using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using StandardDot.CoreExtensions.Object;
using Xunit;

namespace StandardDot.CoreExtensions.UnitTests.Object
{
  public class ObjectSerializationExtensionsTest
    {
        [Fact]
        public void SerializeJson()
        {
            string originalString = "{\"Foo\":4, \"Bar\":3}";
            Foobar original = new Foobar
                {
                    Foo = 4
                };
            
            Assert.NotNull(original.SerializeJson<Foobar>());
            Assert.NotEqual(originalString, original.SerializeJson<Foobar>());
        }
    }
}