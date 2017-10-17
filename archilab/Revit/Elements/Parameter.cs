using System.Linq;

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Parameter related utilities.
    /// </summary>
    public class Parameter
    {
        internal Parameter()
        {
        }

        /// <summary>
        /// Returns name of the BuiltInParameter if such exists. 
        /// </summary>
        /// <param name="element">Element to query.</param>
        /// <param name="name">Name of the parameter. In case that multiple parameters have the same name, first encountered name will be returned.</param>
        /// <returns name="bipName">Name of the Bip.</returns>
        public static string GetBuiltInParamterName(global::Revit.Elements.Element element, string name)
        {
            var e = element.InternalElement;
            var param = e.Parameters.Cast<Autodesk.Revit.DB.Parameter>().FirstOrDefault(x => x.Definition.Name == name);

            return ((Autodesk.Revit.DB.InternalDefinition)param?.Definition)?.BuiltInParameter.ToString();
        }
    }
}
