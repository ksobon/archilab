using System;
using System.Collections.Generic;
using System.Linq;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Revit.Elements.Views;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Views
{
    /// <summary>
    /// View Template related utilities.
    /// </summary>
    public class ViewTemplates
    {
        internal ViewTemplates()
        {
        }

        /// <summary>
        /// Creates a copy of View Template.
        /// </summary>
        /// <param name="viewTemplate">View Template to be duplicated.</param>
        /// <param name="name">Name of the new View Template.</param>
        /// <returns name="view">View Template</returns>
        public static View Duplicate(View viewTemplate, string name)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var v = (Autodesk.Revit.DB.View)viewTemplate.InternalElement;

            TransactionManager.Instance.EnsureInTransaction(doc);
            var newId = Autodesk.Revit.DB.ElementTransformUtils.CopyElement(doc, v.Id, Autodesk.Revit.DB.XYZ.Zero);
            var newView = (Autodesk.Revit.DB.View)doc.GetElement(newId.FirstOrDefault());
            newView.Name = name;
            TransactionManager.Instance.TransactionTaskDone();

            return newView.ToDSType(true) as View;
        }

        /// <summary>
        /// Retrieves Template Parameter Ids.
        /// </summary>
        /// <param name="viewTemplate">View to get the Parameter Ids from.</param>
        /// <returns>List of Parameter Ids.</returns>
        public static List<int> GetTemplateParameterIds(View viewTemplate)
        {
            var v = (Autodesk.Revit.DB.View)viewTemplate.InternalElement;
            if (!v.IsTemplate) throw new Exception("View has to be a View Template type.");

            return v.GetTemplateParameterIds().Select(x => x.IntegerValue).ToList();
        }

        /// <summary>
        /// Sets the View Template parameter's "include" property.
        /// </summary>
        /// <param name="viewTemplate">View to set the parameter for.</param>
        /// <param name="parameters">List of parameters to set.</param>
        /// <param name="include">True to include given parameters, otherwise false.</param>
        /// <returns>View Template.</returns>
        public static View Include(View viewTemplate, List<int> parameters, bool include = true)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var v = (Autodesk.Revit.DB.View)viewTemplate.InternalElement;
            if (!v.IsTemplate) throw new Exception("View has to be a View Template type.");

            var nonControlled = new HashSet<int>(v.GetNonControlledTemplateParameterIds().Select(x => x.IntegerValue));
            foreach (var p in parameters)
            {
                if(include) nonControlled.Remove(p);
                else nonControlled.Add(p);

            }

            TransactionManager.Instance.EnsureInTransaction(doc);
            v.SetNonControlledTemplateParameterIds(new List<Autodesk.Revit.DB.ElementId>(nonControlled.Select(x => new Autodesk.Revit.DB.ElementId(x))));
            TransactionManager.Instance.TransactionTaskDone();

            return v.ToDSType(true) as View;
        }

        /// <summary>
        /// Retrieves current status of the "include" parameter for a View Template.
        /// </summary>
        /// <param name="viewTemplate">View to retrieve the parameter status for.</param>
        /// <param name="parameter">Parameter to check.</param>
        /// <returns>True if parameter's "include" value is checked, otherwise false.</returns>
        public static bool IsIncluded(View viewTemplate, int parameter)
        {
            var v = (Autodesk.Revit.DB.View)viewTemplate.InternalElement;
            if (!v.IsTemplate) throw new Exception("View has to be a View Template type.");

            var nonControlled = new HashSet<int>(v.GetNonControlledTemplateParameterIds().Select(x => x.IntegerValue));
            return !nonControlled.Contains(parameter);
        }

        /// <summary>
        /// Retrieves a list of non-controlled template parameters. These are parameters
        /// that have the "include" parameter unchecked.
        /// </summary>
        /// <param name="viewTemplate">View to retrieve the parameters from.</param>
        /// <returns>List of non-controlled parameter ids.</returns>
        public static List<int> NonControlledTemplateParameters(View viewTemplate)
        {
            var v = (Autodesk.Revit.DB.View)viewTemplate.InternalElement;
            if (!v.IsTemplate) throw new Exception("View has to be a View Template type.");

            return v.GetNonControlledTemplateParameterIds().Select(x => x.IntegerValue).ToList();
        }

        /// <summary>
        /// Retrieves View Template parameter name from its Id. 
        /// </summary>
        /// <param name="parameter">View Template parameter.</param>
        /// <returns>Name of the View Template parameter.</returns>
        public static string GetParameterName(int parameter)
        {
            return Utilities.ViewTemplateParameters.GetName(parameter);
        }
    }
}
