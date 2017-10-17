using System;
using System.Collections.Generic;
using System.Linq;
using CoreNodeModels;
using DSRevitNodesUI;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;

namespace archilabUI.Utilities
{
    public abstract class CustomGenericEnumerationDropDown : RevitDropDownBase
    {
        protected CustomGenericEnumerationDropDown(string name, Type enumerationType) : base(name)
        {
            EnumerationType = enumerationType;
            PopulateItems();
        }

        public Type EnumerationType;

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
    /// Generic UI Dropdown node baseclass for Revit Elements.
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
            var fec = new Autodesk.Revit.DB.FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument).OfClass(ElementType);

            // If there is nothing in the collector add the missing Type message to the Dropdown menu.
            if (fec.ToElements().Count == 0)
            {
                Items.Add(new DynamoDropDownItem("No types found", null));
                SelectedIndex = 0;
                return;
            }

            // if the elementtype is a RebarHookType add an initial "None" value which does not come from the collector
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
            var id = ((Autodesk.Revit.DB.Element)Items[SelectedIndex].Item).Id;

            // Select the element using the elementIds Integer Value
            var node = AstFactory.BuildFunctionCall("Revit.Elements.ElementSelector", "ByElementId",
                new List<AssociativeNode> { AstFactory.BuildIntNode(id.IntegerValue) });

            // Return the selected element
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    public class DropDownItemEqualityComparer : IEqualityComparer<DynamoDropDownItem>
    {
        public bool Equals(DynamoDropDownItem x, DynamoDropDownItem y)
        {
            return string.Equals(x.Name, y.Name);
        }

        public int GetHashCode(DynamoDropDownItem obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
