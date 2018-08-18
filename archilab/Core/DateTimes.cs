using System;
using System.Globalization;

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

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Returns a DateTime from Unix Time.
        /// </summary>
        /// <param name="unixTime">Unix time is number of seconds since 1st January 1970</param>
        /// <returns>Date Time</returns>
        public static DateTime FromUnixTime(long unixTime)
        {
            return Epoch.AddSeconds(unixTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static DateTime ParseExact(string dateTime, string format)
        {
            var dt = DateTime.ParseExact(dateTime, format, CultureInfo.InvariantCulture);
            return dt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long TotalMilliseconds(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(Epoch).TotalMilliseconds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long TotalSeconds(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(Epoch).TotalSeconds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long TotalMinutes(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(Epoch).TotalMinutes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long TotalHours(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(Epoch).TotalHours;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long TotalDays(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(Epoch).TotalDays;
        }
    }
}
