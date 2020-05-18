using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using CoreNodeModels;
using DSRevitNodesUI;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;

namespace archilabUI.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CustomGenericEnumerationDropDown : RevitDropDownBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="enumerationType"></param>
        protected CustomGenericEnumerationDropDown(string name, Type enumerationType) : base(name)
        {
            EnumerationType = enumerationType;
            PopulateItems();
        }

        /// <summary>
        /// 
        /// </summary>
        public Type EnumerationType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentSelection"></param>
        /// <returns></returns>
        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            if (EnumerationType == null) return SelectionState.Restore;

            Items.Clear();

            foreach (var name in Enum.GetNames(EnumerationType))
            {
                Items.Add(new DynamoDropDownItem(name, Enum.Parse(EnumerationType, name)));
            }

            Items = Items.OrderBy(x => x.Name).ToObservableCollection();
            SelectedIndex = 0;
            return SelectionState.Done;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 || Items.Count == -1)
            {
                PopulateItems();
            }

            var stringNode = AstFactory.BuildStringNode(Items[SelectedIndex].Name);

            var assign = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), stringNode);

            return new List<AssociativeNode> { assign };
        }
    }

    /// <summary>
    /// Generic UI Dropdown node base class for Revit Elements.
    /// This class populates a dropdown with all Revit elements of the specified type.
    /// It uses a filtered element collector filtering by class.
    /// </summary>
    public abstract class CustomRevitElementDropDown : RevitDropDownBase
    {
        /// <summary>
        /// Generic Revit Element Class Dropdown Node
        /// </summary>
        /// <param name="name">Name of the Node</param>
        /// <param name="elementType">Type of Revit Element to display</param>
        protected CustomRevitElementDropDown(string name, Type elementType) : base(name) { ElementType = elementType; PopulateDropDownItems(); }

        /// <summary>
        /// Type of Element
        /// </summary>
        private Type ElementType
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentSelection"></param>
        /// <returns></returns>
        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            PopulateDropDownItems();
            return SelectionState.Done;
        }
        /// <summary>
        /// Populate the Dropdown menu
        /// </summary>
        public void PopulateDropDownItems()
        {
            if (ElementType == null) return;

            // Clear the Items
            Items.Clear();

            // If the active doc is null, throw an exception
            if (DocumentManager.Instance.CurrentDBDocument == null)
            {
                throw new Exception("No active doc found.");
            }

            // Set up a new element collector using the Type field
            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument).OfClass(ElementType);

            // If there is nothing in the collector add the missing Type message to the Dropdown menu.
            if (fec.ToElements().Count == 0)
            {
                Items.Add(new DynamoDropDownItem("No types found", null));
                SelectedIndex = 0;
                return;
            }

            // if the element type is a RebarHookType add an initial "None" value which does not come from the collector
            if (ElementType.FullName == "Autodesk.Revit.DB.Structure.RebarHookType") Items.Add(new DynamoDropDownItem("none", null));

            // Walk through all elements in the collector and add them to the dropdown
            foreach (var ft in fec.ToElements())
            {
                Items.Add(new DynamoDropDownItem(ft.Name, ft));
            }

            Items = Items.OrderBy(x => x.Name).ToObservableCollection();
        }

        /// <summary>
        /// Cast the selected element to a dynamo node
        /// </summary>
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            // If there are no elements in the dropdown or the selected Index is invalid return a Null node.
            if (Items.Count == 0 ||
                Items[0].Name == "No types found" ||
                SelectedIndex == -1 || Items[SelectedIndex].Name == "none")
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            // Cast the selected object to a Revit Element and get its Id
            var id = ((Element)Items[SelectedIndex].Item).Id;

            // Select the element using the elementIds Integer Value
            var node = AstFactory.BuildFunctionCall("Revit.Elements.ElementSelector", "ByElementId",
                new List<AssociativeNode> { AstFactory.BuildIntNode(id.IntegerValue) });

            // Return the selected element
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DropDownItemEqualityComparer : IEqualityComparer<DynamoDropDownItem>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(DynamoDropDownItem x, DynamoDropDownItem y)
        {
            return string.Equals(x.Name, y.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(DynamoDropDownItem obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
