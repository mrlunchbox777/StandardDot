using System.Collections.Generic;
using System.Linq;
using StandardDot.TestClasses;
using Xunit;

namespace StandardDot.CoreExtensions.UnitTests
{
	public class IEnumerableExtensionsTests
	{
		[Fact]
		public void DistinctByTest()
		{
			Foobar foobar = new Foobar
			{
				Foo = 2,
				Bar = 4
			};
			Foobar foobar2 = new Foobar
			{
				Foo = 1,
				Bar = 3
			};

			List<Foobar> foobars = new List<Foobar>
			{
				foobar,
				foobar2
			};
			List<Foobar> foobars2 = new List<Foobar>
			{
				foobar,
				foobar,
				foobar2,
				foobar2
			};

			Assert.NotEqual(foobars.Count, foobars2.Count);
			List<Foobar> result = foobars2.DistinctBy(x => x.Foo + "" + x.Bar).ToList();
			Assert.NotEqual(foobars2.Count, result.Count);
			Assert.Equal(foobars.Count, result.Count);
			for (int i = 0; i < foobars.Count; i++)
			{
				Assert.Equal(foobars[i], result[i]);
			}
		}
		
		[Fact]
		public void DistinctTest()
		{
			List<int> source = new List<int>
			{
				2,
				4
			};
			List<int> source2 = new List<int>
			{
				1,
				3
			};

			List<int> sources = new List<int>();
			sources.AddRange(source);
			sources.AddRange(source2);

			List<int> sources2 = new List<int>();
			sources2.AddRange(source);
			sources2.AddRange(source);
			sources2.AddRange(source2);
			sources2.AddRange(source2);

			Assert.NotEqual(sources.Count, sources2.Count);
			List<int> result = sources2.DistinctBy(x => x).ToList();
			Assert.NotEqual(sources2.Count, result.Count);
			Assert.Equal(sources.Count, result.Count);
			for (int i = 0; i < sources.Count; i++)
			{
				Assert.Equal(sources[i], result[i]);
			}
		}
	}
}