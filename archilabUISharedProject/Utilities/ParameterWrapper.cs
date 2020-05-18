namespace archilabUI.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class ParameterWrapper
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string BipName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is ParameterWrapper item && BipName.Equals(item.BipName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return BipName.GetHashCode();
        }
    }
}
