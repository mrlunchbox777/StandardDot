using System;
using System.Collections.Generic;

namespace StandardDot.Constants
{
    public class DateTime
    {
        public static System.DateTime UnixEpoch { get; } = new System.DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
    }
}