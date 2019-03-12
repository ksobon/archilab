using System.Reflection;
using Dynamo.Wpf.Extensions;

namespace archilab.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public class archlabExtension : IViewExtension
    {
        static archlabExtension()
        {
            Assembly.LoadFrom(
                @"C:\Users\ksobon\AppData\Roaming\Dynamo\Dynamo Revit\2.0\packages\archi-lab.net\bin\Xceed.Wpf.AvalonDock.dll");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Startup(ViewStartupParams p)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Loaded(ViewLoadedParams p)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void Shutdown()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        public string UniqueId
        {
            get { return "29ca1545-bc11-4008-911c-5e7555cf4f77"; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return "archilab"; }
        }
    }
}
