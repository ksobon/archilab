using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml;
using archilabUI.Utilities;
using Autodesk.DesignScript.Runtime;
using Dynamo.Engine;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.UI.Commands;
using ProtoCore.AST.AssociativeAST;

namespace archilabUI.TextNotePlus
{
    [NodeName("Text Note Plus")]
    [NodeCategory("archilab.Core.Lists")]
    [NodeDescription("Use this node to select multiple items from a list.")]
    [IsDesignScriptCompatible]
    public class TextNotePlus : NodeModel
    {
        //public event Action UpdateItemsCollection;
        internal EngineController EngineController { get; set; }

        public string Notes { get; set; } = "Please enter text here...";

        private bool _readOnly = false;
        public bool ReadOnly {
            get { return _readOnly; }
            set { _readOnly = value; RaisePropertyChanged("ReadOnly"); }
        }

        
        //public ObservableCollection<ListItemWrapper> ItemsCollection { get; set; }

        //[IsVisibleInDynamoLibrary(false)]
        //public DelegateCommand OnItemChecked { get; set; }

        public TextNotePlus()
        {
            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.Disabled;

            //foreach (var port in InPorts)
            //{
            //    port.Connectors.CollectionChanged += Connectors_CollectionChanged;
            //}
            //ItemsCollection = new ObservableCollection<ListItemWrapper>();

            //OnItemChecked = new DelegateCommand(ItemChecked, CanCheckItem);
        }

        private static bool CanCheckItem(object obj)
        {
            return true;
        }

        private void ItemChecked(object obj)
        {
            OnNodeModified(true);
        }

        //protected virtual void OnUpdateItemsCollection()
        //{
        //    UpdateItemsCollection?.Invoke();
        //}

        //private void Connectors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.Action == NotifyCollectionChangedAction.Remove)
        //    {
        //        ItemsCollection.Clear();
        //        OnUpdateItemsCollection();
        //    }
        //    else
        //    {
        //        OnUpdateItemsCollection();
        //    }
        //}

        //public void PopulateItems(System.Collections.IList selectedItems)
        //{
        //    if (!HasConnectedInput(0)) return;

        //    var owner = InPorts[0].Connectors[0].Start.Owner;
        //    var index = InPorts[0].Connectors[0].Start.Index;
        //    var mirrorName = owner.GetAstIdentifierForOutputIndex(index).Name;
        //    var mirror = EngineController.GetMirror(mirrorName);

        //    var data = mirror?.GetData();
        //    if (data == null) return;

        //    if (data.IsCollection)
        //    {
        //        var counter = 0;
        //        foreach (var item in data.GetElements())
        //        {
        //            var i = item.Data;
        //            var wrapper = new ListItemWrapper
        //            {
        //                Name = i.ToString(),
        //                Index = counter
        //            };

        //            var existing = ItemsCollection.IndexOf(wrapper);
        //            if (existing != -1)
        //            {
        //                ItemsCollection[existing].IsSelected = selectedItems.Contains(wrapper);
        //            }
        //            else
        //            {
        //                ItemsCollection.Add(wrapper);
        //            }

        //            counter++;
        //        }
        //    }
        //}

        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);
            if (nodeElement.OwnerDocument == null) return;

            var wrapper = nodeElement.OwnerDocument.CreateElement("notes");
            wrapper.InnerText = Notes;
            nodeElement.AppendChild(wrapper);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);

            var colorNode = nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "notes");
            if (colorNode == null) return;

            Notes = colorNode.InnerText;
        }

        [IsVisibleInDynamoLibrary(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode())
            };
            //if (!HasConnectedInput(0) || ItemsCollection.Count == 0 || ItemsCollection.Count == -1)
            //{
            //    return new[]
            //    {
            //        AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode())
            //    };
            //}

            //var inputIdentifier = (inputAstNodes[0] as IdentifierNode)?.Value;
            //return new[]
            //{
            //    AstFactory.BuildAssignment(
            //        GetAstIdentifierForOutputIndex(0),
            //        AstFactory.BuildExprList(ItemsCollection.Where(x => x.IsSelected)
            //            .Select(y => AstFactory.BuildIdentifier(inputIdentifier, AstFactory.BuildIntNode(y.Index)))
            //            .ToList<AssociativeNode>()))
            //};
        }
    }
}
