﻿#region References

using System;
using System.Collections.Generic;
using System.Linq;
using archilab.Utilities;
using Autodesk.Revit.DB;
using CoreNodeModels;
using DSRevitNodesUI;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using RestSharp;
using RevitServices.Persistence;
using BuiltInParameterGroup = Autodesk.Revit.DB.BuiltInParameterGroup;
using ElementSelector = Revit.Elements.ElementSelector;
using FilterNumericRuleEvaluator = Autodesk.Revit.DB.FilterNumericRuleEvaluator;
using FilterNumericValueRule = Autodesk.Revit.DB.FilterNumericValueRule;
using FilterStringRuleEvaluator = archilab.Utilities.FilterStringRuleEvaluator;
using ParameterType = Autodesk.Revit.DB.ParameterType;

#endregion

namespace archilabUI
{
    [NodeName("Box Placement Types")]
    [NodeCategory("archilab.Revit.Views")]
    [NodeDescription("Retrieve all available Box Placement Types")]
    [IsDesignScriptCompatible]
    public class BoxPlacementTypeUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "boxPlacementType";
        public BoxPlacementTypeUi() : base(OutputName, typeof(BoxPlacement)) { }

        [JsonConstructor]
        public BoxPlacementTypeUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(BoxPlacement), inPorts, outPorts) { }
    }

    [NodeName("Parameter Groups")]
    [NodeCategory("archilab.Revit.Parameter")]
    [NodeDescription("Retrieve all available Parameter Groups.")]
    [IsDesignScriptCompatible]
    public class ParameterGroupUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "parameterGroup";
        public ParameterGroupUi() : base(OutputName, typeof(BuiltInParameterGroup)) { }

        [JsonConstructor]
        public ParameterGroupUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(BuiltInParameterGroup), inPorts, outPorts) { }
    }

    [NodeName("Fill Pattern Targets")]
    [NodeCategory("archilab.Revit.Select")]
    [NodeDescription("Retrieve FillPatternTarget types.")]
    [IsDesignScriptCompatible]
    public class FillPatternTargetUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "fillPatternTarget";
        public FillPatternTargetUi() : base(OutputName, typeof(FillPatternTarget)) { }

        [JsonConstructor]
        public FillPatternTargetUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(FillPatternTarget), inPorts, outPorts) { }
    }

    [NodeName("Parameter Types")]
    [NodeCategory("archilab.Revit.Parameter")]
    [NodeDescription("Retrieve all available Parameter Types from Revit project.")]
    [IsDesignScriptCompatible]
    public class ParameterTypeUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "parameterType";
        public ParameterTypeUi() : base(OutputName, typeof(ParameterType)) { }

        [JsonConstructor]
        public ParameterTypeUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(ParameterType), inPorts, outPorts) { }
    }

    [NodeName("Print Range")]
    [NodeCategory("archilab.Revit.Printing")]
    [NodeDescription("Retrieve all available Print Ranges from Revit project.")]
    [IsDesignScriptCompatible]
    public class PrintRangeUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "printRange";
        public PrintRangeUi() : base(OutputName, typeof(PrintRange)) { }

        [JsonConstructor]
        public PrintRangeUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(PrintRange), inPorts, outPorts) { }
    }

    [NodeName("Fit Direction Type")]
    [NodeCategory("archilab.Revit.Schedule")]
    [NodeDescription("Retrieve all available Fit Direction Types from Revit project.")]
    [IsDesignScriptCompatible]
    public class FitDirectionTypeUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "fitDirectionType";
        public FitDirectionTypeUi() : base(OutputName, typeof(FitDirectionType)) { }

        [JsonConstructor]
        public FitDirectionTypeUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(FitDirectionType), inPorts, outPorts) { }
    }

    [NodeName("Image Resolution")]
    [NodeCategory("archilab.Revit.Views")]
    [NodeDescription("Retrieve all available Image Resolutions from Revit project.")]
    [IsDesignScriptCompatible]
    public class ImageResolutionUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "imageResolution";
        public ImageResolutionUi() : base(OutputName, typeof(ImageResolution)) { }

        [JsonConstructor]
        public ImageResolutionUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(ImageResolution), inPorts, outPorts) { }
    }

    [NodeName("Zoom Fit Type")]
    [NodeCategory("archilab.Revit.Views")]
    [NodeDescription("Retrieve all available Zoom Fit Types from Revit project.")]
    [IsDesignScriptCompatible]
    public class ZoomFitTypeUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "zoomFitType";
        public ZoomFitTypeUi() : base(OutputName, typeof(ZoomFitType)) { }

        [JsonConstructor]
        public ZoomFitTypeUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(ZoomFitType), inPorts, outPorts) { }
    }

    [NodeName("Image File Type")]
    [NodeCategory("archilab.Revit.Views")]
    [NodeDescription("Retrieve all available Image File Types from Revit project.")]
    [IsDesignScriptCompatible]
    public class ImageFileTypeUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "imageFileType";
        public ImageFileTypeUi() : base(OutputName, typeof(ImageFileType)) { }

        [JsonConstructor]
        public ImageFileTypeUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(ImageFileType), inPorts, outPorts) { }
    }

    [NodeName("Export Range")]
    [NodeCategory("archilab.Revit.Views")]
    [NodeDescription("Retrieve all available Export Ranges from Revit project.")]
    [IsDesignScriptCompatible]
    public class ExportRangeUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "exportRange";
        public ExportRangeUi() : base(OutputName, typeof(ExportRange)) { }

        [JsonConstructor]
        public ExportRangeUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(ExportRange), inPorts, outPorts) { }
    }

    [NodeName("Print Settings")]
    [NodeCategory("archilab.Revit.Printing")]
    [NodeDescription("Retrieve all available Print Settings from Revit project.")]
    [IsDesignScriptCompatible]
    public class PrintSettingUi : CustomRevitElementDropDown
    {
        private const string OutputName = "printSetting";
        public PrintSettingUi() : base(OutputName, typeof(PrintSetting)) { }

        [JsonConstructor]
        public PrintSettingUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(PrintSetting), inPorts, outPorts) { }
    }

    [NodeName("View Sets")]
    [NodeCategory("archilab.Revit.Printing")]
    [NodeDescription("Retrieve all available View Sets from Revit project.")]
    [IsDesignScriptCompatible]
    public class ViewSetUi : CustomRevitElementDropDown
    {
        private const string OutputName = "viewSet";
        public ViewSetUi() : base(OutputName, typeof(ViewSheetSet)) { }

        [JsonConstructor]
        public ViewSetUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(ViewSheetSet), inPorts, outPorts) { }
    }

    [NodeName("Schedule Heading Orientations")]
    [NodeCategory("archilab.Revit.Schedule")]
    [NodeDescription("Retrieve all available Heading Orientation values from Revit project.")]
    [IsDesignScriptCompatible]
    public class ScheduleHeadingOrientationUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "scheduleHeadingOrientation";
        public ScheduleHeadingOrientationUi() : base(OutputName, typeof(ScheduleHeadingOrientation)) { }

        [JsonConstructor]
        public ScheduleHeadingOrientationUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(ScheduleHeadingOrientation), inPorts, outPorts) { }
    }

    [NodeName("Schedule Horizontal Alignment")]
    [NodeCategory("archilab.Revit.Schedule")]
    [NodeDescription("Retrieve all available Horizontal Alignment Types from Revit project.")]
    [IsDesignScriptCompatible]
    public class ScheduleHorizontalAlignmentUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "scheduleHorizontalAlignment";
        public ScheduleHorizontalAlignmentUi() : base(OutputName, typeof(ScheduleHorizontalAlignment)) { }

        [JsonConstructor]
        public ScheduleHorizontalAlignmentUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(ScheduleHorizontalAlignment), inPorts, outPorts) { }
    }

    [NodeName("Horizontal Alignment Style")]
    [NodeCategory("archilab.Revit.Schedule")]
    [NodeDescription("Retrieve all available Horizontal Alignment Styles from Revit project.")]
    [IsDesignScriptCompatible]
    public class HorizontalAlignmentStyleUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "horizontalAlignmentStyle";
        public HorizontalAlignmentStyleUi() : base(OutputName, typeof(HorizontalAlignmentStyle)) { }

        [JsonConstructor]
        public HorizontalAlignmentStyleUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(HorizontalAlignmentStyle), inPorts, outPorts) { }
    }

    [NodeName("Vertical Alignment Style")]
    [NodeCategory("archilab.Revit.Schedule")]
    [NodeDescription("Retrieve all available Vertical Alignment Styles from Revit project.")]
    [IsDesignScriptCompatible]
    public class VerticalAlignmentStyleUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "verticalAlignmentStyle";
        public VerticalAlignmentStyleUi() : base(OutputName, typeof(VerticalAlignmentStyle)) { }

        [JsonConstructor]
        public VerticalAlignmentStyleUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(VerticalAlignmentStyle), inPorts, outPorts) { }
    }

    [NodeName("Schedule Sort Order")]
    [NodeCategory("archilab.Revit.Schedule")]
    [NodeDescription("Retrieve all available Schedule Sort Order types from Revit project.")]
    [IsDesignScriptCompatible]
    public class ScheduleSortOrderUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "scheduleSortOrder";
        public ScheduleSortOrderUi() : base(OutputName, typeof(ScheduleSortOrder)) { }

        [JsonConstructor]
        public ScheduleSortOrderUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(ScheduleSortOrder), inPorts, outPorts) { }
    }

    [NodeName("Numeric Rule Evaluators")]
    [NodeCategory("archilab.Revit.Select")]
    [NodeDescription("Retrieve all available Numeric Rule Evaluators.")]
    [IsDesignScriptCompatible]
    public class FilterNumericRuleEvaluatorUi : RevitDropDownBase
    {
        private const string OutputName = "evaluator";
        private const string NoFamilyTypes = "No types were found.";

        public FilterNumericRuleEvaluatorUi() : base(OutputName) { }

        [JsonConstructor]
        public FilterNumericRuleEvaluatorUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(OutputName, inPorts, outPorts) { }

        // Get Data Class that holds dictionary
        public static archilab.Utilities.FilterNumericRuleEvaluator WTypes = new archilab.Utilities.FilterNumericRuleEvaluator();

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var d = new Dictionary<string, FilterNumericRuleEvaluator>(WTypes.Rules);

            if (d.Count == 0)
            {
                Items.Add(new DynamoDropDownItem(NoFamilyTypes, null));
                SelectedIndex = 0;
                return SelectionState.Done;
            }

            foreach (var pair in d)
            {
                Items.Add(new DynamoDropDownItem(pair.Key, pair.Value));
            }
            Items = Items.OrderBy(x => x.Name).ToObservableCollection();
            return SelectionState.Restore;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 ||
                Items[0].Name == NoFamilyTypes ||
                SelectedIndex == -1)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            var args = new List<AssociativeNode>
            {
                AstFactory.BuildStringNode(Items[SelectedIndex].Name)
            };

            var func = new Func<string, FilterNumericRuleEvaluator>(archilab.Utilities.FilterNumericRuleEvaluator.ByName);
            var functionCall = AstFactory.BuildFunctionCall(func, args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("String Rule Evaluators")]
    [NodeCategory("archilab.Revit.Select")]
    [NodeDescription("Retrieve all available String Rule Evaluators.")]
    [IsDesignScriptCompatible]
    public class FilterStringRuleEvaluatorUi : RevitDropDownBase
    {
        private const string OutputName = "evaluator";
        private const string NoFamilyTypes = "No types were found.";

        public FilterStringRuleEvaluatorUi() : base(OutputName) { }

        [JsonConstructor]
        public FilterStringRuleEvaluatorUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(OutputName, inPorts, outPorts) { }

        // Get Data Class that holds dictionary
        public static FilterStringRuleEvaluator WTypes = new FilterStringRuleEvaluator();

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var d = new Dictionary<string, Autodesk.Revit.DB.FilterStringRuleEvaluator>(WTypes.Rules);

            if (d.Count == 0)
            {
                Items.Add(new DynamoDropDownItem(NoFamilyTypes, null));
                SelectedIndex = 0;
                return SelectionState.Done;
            }

            foreach (var pair in d)
            {
                Items.Add(new DynamoDropDownItem(pair.Key, pair.Value));
            }
            Items = Items.OrderBy(x => x.Name).ToObservableCollection();
            return SelectionState.Restore;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 ||
                Items[0].Name == NoFamilyTypes ||
                SelectedIndex == -1)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            var args = new List<AssociativeNode>
            {
                AstFactory.BuildStringNode(Items[SelectedIndex].Name)
            };

            var func = new Func<string, Autodesk.Revit.DB.FilterStringRuleEvaluator>(FilterStringRuleEvaluator.ByName);
            var functionCall = AstFactory.BuildFunctionCall(func, args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("Filter Numeric Value Rules")]
    [NodeCategory("archilab.Revit.Select")]
    [NodeDescription("Retrieve all available Filter Numeric Value Rules.")]
    [IsDesignScriptCompatible]
    public class FilterNumericValueRuleUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "filterNumericValueRule";
        public FilterNumericValueRuleUi() : base(OutputName, typeof(FilterNumericValueRule)) { }

        [JsonConstructor]
        public FilterNumericValueRuleUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(FilterNumericValueRule), inPorts, outPorts) { }
    }

    [NodeName("Line Styles")]
    [NodeCategory("archilab.Revit.Views")]
    [NodeDescription("Retrieve all available Line Styles.")]
    [IsDesignScriptCompatible]
    public class LineStyleUi : RevitDropDownBase
    {
        private const string OutputName = "lineStyle";
        private const string NoFamilyTypes = "No types were found.";

        public LineStyleUi() : base(OutputName) { }

        [JsonConstructor]
        public LineStyleUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(OutputName, inPorts, outPorts) { }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();
            var cat = Autodesk.Revit.DB.Category.GetCategory(DocumentManager.Instance.CurrentDBDocument, BuiltInCategory.OST_Lines);
            var gsCat = cat.GetGraphicsStyle(GraphicsStyleType.Projection).GraphicsStyleCategory.SubCategories;

            var lineStyles = new List<GraphicsStyle>();
            foreach (Category c in gsCat)
            {
                lineStyles.Add(c.GetGraphicsStyle(GraphicsStyleType.Projection));
            }

            if (lineStyles.Count == 0)
            {
                Items.Add(new DynamoDropDownItem(NoFamilyTypes, null));
                SelectedIndex = 0;
                return SelectionState.Done;
            }

            foreach (var style in lineStyles)
            {
                Items.Add(new DynamoDropDownItem(style.Name, style));
            }
            Items = Items.OrderBy(x => x.Name).ToObservableCollection();
            return SelectionState.Restore;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;

            if (SelectedIndex == -1)
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var graphicStyle = Items[SelectedIndex].Item as GraphicsStyle;
                if (graphicStyle == null)
                {
                    node = AstFactory.BuildNullNode();
                }
                else
                {
                    var idNode = AstFactory.BuildStringNode(graphicStyle.UniqueId);
                    var falseNode = AstFactory.BuildBooleanNode(true);

                    node = AstFactory.BuildFunctionCall(
                        new Func<string, bool, object>(ElementSelector.ByUniqueId),
                        new List<AssociativeNode> { idNode, falseNode });
                }
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    [NodeName("View Templates")]
    [NodeCategory("archilab.Revit.ViewTemplate")]
    [NodeDescription("Retrieve all available View Templates (except 3D view based due to Dynamo limitation).")]
    [IsDesignScriptCompatible]
    public class ViewTemplatesUi : RevitDropDownBase
    {
        private const string OutputName = "viewTemplate";
        private const string NoFamilyTypes = "No types were found.";

        public ViewTemplatesUi() : base(OutputName) { }

        [JsonConstructor]
        public ViewTemplatesUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(OutputName, inPorts, outPorts) { }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var vtList = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(x => x.IsTemplate && x.ViewType != ViewType.ThreeD)
                .ToList();

            if (vtList.Count == 0)
            {
                Items.Add(new DynamoDropDownItem(NoFamilyTypes, null));
                SelectedIndex = 0;
                return SelectionState.Done;
            }

            foreach (var v in vtList)
            {
                Items.Add(new DynamoDropDownItem(v.Name, v));
            }
            Items = Items.OrderBy(x => x.Name).ToObservableCollection();
            return SelectionState.Restore;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;

            if (SelectedIndex == -1)
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var view = Items[SelectedIndex].Item as View;
                if (view == null)
                {
                    node = AstFactory.BuildNullNode();
                }
                else
                {
                    var idNode = AstFactory.BuildStringNode(view.UniqueId);
                    var falseNode = AstFactory.BuildBooleanNode(true);

                    node = AstFactory.BuildFunctionCall(
                        new Func<string, bool, object>(ElementSelector.ByUniqueId),
                        new List<AssociativeNode> { idNode, falseNode });
                }
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    [NodeName("Worksets")]
    [NodeCategory("archilab.Revit.Workset")]
    [NodeDescription("Retrieve all available Worksets.")]
    [IsDesignScriptCompatible]
    public class WorksetUi : RevitDropDownBase
    {
        private const string outputName = "workset";
        private const string NoFamilyTypes = "No types were found.";

        public WorksetUi() : base(outputName) { }

        [JsonConstructor]
        public WorksetUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts) { }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var allWorksets = new FilteredWorksetCollector(DocumentManager.Instance.CurrentDBDocument)
                .OfKind(WorksetKind.UserWorkset)
                .ToList();

            if (allWorksets.Count == 0)
            {
                Items.Add(new DynamoDropDownItem(NoFamilyTypes, null));
                SelectedIndex = 0;
                return SelectionState.Done;
            }

            foreach (var w in allWorksets)
            {
                Items.Add(new DynamoDropDownItem(w.Name, w));
            }
            Items = Items.OrderBy(x => x.Name).ToObservableCollection();
            return SelectionState.Restore;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;

            if (SelectedIndex == -1)
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var workset = Items[SelectedIndex].Item as Workset;
                if (workset == null)
                {
                    node = AstFactory.BuildNullNode();
                }
                else
                {
                    var idNode = AstFactory.BuildIntNode(workset.Id.IntegerValue);
                    node = AstFactory.BuildFunctionCall(
                        new Func<int, archilab.Revit.Elements.Workset>(archilab.Utilities.ElementSelector.GetWorksetById),
                        new List<AssociativeNode> { idNode });
                }
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    [NodeName("View Type")]
    [NodeCategory("archilab.Revit.Views")]
    [NodeDescription("Retrieve all available View Types.")]
    [IsDesignScriptCompatible]
    public class ViewTypesUi : RevitDropDownBase
    {
        private const string NoFamilyTypes = "No types were found.";
        private const string outputName = "viewType";

        public ViewTypesUi() : base("viewType") { }

        [JsonConstructor]
        public ViewTypesUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts) { }

        // Get Data Class that holds dictionary
        public static ViewTypes VTypes = new ViewTypes();

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var d = new Dictionary<string, string>(VTypes.Types);

            if (d.Count == 0)
            {
                Items.Add(new DynamoDropDownItem(NoFamilyTypes, null));
                SelectedIndex = 0;
                return SelectionState.Done;
            }

            foreach (var pair in d)
            {
                Items.Add(new DynamoDropDownItem(pair.Key, pair.Value));
            }
            Items = Items.OrderBy(x => x.Name).ToObservableCollection();
            return SelectionState.Restore;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 ||
                Items[0].Name == NoFamilyTypes ||
                SelectedIndex == -1)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            var args = new List<AssociativeNode>
            {
                AstFactory.BuildStringNode(Items[SelectedIndex].Name)
            };

            var func = new Func<string, string>(ViewTypes.ByName);
            var functionCall = AstFactory.BuildFunctionCall(func, args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    internal enum FilterRules
    {
        None,
        LessThan,
        BeginsWith,
        Contains,
        DoesNotBeginWith,
        DoesNotContain,
        DoesNotEndWith,
        EndsWith,
        Equals,
        GreaterThanOrEqual,
        LessThenOrEqual,
        DoesNotEqual
    }

    [NodeName("Phase Filters")]
    [NodeCategory("archilab.Revit.Views")]
    [NodeDescription("Retrieve all available Phase Filters.")]
    [IsDesignScriptCompatible]
    public class PhaseFilterUi : CustomRevitElementDropDown
    {
        private const string OutputName = "phaseFilter";
        public PhaseFilterUi() : base(OutputName, typeof(PhaseFilter)) { }

        [JsonConstructor]
        public PhaseFilterUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(PhaseFilter), inPorts, outPorts) { }
    }

    [NodeName("Workset Kind")]
    [NodeCategory("archilab.Revit.Workset")]
    [NodeDescription("Retrieve all available Workset kinds.")]
    [IsDesignScriptCompatible]
    public class WorksetKindUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "kind";
        public WorksetKindUi() : base(OutputName, typeof(WorksetKind)) { }

        [JsonConstructor]
        public WorksetKindUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(WorksetKind), inPorts, outPorts) { }
    }

    [NodeName("Workset Visibility")]
    [NodeCategory("archilab.Revit.Workset")]
    [NodeDescription("Retrieve all available Workset Visibility settings.")]
    [IsDesignScriptCompatible]
    public class WorksetVisibilityUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "visibility";
        public WorksetVisibilityUi() : base(OutputName, typeof(WorksetVisibility)) { }

        [JsonConstructor]
        public WorksetVisibilityUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(WorksetVisibility), inPorts, outPorts) { }
    }

    [NodeName("Duplicate Options")]
    [NodeCategory("archilab.Revit.Views")]
    [NodeDescription("Retrieve all available View Duplication Options.")]
    [IsDesignScriptCompatible]
    public class DuplicateOptionsUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "options";
        public DuplicateOptionsUi() : base(OutputName, typeof(ViewDuplicateOption)) { }

        [JsonConstructor]
        public DuplicateOptionsUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(ViewDuplicateOption), inPorts, outPorts) { }
    }

    [NodeName("Method Types")]
    [NodeCategory("archilab.Http.Http")]
    [NodeDescription("Retrieve all available Http Request types.")]
    [IsDesignScriptCompatible]
    public class HttpMethodType : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "methodType";
        public HttpMethodType() : base(OutputName, typeof(Method)) { }

        [JsonConstructor]
        public HttpMethodType(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(Method), inPorts, outPorts) { }
    }
}
