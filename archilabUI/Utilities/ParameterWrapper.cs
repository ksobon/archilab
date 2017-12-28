
namespace archilabUI.Utilities
{
    public class ParameterWrapper
    {
        public string Name { get; set; }
        public string BipName { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as ParameterWrapper;

            return item != null && BipName.Equals(item.BipName);
        }

        public override int GetHashCode()
        {
            return BipName.GetHashCode();
        }
    }
}
