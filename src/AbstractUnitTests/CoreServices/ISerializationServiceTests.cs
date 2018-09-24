using System.IO;
using Moq;
using StandardDot.Abstract.CoreServices;
using StandardDot.TestClasses;
using Xunit;

namespace StandardDot.Abstract.UnitTests.CoreServices
{
	public class ISerializationServiceTests
	{
		[Fact]
		public void SerializeObjectTest()
		{
			string expected = "expected";
			Foobar foobar = new Foobar { Foo = 4, Bar = 9 };
			Mock<ISerializationService> serializationServiceProxy = new Mock<ISerializationService>(MockBehavior.Strict);
			serializationServiceProxy.Setup(x => x.SerializeObject(foobar, null)).Returns(expected);
			Assert.Equal(expected, serializationServiceProxy.Object.SerializeObject(foobar));
		}

		[Fact]
		public void DeserializeObjectTest()
		{
			string expected = "expected";
			Foobar foobar = new Foobar { Foo = 4, Bar = 9 };
			Mock<ISerializationService> serializationServiceProxy = new Mock<ISerializationService>(MockBehavior.Strict);
			serializationServiceProxy.Setup(x => x.DeserializeObject<Foobar>(expected, null)).Returns(foobar);
			Assert.Equal(foobar, serializationServiceProxy.Object.DeserializeObject<Foobar>(expected));
		}

		[Fact]
		public void DeserializeObjectStreamTest()
		{
			using (Stream expected = new MemoryStream())
			{
				Foobar foobar = new Foobar { Foo = 4, Bar = 9 };
				Mock<ISerializationService> serializationServiceProxy = new Mock<ISerializationService>(MockBehavior.Strict);
				serializationServiceProxy.Setup(x => x.DeserializeObject<Foobar>(expected, null)).Returns(foobar);
				Assert.Equal(foobar, serializationServiceProxy.Object.DeserializeObject<Foobar>(expected));
			}
		}
	}
}