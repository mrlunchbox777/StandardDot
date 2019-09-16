using System;
using StandardDot.CoreExtensions.Object;
using StandardDot.CoreExtensions.Object.DeepClone;
using Xunit;

namespace StandardDot.CoreExtensions.UnitTests.Object.DeepClone
{
	public class InstanceProviderTest
	{
		[Fact]
		public void BrushIsCopiedWithInstanceProvider()
		{
			FooBarProvider.NumCalls = 0;

			FooBar a = new FooBar(4);
			FooBar b = (FooBar)a.Copy();

			Assert.NotSame(a, b);
			Assert.True(FooBarProvider.NumCalls > 0);
			Assert.Equal(a.Foo, b.Foo);
			Assert.Equal(4, a.Foo);
		}

		[Fact]
		public void BrushIsCopiedWithSuppliedInstance()
		{
			FooBar a = new FooBar(4);
			FooBar b = new FooBar(6);
			a.Copy(b);

			Assert.NotSame(a, b);
			Assert.Equal(a.Foo, b.Foo);
		}
	}

	internal class FooBar
	{
		public FooBar(int foo)
		{
			Foo = foo;
		}

		public int Foo { get; }
	}

	internal class FooBarProvider : InstanceProvider<FooBar>
	{
		public static int NumCalls { get; set; }

		public override FooBar CreateTypedCopy(FooBar toBeCopied)
		{
			NumCalls++;
			return new FooBar(toBeCopied.Foo);
		}
	}
}
