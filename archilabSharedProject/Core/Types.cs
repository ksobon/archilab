using System;

namespace archilab.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class Types
    {
        internal Types()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetAssemblyQualifiedName(Type type)
        {
            return type.AssemblyQualifiedName;
        }
    }
}
