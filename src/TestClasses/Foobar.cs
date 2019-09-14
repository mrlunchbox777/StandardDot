using System.Runtime.Serialization;

namespace StandardDot.TestClasses
{
	[DataContract]
	public class Foobar
	{
		[DataMember]
		public int Foo { get; set; }

		[IgnoreDataMember]
		public int Bar { get; set; }
	}
}