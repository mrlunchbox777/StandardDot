using StandardDot.CoreExtensions.Object;
using StandardDot.CoreExtensions.Object.DeepClone;
using System;
using Xunit;

namespace StandardDot.CoreExtensions.UnitTests.Object.DeepClone
{
	public class FlatObjectTest
	{
		[Fact]
		public void CopyableFlatObjectIsCopied()
		{
			CopyableNumbers n = new CopyableNumbers(3, 4.0f);
			CopyableNumbers c = (CopyableNumbers)n.Copy();
			Assert.NotSame(n, c);
			Assert.Equal(n.TheA, c.TheA);
			Assert.Equal(n.TheB, c.TheB);
		}

		[Fact]
		public void RegularObjectWithoutDefaultConstructorIsCopied()
		{
			JustNumbers n = new JustNumbers(3, 4.0f);
			JustNumbers c = (JustNumbers)n.Copy(new JustNumbers(0, 0));
			Assert.NotSame(n, c);
			Assert.Equal(n.TheA, c.TheA);
			Assert.Equal(n.TheB, c.TheB);
		}

		[Fact]
		public void RegularObjectWithDefaultConstructorIsCopied()
		{
			JustNumbersDefault n = new JustNumbersDefault();
			n.TheA = 3;
			n.TheB = 4.0f;
			JustNumbersDefault c = (JustNumbersDefault)n.Copy(new JustNumbersDefault());
			Assert.NotSame(n, c);
			Assert.Equal(n.TheA, c.TheA);
			Assert.Equal(n.TheB, c.TheB);
		}
	}

	internal class JustNumbersDefault
	{
		private int _a;
		private float _b;

		public JustNumbersDefault()
		{
			_a = 0;
			_b = 0.0f;
		}

		public int TheA { get { return _a; } set { _a = value; } }
		public float TheB { get { return _b; } set { _b = value; } }
	}

	internal class JustNumbers
	{
		private int _a;
		private float _b;

		public JustNumbers(int a, float b)
		{
			_a = a;
			_b = b;
		}

		public int TheA { get { return _a; } }
		public float TheB { get { return _b; } }
	}

	internal class CopyableNumbers : Copyable
	{
		private int _a;
		private float _b;

		public CopyableNumbers(int a, float b)
			: base(a, b)
		{
			_a = a;
			_b = b;
		}

		public int TheA { get { return _a; } }
		public float TheB { get { return _b; } }
	}
}
