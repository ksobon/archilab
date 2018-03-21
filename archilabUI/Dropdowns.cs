using System;
using System.Collections.Generic;
using System.Linq;
using archilab.Utilities;
using CoreNodeModels;
using DSRevitNodesUI;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using RestSharp;
using RevitServices.Persistence;
using Category = Autodesk.Revit.DB.Category;
using ElementSelector = Revit.Elements.ElementSelector;
using archilab.Revit.Elements;

namespace archilabUI
{
    [NodeName("Box Placement Types")]
    [NodeCategory("archilab.Revit.Views")]
    [NodeDescription("Retrieve all available Box Placement Types")]
    [IsDesignScriptCompatible]
    public class BoxPlacementTypeUi : CustomGenericEnumerationDropDown
    {
        public BoxPlacementTypeUi() : base("boxPlacementType", typeof(Autodesk.Revit.DB.BoxPlacement)) { }
    }

    [NodeName("Parameter Groups")]
    [NodeCategory("archilab.Revit.Parameter")]
    [NodeDescription("Retrieve all available Parameter Groups.")]
    [IsDesignScriptCompatible]
    public class ParameterGroupUi : CustomGenericEnumerationDropDown
    {
        public ParameterGroupUi() : base("parameterGroup", typeof(Autodesk.Revit.DB.BuiltInParameterGroup)) { }
    }

    [NodeName("Fill Pattern Targets")]
    [NodeCategory("archilab.Revit.Select")]
    [NodeDescription("Retrieve FillPatternTarget types.")]
    [IsDesignScriptCompatible]
    public class FillPatternTargetUi : CustomGenericEnumerationDropDown
    {
        public FillPatternTargetUi() : base("fillPatternTarget", typeof(Autodesk.Revit.DB.FillPatternTarget)) { }
    }

    [NodeName("Parameter Types")]
    [NodeCategory("archilab.Revit.Parameter")]
    [NodeDescription("Retrieve all available Parameter Types from Revit project.")]
    [IsDesignScriptCompatible]
    public class ParameterTypeUi : CustomGenericEnumerationDropDown
    {
        public ParameterTypeUi() : base("parameterType", typeof(Autodesk.Revit.DB.ParameterType)) { }
    }

    [NodeName("Print Range")]
    [NodeCategory("archilab.Revit.Printing")]
    [NodeDescription("Retrieve all available Print Ranges from Revit project.")]
    [IsDesignScriptCompatible]
    public class PrintRangeUi : CustomGenericEnumerationDropDown
    {
        public PrintRangeUi() : base("printRange", typeof(Autodesk.Revit.DB.PrintRange)) { }
    }

    [NodeName("Fit Direction Type")]
    [NodeCategory("archilab.Revit.Schedule")]
    [NodeDescription("Retrieve all available Fit Direction Types from Revit project.")]
    [IsDesignScriptCompatible]
    public class FitDirectionTypeUi : CustomGenericEnumerationDropDown
    {
        public FitDirectionTypeUi() : base("fitDirectionType", typeof(Autodesk.Revit.DB.FitDirectionType)) { }
    }

    [NodeName("Image Resolution")]
    [NodeCategory("archilab.Revit.Views")]
    [NodeDescription("Retrieve all available Image Resolutions from Revit project.")]
    [IsDesignScriptCompatible]
    public class ImageResolutionUi : CustomGenericEnumerationDropDown
    {
        public ImageResolutionUi() : base("imageResolution", typeof(Autodesk.Revit.DB.ImageResolution)) { }
    }

    [NodeName("Zoom Fit Type")]
    [NodeCategory("archilab.Revit.Views")]
    [NodeDescription("Retrieve all available Zoom Fit Types from Revit project.")]
    [IsDesignScriptCompatible]
    public class ZoomFitTypeUi : CustomGenericEnumerationDropDown
    {
        public ZoomFitTypeUi() : base("zoomFitType", typeof(Autodesk.Revit.DB.ZoomFitType)) { }
    }

    [NodeName("Image File Type")]
    [NodeCategory("archilab.Revit.Views")]
    [NodeDescription("Retrieve all available Image File Types from Revit project.")]
    [IsDesignScriptCompatible]
    public class ImageFileTypeUi : CustomGenericEnumerationDropDown
    {
        public ImageFileTypeUi() : base("imageFileType", typeof(Autodesk.Revit.DB.ImageFileType)) { }
    }

