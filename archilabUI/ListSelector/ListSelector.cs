using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dynamo.Graph.Nodes;
using System.Collections.Specialized;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using Dynamo.Engine;
using Dynamo.Graph;
using Dynamo.UI.Commands;
using ProtoCore.AST.AssociativeAST;
using archilabUI.Utilities;

namespace archilabUI.ListSelector
{
    [NodeName("List Selector")]
    [NodeCategory("archilab.Core.Lists")]
    [NodeDescription("Use this node to select multiple items from a list.")]
    [IsDesignScriptCompatible]
    [InPortNames("List")]
    [InPortTypes("Object")]
    [InPortDescriptions("Input List.")]
    [OutPortNames("List")]
    [OutPortTypes("Object")]
    [OutPortDescriptions("Selected items.")]
    public class ListSelector : NodeModel
    {
        public event Action UpdateItemsCollection;
        internal EngineController EngineController { get; set; }
        public ObservableCollection<ListItemWrapper> ItemsCollection { get; set; }

        [IsVisibleInDynamoLibrary(false)]
        public DelegateCommand OnItemChecked { get; set; }

        public ListSelector()
        {
            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.Disabled;

            foreach (var port in InPorts)
            {
                port.Connectors.CollectionChanged += Connectors_CollectionChanged;
            }
            ItemsCollection = new ObservableCollection<ListItemWrapper>();

            OnItemChecked = new DelegateCommand(ItemChecked, CanCheckItem);
        }

        #region UI Methods

        private static bool CanCheckItem(object obj)
        {
            return true;
        }

        private void ItemChecked(object obj)
        {
            OnNodeModified(true);
        }

        protected virtual void OnUpdateItemsCollection()
        {
            UpdateItemsCollection?.Invoke();
        }

        private void Connectors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                ItemsCollection.Clear();
                OnUpdateItemsCollection();
            }
            else
            {
                OnUpdateItemsCollection();
            }
        }

        public void PopulateItems(System.Collections.IList selectedItems)
        {
            if (!HasConnectedInput(0)) return;

            var owner = InPorts[0].Connectors[0].Start.Owner;
            var index = InPorts[0].Connectors[0].Start.Index;
            var mirrorName = owner.GetAstIdentifierForOutputIndex(index).Name;
            var mirror = EngineController.GetMirror(mirrorName);

            var data = mirror?.GetData();
            if (data == null) return;

            if (data.IsCollection)
            {
                var counter = 0;
                foreach (var item in data.GetElements())
                {
                    var i = item.Data;
                    var wrapper = new ListItemWrapper
                    {
                        Name = i.ToString(),
                        Index = counter
                    };

                    var existing = ItemsCollection.IndexOf(wrapper);
                    if (existing != -1)
                    {
                        ItemsCollection[existing].IsSelected = selectedItems.Contains(wrapper);
                    }
                    else
                    {
                        ItemsCollection.Add(wrapper);
                    }

                    counter++;
                }
            }
        }

        #endregion

        #region Node Serialization/Deserialization

        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);
            if (nodeElement.OwnerDocument == null) return;

            var counter = 0;
            var wrapperName = nodeElement.OwnerDocument.CreateElement("listWrapper_name");
            var wrapperSelected = nodeElement.OwnerDocument.CreateElement("listWrapper_selected");
            var wrapperIndex = nodeElement.OwnerDocument.CreateElement("listWrapper_index");
            foreach (var item in ItemsCollection)
            {
                wrapperName.SetAttribute("name" + counter, item.Name);
                wrapperSelected.SetAttribute("selected" + counter, item.IsSelected.ToString());
                wrapperIndex.SetAttribute("index" + counter, item.Index.ToString());
                counter++;
            }

            nodeElement.AppendChild(wrapperName);
            nodeElement.AppendChild(wrapperSelected);
            nodeElement.AppendChild(wrapperIndex);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            var wrapperName = nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "listWrapper_name");
            var wrapperSelected = nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "listWrapper_selected");
            var wrapperIndex = nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "listWrapper_index");

            if (wrapperName == null || wrapperSelected == null || wrapperIndex == null) return;
            if (wrapperName.Attributes == null || wrapperName.Attributes.Count <= 0) return;
            if (wrapperSelected.Attributes == null || wrapperSelected.Attributes.Count <= 0) return;
            if (wrapperIndex.Attributes == null || wrapperIndex.Attributes.Count <= 0) return;

            for (var i = 0; i <= wrapperName.Attributes.Count - 1; i++)
            {
                var name = wrapperName.Attributes[i].Value;
                var selected = wrapperSelected.Attributes[i].Value == "True";
                var index = int.Parse(wrapperIndex.Attributes[i].Value);
                var itemWrapper = new ListItemWrapper { Name = name, IsSelected = selected, Index = index };
                ItemsCollection.Add(itemWrapper);
            }
        }

        #endregion

        [IsVisibleInDynamoLibrary(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (!HasConnectedInput(0) || ItemsCollection.Count == 0 || ItemsCollection.Count == -1)
            {
                return new[]
                {
                    AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode())
                };
            }
            
            var inputIdentifier = (inputAstNodes[0] as IdentifierNode)?.Value;
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildExprList(ItemsCollection.Where(x => x.IsSelected)
                        .Select(y => AstFactory.BuildIdentifier(inputIdentifier, AstFactory.BuildIntNode(y.Index)))
                        .ToList<AssociativeNode>()))
            };
        }
    }
}
