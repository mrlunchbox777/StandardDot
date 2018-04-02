using System.Runtime.Serialization;

namespace shoellibraries.CoreExtensions.UnitTests
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