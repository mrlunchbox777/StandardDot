using System.IO;
using System.Text;
using StandardDot.CoreServices.Serialization;
using StandardDot.TestClasses;
using Xunit;

namespace StandardDot.CoreServices.UnitTests
{
    public class JsonTests
    {
        [Fact]
        public void DeserializeJsonString()
        {
            Json service = new Json();
            string originalString = "{\"Foo\":4, \"Bar\":3}";
            Foobar original = new Foobar
                {
                    Foo = 4
                };
            
            Foobar deserialized = service.DeserializeObject<Foobar>(originalString);
            Assert.NotNull(deserialized);
            Assert.NotEqual(original, deserialized);
            Assert.Equal(original.Foo, deserialized.Foo);
            Assert.Equal(original.Bar, deserialized.Bar);
        }

        [Fact]
        public void DeserializeJsonStream()
        {
            Json service = new Json();
            string originalString = "{\"Foo\":4, \"Bar\":3}";
            Foobar original = new Foobar
                {
                    Foo = 4
                };
            
            Foobar deserialized;
            byte[] bytes = Encoding.UTF8.GetBytes(originalString);
            using (Stream currentStream = new MemoryStream(bytes))
            {
                deserialized = service.DeserializeObject<Foobar>(currentStream);
            }
            Assert.NotNull(deserialized);
            Assert.NotEqual(original, deserialized);
            Assert.Equal(original.Foo, deserialized.Foo);
            Assert.Equal(original.Bar, deserialized.Bar);
        }

        [Fact]
        public void SerializeJson()
        {
            Json service = new Json();
            string originalString = "{\"Foo\":4}";
            Foobar original = new Foobar
                {
                    Foo = 4
                };
            
            string serailizedObject = service.SerializeObject(original);
            Assert.NotNull(serailizedObject);
            Assert.Equal(originalString, serailizedObject);
        }
    }
}