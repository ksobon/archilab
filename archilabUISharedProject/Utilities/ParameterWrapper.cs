namespace archilabUI.Utilities
{
    public class ParameterWrapper
    {
        public string Name { get; set; }
        public string BipName { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ParameterWrapper item && BipName.Equals(item.BipName);
        }

        public override int GetHashCode()
        {
            return BipName.GetHashCode();
        }
    }
}
