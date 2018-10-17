using System.Collections.Generic;
using StandardDot.CoreExtensions.Object;
using StandardDot.CoreExtensions.Object.DeepClone;
using Xunit;

namespace CoreExtensionsUnitTests.Object.DeepClone
{
	public class CopyOverrideSettingsTest
	{
		[Fact]
		public void CanCreate()
		{
			int initialA = 10;
			int overrideA = 15;
			CopyOverrideSettings<int> settings = new CopyOverrideSettings<int>(typeof(CopyableNumbersWithSettings)
				, "_a", overrideA, false, typeof(CopyableNumbersWithSettings), true);
			
			CopyableNumbersWithSettings numbers = new CopyableNumbersWithSettings(initialA, 0, settings);
			Assert.Equal(initialA, numbers.TheA);
			CopyableNumbersWithSettings numbers2 = numbers.Copy(settings);
			Assert.NotEqual(numbers, numbers2);
			Assert.Equal(numbers.TheB, numbers2.TheB);
			Assert.NotEqual(numbers.Settings, numbers2.Settings);
			Assert.Equal(initialA, numbers.TheA);
			Assert.Equal(overrideA, numbers2.TheA);
		}

		internal class CopyableNumbersWithSettings : Copyable
		{
			private int _a;
			private float _b;
			private ICopyOverrideSettings _settings;

			public CopyableNumbersWithSettings(int a, float b, ICopyOverrideSettings settings)
				: base(a, b, settings)
			{
				_a = a;
				_b = b;
				_settings = settings;
			}

			public int TheA { get { return _a; } }
			public float TheB { get { return _b; } }
			public ICopyOverrideSettings Settings => _settings;
		}
	}
}