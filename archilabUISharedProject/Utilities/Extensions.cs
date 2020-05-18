using System;
using System.Collections.Generic;

namespace archilabUI.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public static class Extensions
    {
        #region IEnumerable.ForEach

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        #endregion
    }
}
