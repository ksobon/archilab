#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml;
using archilabUI.Utilities;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.Engine;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;

#endregion

namespace archilabUI.BuiltInParamSelector
{
    [NodeCategory("archilab.Revit.Parameter")]
    [NodeDescription("Allows you to select a BuiltInParameter name for use with GetBuiltInParameter node.")]
    [NodeName("Get BipParameter Name")]
    [IsDesignScriptCompatible]
    public class BuiltInParamSelector : NodeModel
    {
        #region Properties

        public event Action RequestChangeBuiltInParamSelector;
        internal EngineController EngineController { get; set; }

        private ObservableCollection<ParameterWrapper> _itemsCollection;
        public ObservableCollection<ParameterWrapper> ItemsCollection
        {
            get { return _itemsCollection; }
            set { _itemsCollection = value; RaisePropertyChanged("ItemsCollection"); }
        }

        private ParameterWrapper _selectedItem;
        public ParameterWrapper SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
                OnNodeModified(true);
            }
        }

        #endregion

        public BuiltInParamSelector()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("Element", "Input Element.")));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("bipName", "Name of the BuiltInParameter selected.")));
            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.Disabled;

            foreach (var current in InPorts)
            {
                current.Connectors.CollectionChanged += Connectors_CollectionChanged;
            }
            ItemsCollection = new ObservableCollection<ParameterWrapper>();
        }

        [JsonConstructor]
        protected BuiltInParamSelector(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts,
            outPorts)
        {
            foreach (var current in InPorts)
            {
                current.Connectors.CollectionChanged += Connectors_CollectionChanged;
            }
        }

        #region UI Methods

        protected virtual void OnRequestChangeBuiltInParamSelector()
        {
            RequestChangeBuiltInParamSelector?.Invoke();
        }

        private void Connectors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                ItemsCollection.Clear();
                SelectedItem = null;
                OnRequestChangeBuiltInParamSelector();
            }
            else
            {
                OnRequestChangeBuiltInParamSelector();
            }
        }

        private static IEnumerable<ParameterWrapper> GetParameters(Element e, string prefix)
        {
            var items = new List<ParameterWrapper>();
            foreach (Parameter p in e.Parameters)
            {
                if (p.StorageType != StorageType.None)
                {
                    if (!(p.Definition is InternalDefinition iDef)) continue;

                    var bipName = iDef.BuiltInParameter.ToString();
                    items.Add(new ParameterWrapper { Name = prefix + " | " + p.Definition.Name, BipName = bipName });
                }
            }
            return items;
        }

        private Element GetInputElement()
        {
            Element e = null;

            var owner = InPorts[0].Connectors[0].Start.Owner;
            var index = InPorts[0].Connectors[0].Start.Index;
            var name = owner.GetAstIdentifierForOutputIndex(index).Name;
            var mirror = EngineController.GetMirror(name);
            if (!mirror.GetData().IsCollection)
            {
                var element = (Revit.Elements.Element)mirror.GetData().Data;
                if (element != null)
                {
                    e = element.InternalElement;
                }
            }
            return e;
        }

        public void PopulateItems()
        {
            var e = GetInputElement();
            if (e != null)
            {
                var items = new List<ParameterWrapper>();

                // add instance parameters
                items.AddRange(GetParameters(e, "Instance"));

                // add type parameters
                if (e.CanHaveTypeAssigned())
                {
                    var et = DocumentManager.Instance.CurrentDBDocument.GetElement(e.GetTypeId());
                    if (et != null)
                    {
                        items.AddRange(GetParameters(et, "Type"));
                    }
                }
                ItemsCollection = new ObservableCollection<ParameterWrapper>(items.OrderBy(x => x.Name));
            }

            if (SelectedItem == null)
            {
                SelectedItem = ItemsCollection.FirstOrDefault();
            }
        }

        #endregion

        #region Node Serialization/Deserialization

        private string SerializeValue()
        {
            if (SelectedItem != null)
            {
                return SelectedItem.Name + "+" + SelectedItem.BipName;
            }
            return string.Empty;
        }

        private ParameterWrapper DeserializeValue(string val)
        {
            try
            {
                var name = val.Split('+')[0];
                var bipName = val.Split('+')[1];
                return SelectedItem = new ParameterWrapper { Name = name, BipName = bipName };
            }
            catch
            {
                return SelectedItem = new ParameterWrapper { Name = "None", BipName = "None" };
            }
        }

        [Obsolete]
        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);

            if (nodeElement.OwnerDocument == null) return;

            var wrapper = nodeElement.OwnerDocument.CreateElement("paramWrapper");
            wrapper.InnerText = SerializeValue();
            nodeElement.AppendChild(wrapper);
        }

        [Obsolete]
        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);

            var colorNode = nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "paramWrapper");
            if (colorNode == null) return;

            var deserialized = DeserializeValue(colorNode.InnerText);
            if (deserialized.Name == "None" && deserialized.BipName == "None")
            {
                return;
            }
            SelectedItem = deserialized;
        }

        #endregion

        [IsVisibleInDynamoLibrary(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var list = new List<AssociativeNode>();
            if (SelectedItem == null)
            {
                list.Add(AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()));
            }
            else
            {
                AssociativeNode associativeNode = AstFactory.BuildStringNode(SelectedItem.BipName);
                list.Add(AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), associativeNode));
            }
            return list;
        }
    }
}
