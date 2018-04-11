using System;
using System.Linq;
using StandardDot.Enums;
using Xunit;

namespace  StandardDot.Enums.UnitTests
{
    public class LogLevelTests
    {
        [Fact]
        public void BasicEnumVerification()
        {
            Array logLevelArray = Enum.GetValues(typeof(LogLevel));

            Assert.NotNull(logLevelArray);

            LogLevel[] allLogLevels = logLevelArray.Cast<LogLevel>()?.ToArray();

            Assert.NotNull(allLogLevels);
            Assert.NotEmpty(allLogLevels);
            Assert.Equal(4, allLogLevels.Length);
            Assert.Equal(0, (int)LogLevel.Debug);
            Assert.Equal(1, (int)LogLevel.Info);
            Assert.Equal(2, (int)LogLevel.Warning);
            Assert.Equal(3, (int)LogLevel.Error);
        }
    }
}
