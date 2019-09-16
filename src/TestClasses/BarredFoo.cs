using System.Runtime.Serialization;

namespace StandardDot.TestClasses
{
	[DataContract]
	public class BarredFoo
	{
		[DataMember]
		public int Barred { get; set; }

		[IgnoreDataMember]
		public int Foo { get; set; }
	}
}