    [NodeName("Export Range")]
    [NodeCategory("archilab.Revit.Views")]
    [NodeDescription("Retrieve all available Export Ranges from Revit project.")]
    [IsDesignScriptCompatible]
    public class ExportRangeUi : CustomGenericEnumerationDropDown
    {
        public ExportRangeUi() : base("exportRange", typeof(Autodesk.Revit.DB.ExportRange)) { }
    }

    [NodeName("Print Settings")]
    [NodeCategory("archilab.Revit.Printing")]
    [NodeDescription("Retrieve all available Print Settings from Revit project.")]
    [IsDesignScriptCompatible]
    public class PrintSettingUi : CustomRevitElementDropDown
    {
        public PrintSettingUi() : base("printSetting", typeof(Autodesk.Revit.DB.PrintSetting)) { }
    }

    [NodeName("View Sets")]
    [NodeCategory("archilab.Revit.Printing")]
    [NodeDescription("Retrieve all available View Sets from Revit project.")]
    [IsDesignScriptCompatible]
    public class ViewSetUi : CustomRevitElementDropDown
    {
        public ViewSetUi() : base("viewSet", typeof(Autodesk.Revit.DB.ViewSheetSet)) { }
    }

    [NodeName("Schedule Heading Orientations")]
    [NodeCategory("archilab.Revit.Schedule")]
    [NodeDescription("Retrieve all available Heading Orientation values from Revit project.")]
    [IsDesignScriptCompatible]
    public class ScheduleHeadingOrientationUi : CustomGenericEnumerationDropDown
    {
        public ScheduleHeadingOrientationUi() : base("scheduleHeadingOrientation", typeof(Autodesk.Revit.DB.ScheduleHeadingOrientation)) { }
    }

    [NodeName("Schedule Horizontal Alignment")]
    [NodeCategory("archilab.Revit.Schedule")]
    [NodeDescription("Retrieve all available Horizontal Alignment Types from Revit project.")]
    [IsDesignScriptCompatible]
    public class ScheduleHorizontalAlignmentUi : CustomGenericEnumerationDropDown
    {
        public ScheduleHorizontalAlignmentUi() : base("scheduleHorizontalAlignment", typeof(Autodesk.Revit.DB.ScheduleHorizontalAlignment)) { }
    }

    [NodeName("Horizontal Alignment Style")]
    [NodeCategory("archilab.Revit.Schedule")]
    [NodeDescription("Retrieve all available Horizontal Alignment Styles from Revit project.")]
    [IsDesignScriptCompatible]
    public class HorizontalAlignmentStyleUi : CustomGenericEnumerationDropDown
    {
        public HorizontalAlignmentStyleUi() : base("horizontalAlignmentStyle", typeof(Autodesk.Revit.DB.HorizontalAlignmentStyle)) { }
    }


    [NodeName("Vertical Alignment Style")]
    [NodeCategory("archilab.Revit.Schedule")]
    [NodeDescription("Retrieve all available Vertical Alignment Styles from Revit project.")]
    [IsDesignScriptCompatible]
    public class VerticalAlignmentStyleUi : CustomGenericEnumerationDropDown
    {
        public VerticalAlignmentStyleUi() : base("verticalAlignmentStyle", typeof(Autodesk.Revit.DB.VerticalAlignmentStyle)) { }
    }

    [NodeName("Schedule Sort Order")]
    [NodeCategory("archilab.Revit.Schedule")]
    [NodeDescription("Retrieve all available Schedule Sort Order types from Revit project.")]
    [IsDesignScriptCompatible]
    public class ScheduleSortOrderUi : CustomGenericEnumerationDropDown
    {
        public ScheduleSortOrderUi() : base("scheduleSortOrder", typeof(Autodesk.Revit.DB.ScheduleSortOrder)) { }
    }

    [NodeName("Numeric Rule Evaluators")]
    [NodeCategory("archilab.Revit.Select")]
    [NodeDescription("Retrieve all available Numeric Rule Evaluators.")]
    [IsDesignScriptCompatible]
    public class FilterNumericRuleEvaluatorUi : RevitDropDownBase
    {
        private const string NoFamilyTypes = "No types were found.";

        public FilterNumericRuleEvaluatorUi() : base("evaluator") { }

        // Get Data Class that holds dictionary
        public static FilterNumericRuleEvaluator WTypes = new FilterNumericRuleEvaluator();

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

            var func = new Func<string, Autodesk.Revit.DB.FilterNumericRuleEvaluator>(FilterNumericRuleEvaluator.ByName);
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
        private const string NoFamilyTypes = "No types were found.";

