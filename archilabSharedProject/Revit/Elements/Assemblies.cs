using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class Assemblies
    {
        internal Assemblies()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element AddMembers(Element assembly, List<Element> members)
        {
            if (!(assembly.InternalElement is Autodesk.Revit.DB.AssemblyInstance ai))
                throw new ArgumentNullException(nameof(assembly));
            if (!members.Any())
                throw new ArgumentNullException(nameof(members));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            ai.AddMemberIds(members.Select(x => x.InternalElement.Id).ToList());
            TransactionManager.Instance.TransactionTaskDone();

            return assembly;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static List<Element> Members(Element assembly)
        {
            if (!(assembly.InternalElement is Autodesk.Revit.DB.AssemblyInstance ai))
                throw new ArgumentNullException(nameof(assembly));

            var doc = DocumentManager.Instance.CurrentDBDocument;

            return ai.GetMemberIds().Select(x => doc.GetElement(x).ToDSType(true)).ToList();
        }
    }
}
