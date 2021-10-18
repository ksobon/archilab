#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
using ElementSelector = Revit.Elements.ElementSelector;
// ReSharper disable InconsistentNaming

#endregion

namespace archilabUI
{
    [NodeName("Box Placement Types")]
    [NodeCategory("archilab.Revit.Views.Query")]
    [NodeDescription("Retrieve all available Box Placement Types")]
    [IsDesignScriptCompatible]
    public class BoxPlacementTypeUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "boxPlacementType";

        /// <summary>
        /// 
        /// </summary>
        public BoxPlacementTypeUi() : base(OutputName, typeof(BoxPlacement)) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inPorts"></param>
        /// <param name="outPorts"></param>
        [JsonConstructor]
        public BoxPlacementTypeUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(BoxPlacement), inPorts, outPorts) { }
    }

    [NodeName("Parameter Groups")]
    [NodeCategory("archilab.Revit.Parameter.Query")]
    [NodeDescription("Retrieve all available Parameter Groups.")]
    [IsDesignScriptCompatible]
    public class ParameterGroupUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "parameterGroup";
        public ParameterGroupUi() : base(OutputName, typeof(Autodesk.Revit.DB.BuiltInParameterGroup)) { }

        [JsonConstructor]
        public ParameterGroupUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(Autodesk.Revit.DB.BuiltInParameterGroup), inPorts, outPorts) { }
    }

    [NodeName("Fill Pattern Target")]
    [NodeCategory("archilab.Revit.Select.Query")]
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
    [NodeCategory("archilab.Revit.Parameter.Query")]
    [NodeDescription("Retrieve all available Parameter Types from Revit project.")]
    [IsDesignScriptCompatible]
    public class ParameterTypeUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "parameterType";
        public ParameterTypeUi() : base(OutputName, typeof(Autodesk.Revit.DB.ParameterType)) { }

        [JsonConstructor]
        public ParameterTypeUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(Autodesk.Revit.DB.ParameterType), inPorts, outPorts) { }
    }

    [NodeName("Print Range")]
    [NodeCategory("archilab.Revit.Printing.Query")]
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
    [NodeCategory("archilab.Revit.Schedule.Query")]
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
    [NodeCategory("archilab.Revit.Views.Query")]
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
    [NodeCategory("archilab.Revit.Views.Query")]
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
    [NodeCategory("archilab.Revit.Views.Query")]
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
    [NodeCategory("archilab.Revit.Views.Query")]
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
    [NodeCategory("archilab.Revit.Printing.Query")]
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
    [NodeCategory("archilab.Revit.Printing.Query")]
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
    [NodeCategory("archilab.Revit.Schedule.Query")]
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
    [NodeCategory("archilab.Revit.Schedule.Query")]
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
    [NodeCategory("archilab.Revit.Schedule.Query")]
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
    [NodeCategory("archilab.Revit.Schedule.Query")]
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
    [NodeCategory("archilab.Revit.Schedule.Query")]
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

    [NodeName("Select Rule Type")]
    [NodeCategory("archilab.Revit.FilterRule.Query")]
    [NodeDescription("Retrieve all available Filter Rules.")]
    [IsDesignScriptCompatible]
    public class FilterRuleTypesUi : RevitDropDownBase
    {
        private const string OutputName = "ruleType";
        private const string NoFamilyTypes = "No types were found.";

        public FilterRuleTypesUi() : base(OutputName) { }

        [JsonConstructor]
        public FilterRuleTypesUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(OutputName, inPorts, outPorts) { }

        // Get Data Class that holds dictionary
        public static FilterRuleTypes WTypes = new FilterRuleTypes();

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var d = new Dictionary<string, string>(WTypes.Rules);

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

            var func = new Func<string, string>(FilterRuleTypes.ByName);
            var functionCall = AstFactory.BuildFunctionCall(func, args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("Numeric Rule Evaluators")]
    [NodeCategory("archilab.Revit.Select.Query")]
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

            var d = new Dictionary<string, Autodesk.Revit.DB.FilterNumericRuleEvaluator>(WTypes.Rules);

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

            var func = new Func<string, Autodesk.Revit.DB.FilterNumericRuleEvaluator>(archilab.Utilities.FilterNumericRuleEvaluator.ByName);
            var functionCall = AstFactory.BuildFunctionCall(func, args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("String Rule Evaluators")]
    [NodeCategory("archilab.Revit.Select.Query")]
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
        public static archilab.Utilities.FilterStringRuleEvaluator WTypes = new archilab.Utilities.FilterStringRuleEvaluator();

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

            var func = new Func<string, Autodesk.Revit.DB.FilterStringRuleEvaluator>(archilab.Utilities.FilterStringRuleEvaluator.ByName);
            var functionCall = AstFactory.BuildFunctionCall(func, args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("Google Map Types")]
    [NodeCategory("archilab.Maps.GoogleMaps.Query")]
    [NodeDescription("")]
    [IsDesignScriptCompatible]
    public class GoogleMapTypesUi : RevitDropDownBase
    {
        private const string OutputName = "mapType";
        private const string NoFamilyTypes = "No types were found.";

        public GoogleMapTypesUi() : base(OutputName) { }

        [JsonConstructor]
        public GoogleMapTypesUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(OutputName, inPorts, outPorts) { }

        // Get Data Class that holds dictionary
        public static GoogleMapTypes WTypes = new GoogleMapTypes();

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var d = new Dictionary<string, string>(WTypes.MapTypes);

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

            var func = new Func<string, string>(GoogleMapTypes.ByName);
            var functionCall = AstFactory.BuildFunctionCall(func, args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("Google Image Formats")]
    [NodeCategory("archilab.Maps.GoogleMaps.Query")]
    [NodeDescription("")]
    [IsDesignScriptCompatible]
    public class GoogleImageFormatsUi : RevitDropDownBase
    {
        private const string OutputName = "imageFormat";
        private const string NoFamilyTypes = "No types were found.";

        public GoogleImageFormatsUi() : base(OutputName) { }

        [JsonConstructor]
        public GoogleImageFormatsUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(OutputName, inPorts, outPorts) { }

        // Get Data Class that holds dictionary
        public static GoogleImageFormats WTypes = new GoogleImageFormats();

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var d = new Dictionary<string, string>(WTypes.ImageFormats);

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

            var func = new Func<string, string>(GoogleImageFormats.ByName);
            var functionCall = AstFactory.BuildFunctionCall(func, args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("View Template Parameters")]
    [NodeCategory("archilab.Revit.ViewTemplates.Query")]
    [NodeDescription("Retrieve all available built in View Template parameters.")]
    [IsDesignScriptCompatible]
    public class ViewTemplateParametersUi : RevitDropDownBase
    {
        private const string OutputName = "parameter";
        private const string NoFamilyTypes = "No types were found.";

        public ViewTemplateParametersUi() : base(OutputName) { }

        [JsonConstructor]
        public ViewTemplateParametersUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(OutputName, inPorts, outPorts) { }

        // Get Data Class that holds dictionary
        public static ViewTemplateParameters WTypes = new ViewTemplateParameters();

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var d = new Dictionary<string, int>(WTypes.Parameters);

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

            var func = new Func<string, int>(ViewTemplateParameters.ByName);
            var functionCall = AstFactory.BuildFunctionCall(func, args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("Filter Numeric Value Rules")]
    [NodeCategory("archilab.Revit.Select.Query")]
    [NodeDescription("Retrieve all available Filter Numeric Value Rules.")]
    [IsDesignScriptCompatible]
    public class FilterNumericValueRuleUi : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "filterNumericValueRule";
        public FilterNumericValueRuleUi() : base(OutputName, typeof(Autodesk.Revit.DB.FilterNumericValueRule)) { }

        [JsonConstructor]
        public FilterNumericValueRuleUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) 
            : base(OutputName, typeof(Autodesk.Revit.DB.FilterNumericValueRule), inPorts, outPorts) { }
    }

    [NodeName("Line Styles")]
    [NodeCategory("archilab.Revit.Views.Query")]
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
                if (!(Items[SelectedIndex].Item is GraphicsStyle graphicStyle))
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
    [NodeCategory("archilab.Revit.ViewTemplates.Query")]
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
                if (!(Items[SelectedIndex].Item is View view))
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
    [NodeCategory("archilab.Revit.Workset.Query")]
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
                if (!(Items[SelectedIndex].Item is Workset workset))
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
    [NodeCategory("archilab.Revit.Views.Query")]
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

    [NodeName("Phase Filters")]
    [NodeCategory("archilab.Revit.Views.Query")]
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
    [NodeCategory("archilab.Revit.Workset.Query")]
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
    [NodeCategory("archilab.Revit.Workset.Query")]
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
    [NodeCategory("archilab.Revit.Views.Query")]
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
    [NodeCategory("archilab.Http.Http.Query")]
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

#if !Revit2018 && !Revit2021
    // (Konrad) This is replaced by Forge Unit in 2022 and up.
#else
    [NodeName("Unit Type")]
    [NodeCategory("archilab.Revit.Units.Query")]
    [NodeDescription("Retrieve all available Unit Types.")]
    [IsDesignScriptCompatible]
    public class UnitTypeUI : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "unitType";
        public UnitTypeUI() : base(OutputName, typeof(UnitType)) { }

        [JsonConstructor]
        public UnitTypeUI(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(OutputName, typeof(UnitType), inPorts, outPorts) { }
    }
#endif
    [NodeName("Unit Systems")]
    [NodeCategory("archilab.Revit.Units.Query")]
    [NodeDescription("Retrieve all available Unit Systems.")]
    [IsDesignScriptCompatible]
    public class UnitSystemUI : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "unitSystem";
        public UnitSystemUI() : base(OutputName, typeof(UnitSystem)) { }

        [JsonConstructor]
        public UnitSystemUI(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(OutputName, typeof(UnitSystem), inPorts, outPorts) { }
    }

#if !Revit2018 && !Revit2019 && !Revit2020 && !Revit2021
    [NodeName("Forge Units")]
    [NodeCategory("archilab.Revit.Units.Query")]
    [NodeDescription("Retrieve all available Forge Units.")]
    [IsDesignScriptCompatible]
    public class ForgeUnitsUi : RevitDropDownBase
    {
        private const string NoFamilyTypes = "No units were found.";
        private const string outputName = "forgeUnit";

        public ForgeUnitsUi() : base("forgeUnit") { }

        [JsonConstructor]
        public ForgeUnitsUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts) { }

        // Get Data Class that holds dictionary
        public static archilab.Utilities.Units VTypes = new archilab.Utilities.Units();

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var d = new Dictionary<string, string>(VTypes.ForgeUnits);

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

            var func = new Func<string, string>(archilab.Utilities.Units.ByName);
            var functionCall = AstFactory.BuildFunctionCall(func, args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }
    
    [NodeName("Forge Specs")]
    [NodeCategory("archilab.Revit.Units.Query")]
    [NodeDescription("Retrieve all available Forge Specs.")]
    [IsDesignScriptCompatible]
    public class ForgeSpecsUi : RevitDropDownBase
    {
        private const string NoFamilyTypes = "No specs were found.";
        private const string outputName = "forgeSpec";

        public ForgeSpecsUi() : base("forgeSpec") { }

        [JsonConstructor]
        public ForgeSpecsUi(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts) { }

        // Get Data Class that holds dictionary
        public static Specs VTypes = new Specs();

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var d = new Dictionary<string, string>(VTypes.ForgeSpecs);

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

            var func = new Func<string, string>(Specs.ByName);
            var functionCall = AstFactory.BuildFunctionCall(func, args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }
#else
    [NodeName("Display Unit Types")]
    [NodeCategory("archilab.Revit.Units.Query")]
    [NodeDescription("Retrieve all available Display Unit Types.")]
    [IsDesignScriptCompatible]
    public class DisplayUnitTypeUI : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "unitSystem";
        public DisplayUnitTypeUI() : base(OutputName, typeof(DisplayUnitType)) { }

        [JsonConstructor]
        public DisplayUnitTypeUI(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(OutputName, typeof(DisplayUnitType), inPorts, outPorts) { }
    }
#endif

    [NodeName("Revision Number Type")]
    [NodeCategory("archilab.Revit.Revisions.Query")]
    [NodeDescription("Retrieve all available Revision Number Types.")]
    [IsDesignScriptCompatible]
    public class RevisionNumberTypeUI : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "numberType";
        public RevisionNumberTypeUI() : base(OutputName, typeof(Autodesk.Revit.DB.RevisionNumberType)) { }

        [JsonConstructor]
        public RevisionNumberTypeUI(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(OutputName, typeof(Autodesk.Revit.DB.RevisionNumberType), inPorts, outPorts) { }
    }

    [NodeName("Revision Visibility")]
    [NodeCategory("archilab.Revit.Revisions.Query")]
    [NodeDescription("Retrieve all available Revision Visibility.")]
    [IsDesignScriptCompatible]
    public class RevisionVisibilityUI : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "visibility";
        public RevisionVisibilityUI() : base(OutputName, typeof(Autodesk.Revit.DB.RevisionVisibility)) { }

        [JsonConstructor]
        public RevisionVisibilityUI(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(OutputName, typeof(Autodesk.Revit.DB.RevisionVisibility), inPorts, outPorts) { }
    }

    [NodeName("Spatial Element Boundary Locations")]
    [NodeCategory("archilab.Revit.Room.Query")]
    [NodeDescription("Retrieve all available Spatial Element Boundary Locations.")]
    [IsDesignScriptCompatible]
    public class SpatialElementBoundaryLocationUI : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "location";
        public SpatialElementBoundaryLocationUI() : base(OutputName, typeof(SpatialElementBoundaryLocation)) { }

        [JsonConstructor]
        public SpatialElementBoundaryLocationUI(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(OutputName, typeof(SpatialElementBoundaryLocation), inPorts, outPorts) { }
    }

    [NodeName("Midpoint Rounding Types")]
    [NodeCategory("archilab.Core.Maths.Query")]
    [NodeDescription("Retrieve all available Midpoint Rounding Types.")]
    [IsDesignScriptCompatible]
    public class MidpointRoundingTypesUI : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "roundingType";
        public MidpointRoundingTypesUI() : base(OutputName, typeof(MidpointRounding)) { }

        [JsonConstructor]
        public MidpointRoundingTypesUI(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(OutputName, typeof(MidpointRounding), inPorts, outPorts) { }
    }

    [NodeName("Grid Extent Type")]
    [NodeCategory("archilab.Revit.Grids.Query")]
    [NodeDescription("Retrieve all available Grid Extent Types.")]
    [IsDesignScriptCompatible]
    public class ExtentTypeUI : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "extentType";
        public ExtentTypeUI() : base(OutputName, typeof(DatumExtentType)) { }

        [JsonConstructor]
        public ExtentTypeUI(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(OutputName, typeof(DatumExtentType), inPorts, outPorts) { }
    }

    [NodeName("Regex Options")]
    [NodeCategory("archilab.Core.Strings.Query")]
    [NodeDescription("Retrieve all available Regex Options.")]
    [IsDesignScriptCompatible]
    public class RegexOptionsUI : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "regexOption";
        public RegexOptionsUI() : base(OutputName, typeof(RegexOptions)) { }

        [JsonConstructor]
        public RegexOptionsUI(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(OutputName, typeof(RegexOptions), inPorts, outPorts) { }
    }

    [NodeName("Display Styles")]
    [NodeCategory("archilab.Revit.Views.Query")]
    [NodeDescription("Retrieve all available Display Styles.")]
    [IsDesignScriptCompatible]
    public class DisplayStylesUI : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "displayStyle";
        public DisplayStylesUI() : base(OutputName, typeof(DisplayStyle)) { }

        [JsonConstructor]
        public DisplayStylesUI(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(OutputName, typeof(DisplayStyle), inPorts, outPorts) { }
    }

    [NodeName("Tag Modes")]
    [NodeCategory("archilab.Revit.Tags.Query")]
    [NodeDescription("Retrieve all available Tag Modes.")]
    [IsDesignScriptCompatible]
    public class TagModesUI : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "tagMode";
        public TagModesUI() : base(OutputName, typeof(TagMode)) { }

        [JsonConstructor]
        public TagModesUI(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(OutputName, typeof(TagMode), inPorts, outPorts) { }
    }

    [NodeName("Tag Orientations")]
    [NodeCategory("archilab.Revit.Tags.Query")]
    [NodeDescription("Retrieve all available Tag Orientations.")]
    [IsDesignScriptCompatible]
    public class TagOrientationsUI : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "tagOrientation";
        public TagOrientationsUI() : base(OutputName, typeof(TagOrientation)) { }

        [JsonConstructor]
        public TagOrientationsUI(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(OutputName, typeof(TagOrientation), inPorts, outPorts) { }
    }
    
    [NodeName("Leader End Conditions")]
    [NodeCategory("archilab.Revit.Tags.Query")]
    [NodeDescription("Retrieve all available Leader End Conditions.")]
    [IsDesignScriptCompatible]
    public class LeaderEndConditionUI : CustomGenericEnumerationDropDown
    {
        private const string OutputName = "leaderEndCondition";
        public LeaderEndConditionUI() : base(OutputName, typeof(Autodesk.Revit.DB.LeaderEndCondition)) { }

        [JsonConstructor]
        public LeaderEndConditionUI(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(OutputName, typeof(Autodesk.Revit.DB.LeaderEndCondition), inPorts, outPorts) { }
    }
}