        public FilterStringRuleEvaluatorUi() : base("evaluator") { }

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
        public FilterNumericValueRuleUi() : base("filterNumericValueRule", typeof(FilterNumericValueRule)) { }
    }

    [NodeName("Line Styles")]
    [NodeCategory("archilab.Revit.Views")]
    [NodeDescription("Retrieve all available Line Styles.")]
    [IsDesignScriptCompatible]
    public class LineStyleUi : RevitDropDownBase
    {
        private const string NoFamilyTypes = "No types were found.";

        public LineStyleUi() : base("lineStyle") { }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();
            var cat = Autodesk.Revit.DB.Category.GetCategory(DocumentManager.Instance.CurrentDBDocument, Autodesk.Revit.DB.BuiltInCategory.OST_Lines);
            var gsCat = cat.GetGraphicsStyle(Autodesk.Revit.DB.GraphicsStyleType.Projection).GraphicsStyleCategory.SubCategories;

            var lineStyles = new List<Autodesk.Revit.DB.GraphicsStyle>();
            foreach (Category c in gsCat)
            {
                lineStyles.Add(c.GetGraphicsStyle(Autodesk.Revit.DB.GraphicsStyleType.Projection));
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
                var graphicStyle = Items[SelectedIndex].Item as Autodesk.Revit.DB.GraphicsStyle;
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
        private const string NoFamilyTypes = "No types were found.";

        public ViewTemplatesUi() : base("viewTemplate") { }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var vtList = new Autodesk.Revit.DB.FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument)
                .OfClass(typeof(Autodesk.Revit.DB.View))
                .Cast<Autodesk.Revit.DB.View>()
                .Where(x => x.IsTemplate && x.ViewType != Autodesk.Revit.DB.ViewType.ThreeD)
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
                var view = Items[SelectedIndex].Item as Autodesk.Revit.DB.View;
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
        private const string NoFamilyTypes = "No types were found.";

        public WorksetUi() : base("workset") { }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var allWorksets = new Autodesk.Revit.DB.FilteredWorksetCollector(DocumentManager.Instance.CurrentDBDocument)
                .OfKind(Autodesk.Revit.DB.WorksetKind.UserWorkset)
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
                var workset = Items[SelectedIndex].Item as Autodesk.Revit.DB.Workset;
                if (workset == null)
                {
                    node = AstFactory.BuildNullNode();
                }
                else
                {
                    var idNode = AstFactory.BuildIntNode(workset.Id.IntegerValue);
                    node = AstFactory.BuildFunctionCall(
                        new Func<int, Workset>(archilab.Utilities.ElementSelector.GetWorksetById),
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

        public ViewTypesUi() : base("viewType") { }

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
        public PhaseFilterUi() : base("phaseFilter", typeof(Autodesk.Revit.DB.PhaseFilter)) { }
    }

    [NodeName("Workset Kind")]
    [NodeCategory("archilab.Revit.Workset")]
    [NodeDescription("Retrieve all available Workset kinds.")]
    [IsDesignScriptCompatible]
    public class WorksetKindUi : CustomGenericEnumerationDropDown
    {
        public WorksetKindUi() : base("kind", typeof(Autodesk.Revit.DB.WorksetKind)) { }
    }

    [NodeName("Workset Visibility")]
    [NodeCategory("archilab.Revit.Workset")]
    [NodeDescription("Retrieve all available Workset Visibility settings.")]
    [IsDesignScriptCompatible]
    public class WorksetVisibilityUi : CustomGenericEnumerationDropDown
    {
        public WorksetVisibilityUi() : base("visibility", typeof(Autodesk.Revit.DB.WorksetVisibility)) { }
    }

    [NodeName("Duplicate Options")]
    [NodeCategory("archilab.Revit.Views")]
    [NodeDescription("Retrieve all available View Duplication Options.")]
    [IsDesignScriptCompatible]
    public class DuplicateOptionsUi : CustomGenericEnumerationDropDown
    {
        public DuplicateOptionsUi() : base("options", typeof(Autodesk.Revit.DB.ViewDuplicateOption)) { }
    }

    [NodeName("Method Types")]
    [NodeCategory("archilab.Http.Http")]
    [NodeDescription("Retrieve all available Http Request types.")]
    [IsDesignScriptCompatible]
    public class HttpMethodType : CustomGenericEnumerationDropDown
    {
        public HttpMethodType() : base("methodType", typeof(Method)) { }
    }
}
