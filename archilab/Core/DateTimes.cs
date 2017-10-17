using System;

namespace archilab.Core
{
    /// <summary>
    /// Wrapper class for some basic date time operations.
    /// </summary>
    public class DateTimes
    {
        internal DateTimes()
        {
        }

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Returns a DateTime from Unix Time.
        /// </summary>
        /// <param name="unixTime">Unix time is number of seconds since 1st January 1970</param>
        /// <returns>Date Time</returns>
        public static DateTime FromUnixTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }
    }
}
