using System;
using System.Linq;
using Xunit;

namespace StandardDot.Enums.UnitTests
{
    public class HmacIsValidRequestResultTests
    {
        [Fact]
        public void BasicEnumVerification()
        {
            Array resultArray = Enum.GetValues(typeof(HmacIsValidRequestResult));

            Assert.NotNull(resultArray);

            HmacIsValidRequestResult[] allresults = resultArray.Cast<HmacIsValidRequestResult>()?.ToArray();

            Assert.NotNull(allresults);
            Assert.NotEmpty(allresults);
            Assert.Equal(9, allresults.Length);
            Assert.Equal(0, (int)HmacIsValidRequestResult.General);
            Assert.Equal(1, (int)HmacIsValidRequestResult.NoValidResouce);
            Assert.Equal(2, (int)HmacIsValidRequestResult.UnableToFindAppId);
            Assert.Equal(3, (int)HmacIsValidRequestResult.ReplayRequest);
            Assert.Equal(4, (int)HmacIsValidRequestResult.SignaturesMismatch);
            Assert.Equal(5, (int)HmacIsValidRequestResult.NoError);
            Assert.Equal(6, (int)HmacIsValidRequestResult.NoHmacHeader);
            Assert.Equal(7, (int)HmacIsValidRequestResult.NotEnoughHeaderParts);
            Assert.Equal(8, (int)HmacIsValidRequestResult.BadNamespace);
        }
    }
}