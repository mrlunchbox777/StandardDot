using System;
using System.Collections.Generic;
using System.Linq;
using StandardDot.Enums;
using Xunit;

namespace StandardDot.CoreExtensions.UnitTests
{
    public class ListExtensionsTests
    {
        [Fact]
        public void MoveItemToFirstTest()
        {
            List<LogLevel> logLevels = new List<LogLevel>
            {
                LogLevel.Debug,
                LogLevel.Error,
                LogLevel.Info,
                LogLevel.Warning
            };
            Assert.Equal(LogLevel.Debug, logLevels[0]);
            Assert.Equal(LogLevel.Error, logLevels[1]);
            Assert.Equal(LogLevel.Info, logLevels[2]);
            Assert.Equal(LogLevel.Warning, logLevels[3]);
            logLevels.MoveItemToFirst(x => x == LogLevel.Warning);
            Assert.Equal(LogLevel.Warning, logLevels[0]);
            Assert.Equal(LogLevel.Debug, logLevels[1]);
            Assert.Equal(LogLevel.Error, logLevels[2]);
            Assert.Equal(LogLevel.Info, logLevels[3]);
        }
    }
}