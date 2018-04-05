using System;
using System.Collections.Generic;

namespace StandardDot.Constants
{
    /// <summary>
    /// DateTime Constants
    /// </summary>
    public class DateTime
    {
        
        /// <summary>
        /// The Unix Epoch (time since Jan 1st 1970 UTC)
        /// </summary>
        public static System.DateTime UnixEpoch { get; } = new System.DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
    }
}