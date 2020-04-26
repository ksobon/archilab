using System;
using System.Globalization;
// ReSharper disable UnusedMember.Global

namespace archilab.Core
{
    /// <summary>
    /// Wrapper class for some basic DateTime operations.
    /// </summary>
    public class DateTimes
    {
        internal DateTimes()
        {
        }

        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Returns a DateTime from Unix Time.
        /// </summary>
        /// <param name="unixTime">Unix time is number of seconds since 1st January 1970</param>
        /// <returns>Date Time</returns>
        public static DateTime FromUnixTime(long unixTime)
        {
            return _epoch.AddSeconds(unixTime);
        }

        /// <summary>
        /// Parses a date time string into a DateTime object.
        /// </summary>
        /// <param name="dateTime">String representation of date and time to parse./</param>
        /// <param name="format">Format the the string is in. Ex. DDMMYY</param>
        /// <returns>Date Time</returns>
        public static DateTime ParseExact(string dateTime, string format)
        {
            var dt = DateTime.ParseExact(dateTime, format, CultureInfo.InvariantCulture);
            return dt;
        }

        /// <summary>
        /// Converts a Date Time object into number of total milliseconds that passed since epoch.
        /// </summary>
        /// <param name="dateTime">Date Time to process.</param>
        /// <returns>Number of Milliseconds that passed since Epoch (1970/1/1)</returns>
        public static long TotalMilliseconds(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(_epoch).TotalMilliseconds;
        }

        /// <summary>
        /// Converts a Date Time object into number of total seconds that passed since epoch.
        /// </summary>
        /// <param name="dateTime">Date Time to process.</param>
        /// <returns>Number of Seconds that passed since Epoch (1970/1/1)</returns>
        public static long TotalSeconds(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(_epoch).TotalSeconds;
        }

        /// <summary>
        /// Converts a Date Time object into number of total minutes that passed since epoch.
        /// </summary>
        /// <param name="dateTime">Date Time to process.</param>
        /// <returns>Number of Minutes that passed since Epoch (1970/1/1)</returns>
        public static long TotalMinutes(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(_epoch).TotalMinutes;
        }

        /// <summary>
        /// Converts a Date Time object into number of total hours that passed since epoch.
        /// </summary>
        /// <param name="dateTime">Date Time to process.</param>
        /// <returns>Number of Hours that passed since Epoch (1970/1/1)</returns>
        public static long TotalHours(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(_epoch).TotalHours;
        }

        /// <summary>
        /// Converts a Date Time object into number of total Days that passed since epoch.
        /// </summary>
        /// <param name="dateTime">Date Time to process.</param>
        /// <returns>Number of Days that passed since Epoch (1970/1/1)</returns>
        public static long TotalDays(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(_epoch).TotalDays;
        }
    }
}
