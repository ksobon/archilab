using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using archilab.Revit.Utils;
using Autodesk.DesignScript.Runtime;
using DynamoServices;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using Element = Revit.Elements.Element;
using Point = Autodesk.DesignScript.Geometry.Point;
using Surface = Autodesk.DesignScript.Geometry.Surface;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Wrapper class for Rooms.
    /// </summary>
    [RegisterForTrace]
    public class Room
    {
        internal Room()
        {
        }

        /// <summary>
        /// Room Name
        /// </summary>
        /// <param name="room">Room element.</param>
        /// <returns name="name">Name of the room.</returns>
        public static string Name(Element room)
        {
            if (room == null)
            {
                throw new ArgumentNullException(nameof(room));
            }

            var rm = (Autodesk.Revit.DB.SpatialElement)room.InternalElement;
            var name = rm.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.ROOM_NAME).AsString();
            return name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="boundaryLocation"></param>
        /// <returns></returns>
        [MultiReturn("Elements", "Curves")]
        public static Dictionary<string, object> Boundaries(Element room, string boundaryLocation = "Center")
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var bLoc = (Autodesk.Revit.DB.SpatialElementBoundaryLocation) Enum.Parse(
                typeof(Autodesk.Revit.DB.SpatialElementBoundaryLocation), boundaryLocation);
            var bOptions = new Autodesk.Revit.DB.SpatialElementBoundaryOptions
            {
                SpatialElementBoundaryLocation = bLoc
            };

            var rm = (Autodesk.Revit.DB.SpatialElement)room.InternalElement;
            var offset = rm.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.ROOM_LOWER_OFFSET).AsDouble();

            var calculator = new Autodesk.Revit.DB.SpatialElementGeometryCalculator(doc, bOptions);
            var roomGeo = calculator.CalculateSpatialElementGeometry(rm);
            var faces = new List<Autodesk.Revit.DB.Face>();
            foreach (Autodesk.Revit.DB.Face face in roomGeo.GetGeometry().Faces)
            {
                faces.Add(face);
            }
            var boundarySegments = rm.GetBoundarySegments(bOptions);
            var boundaryCurves = new List<List<Curve>>();
            var boundaryElements = new List<List<Element>>();
            foreach (var segments in boundarySegments)
            {
                var curvesList = new List<Curve>();
                var elementsList = new List<Element>();
                foreach (var segment in segments)
                {
                    var boundaryCurve = segment.GetCurve().Offset(offset);
                    curvesList.Add(boundaryCurve.ToProtoType());

                    var roomSeparationLine = doc.GetElement(segment.ElementId) as Autodesk.Revit.DB.ModelLine;
                    if (roomSeparationLine != null)
                    {
                        elementsList.Add(roomSeparationLine.ToDSType(true));
                        continue;
                    }

                    var face = FindFace(faces, roomGeo, boundaryCurve);
                    if (face == null)
                    {
                        elementsList.Add(null);
                        continue;
                    }

                    faces.Remove(face);

                    var bFace = roomGeo.GetBoundaryFaceInfo(face).FirstOrDefault();
                    if (bFace == null)
                    {
                        elementsList.Add(null);
                        continue;
                    }

                    elementsList.Add(doc.GetElement(bFace.SpatialBoundaryElement.HostElementId).ToDSType(true));
                }

                boundaryCurves.Add(curvesList);
                boundaryElements.Add(elementsList);
            }

            return new Dictionary<string, object>
            {
                {"Elements", boundaryElements},
                {"Curves", boundaryCurves}
            };
        }

        private static Autodesk.Revit.DB.Face FindFace(
            IEnumerable faces,
            Autodesk.Revit.DB.SpatialElementGeometryResults result,
            Autodesk.Revit.DB.Curve bCurve)
        {
            foreach (Autodesk.Revit.DB.Face f in faces)
            {
                var boundaryFaces = result.GetBoundaryFaceInfo(f).FirstOrDefault();
                if (boundaryFaces != null && (boundaryFaces.SubfaceType == Autodesk.Revit.DB.SubfaceType.Top ||
                                              boundaryFaces.SubfaceType == Autodesk.Revit.DB.SubfaceType.Bottom))
                {
                    continue; // face is either Top/Bottom so we can skip
                }

                var normal = f.ComputeNormal(new Autodesk.Revit.DB.UV(0.5, 0.5));
                if (normal.IsAlmostEqualTo(Autodesk.Revit.DB.XYZ.BasisZ) || normal.IsAlmostEqualTo(Autodesk.Revit.DB.XYZ.BasisZ.Negate()))
                    continue; // face is either Top/Bottom so we can skip

                var edges = f.GetEdgesAsCurveLoops().First(); // first loop is outer boundary
                foreach (var edge in edges)
                {
                    if (edge.OverlapsWithIn2D(bCurve))
                        return f;
                }
                //if (!edges.Any(x => x.OverlapsWithIn2D(bCurve))) // room's face might be off the floor/level above or offset. if XY matches, we are good.
                //    continue; // none of the edges of that face match our curve so we can skip

                //return f;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="boundaryLocation"></param>
        [MultiReturn("Boundary", "Holes", "Bottom", "Top")]
        public static Dictionary<string, object> Faces(Element room, string boundaryLocation = "Center")
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room));

            var bLoc = (Autodesk.Revit.DB.SpatialElementBoundaryLocation)Enum.Parse(typeof(Autodesk.Revit.DB.SpatialElementBoundaryLocation), boundaryLocation);
            var bOptions = new Autodesk.Revit.DB.SpatialElementBoundaryOptions
            {
                SpatialElementBoundaryLocation = bLoc
            };
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var rm = (Autodesk.Revit.DB.SpatialElement)room.InternalElement;
            var calculator = new Autodesk.Revit.DB.SpatialElementGeometryCalculator(doc, bOptions);
            var result = calculator.CalculateSpatialElementGeometry(rm);

            var segments = rm.GetBoundarySegments(bOptions);
            var outerBoundaryCurves = new List<Autodesk.Revit.DB.Curve>();
            var innerBoundaryCurves = new List<Autodesk.Revit.DB.Curve>();
            for (var i = 0; i < segments.Count; i++)
            {
                if (i == 0)
                {
                    outerBoundaryCurves = segments[i].Select(x => x.GetCurve()).ToList();
                }
                else
                {
                    innerBoundaryCurves.AddRange(segments[i].Select(x => x.GetCurve()));
                }
            }

            var bottom = new List<Surface>();
            var top = new List<Surface>();
            var boundary = new List<Surface>();
            var holes = new List<Surface>();

            var faces = result.GetGeometry().Faces;
            for (var i = 0; i < faces.Size; i++)
            {
                var face = faces.get_Item(i);
                var boundaryFaces = result.GetBoundaryFaceInfo(face).FirstOrDefault();

                if (boundaryFaces?.SubfaceType == Autodesk.Revit.DB.SubfaceType.Top)
                {
                    top.Add(face.ToProtoType().First());
                    continue;
                }

                if (boundaryFaces?.SubfaceType == Autodesk.Revit.DB.SubfaceType.Bottom)
                {
                    bottom.Add(face.ToProtoType().First());
                    continue;
                }

                var edges = face.GetEdgesAsCurveLoops().First(); // first loop is outer boundary, first curve is bottom edge
                var outerIndex = outerBoundaryCurves.FindIndex(x => edges.Any(y => y.OverlapsWith(x)));
                var innerIndex = innerBoundaryCurves.FindIndex(x => edges.Any(y => y.OverlapsWith(x)));

                if (outerIndex != -1)
                    boundary.Add(face.ToProtoType().First());

                if (innerIndex != -1)
                    holes.Add(face.ToProtoType().First());
            }

            return new Dictionary<string, object>
            {
                { "Boundary", boundary },
                { "Holes", holes },
                { "Bottom", bottom },
                { "Top", top } 
            };
        }
#if !Revit2018 && !Revit2019 && !Revit2020 && !Revit2021
        /// <summary>
        /// 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="boundaryLocation"></param>
        /// <returns></returns>
        public static double Height(Element room, string boundaryLocation = "Center")
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room));

            var bLoc = (Autodesk.Revit.DB.SpatialElementBoundaryLocation)Enum.Parse(typeof(Autodesk.Revit.DB.SpatialElementBoundaryLocation), boundaryLocation);
            var bOptions = new Autodesk.Revit.DB.SpatialElementBoundaryOptions
            {
                SpatialElementBoundaryLocation = bLoc
            };
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var rm = (Autodesk.Revit.DB.SpatialElement)room.InternalElement;
            var calculator = new Autodesk.Revit.DB.SpatialElementGeometryCalculator(doc, bOptions);
            var result = calculator.CalculateSpatialElementGeometry(rm);
            var geo = result.GetGeometry();
            var bb = geo.GetBoundingBox();
            var height = bb.Max.Z - bb.Min.Z;

            var fu = Autodesk.Revit.DB.UnitUtils.GetAllMeasurableSpecs()
                .FirstOrDefault(x => x.TypeId.StartsWith("autodesk.spec.aec:length"));
            var units = doc.GetUnits().GetFormatOptions(fu);

            return Autodesk.Revit.DB.UnitUtils.ConvertFromInternalUnits(height, units.GetUnitTypeId());
        }
#else
        /// <summary>
        /// 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="boundaryLocation"></param>
        /// <returns></returns>
        public static double Height(Element room, string boundaryLocation = "Center")
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room));

            var bLoc = (Autodesk.Revit.DB.SpatialElementBoundaryLocation)Enum.Parse(typeof(Autodesk.Revit.DB.SpatialElementBoundaryLocation), boundaryLocation);
            var bOptions = new Autodesk.Revit.DB.SpatialElementBoundaryOptions
            {
                SpatialElementBoundaryLocation = bLoc
            };
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var rm = (Autodesk.Revit.DB.SpatialElement)room.InternalElement;
            var calculator = new Autodesk.Revit.DB.SpatialElementGeometryCalculator(doc, bOptions);
            var result = calculator.CalculateSpatialElementGeometry(rm);
            var geo = result.GetGeometry();
            var bb = geo.GetBoundingBox();
            var height = bb.Max.Z - bb.Min.Z;

            var units = doc.GetUnits().GetFormatOptions(Autodesk.Revit.DB.UnitType.UT_Length);

            return Autodesk.Revit.DB.UnitUtils.ConvertFromInternalUnits(height, units.DisplayUnits);
        }
#endif
        /// <summary>
        /// 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="boundaryLocation"></param>
        /// <returns></returns>
        [MultiReturn("Boundary", "Holes", "GlazingRatios", "Height", "Walls", "Faces")]
        public static Dictionary<string, object> GlazingInfo(Element room, string boundaryLocation = "Center")
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var bLoc = (Autodesk.Revit.DB.SpatialElementBoundaryLocation)Enum.Parse(typeof(Autodesk.Revit.DB.SpatialElementBoundaryLocation), boundaryLocation);
            var bOptions = new Autodesk.Revit.DB.SpatialElementBoundaryOptions
            {
                SpatialElementBoundaryLocation = bLoc
            };

            var tolerance = doc.Application.ShortCurveTolerance;
            var e = (Autodesk.Revit.DB.SpatialElement)room.InternalElement;
            var calculator = new Autodesk.Revit.DB.SpatialElementGeometryCalculator(doc, bOptions);
            var roomGeo = calculator.CalculateSpatialElementGeometry(e);
            var geo = roomGeo.GetGeometry();
            var bb = geo.GetBoundingBox();
            var height = bb.Max.Z - bb.Min.Z;
            var segments = e.GetBoundarySegments(bOptions);
            var faces = roomGeo.GetGeometry().Faces;
            var offset = e.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.ROOM_LOWER_OFFSET).AsDouble();

            var boundary = new List<Point>();
            var holes = new List<List<Point>>();
            var windows = new List<double>();
            var walls = new List<List<Element>>();
            var foundFaces = new List<Surface>();
            for (var i = 0; i < segments.Count; i++)
            {
                if (i == 0) // outer boundary
                {
                    foreach (var bs in segments[i])
                    {
                        var boundaryCurve = bs.GetCurve().Offset(offset);
                        if (boundaryCurve.Length < 0.01)
                            continue; // Exclude tiny curves, they don't produce faces.

                        var face = FindFace(faces, roomGeo, boundaryCurve);
                        if (face == null)
                            continue; // Couldn't find a matching face. Not good.

                        GetGlazingInfo(face, doc, roomGeo, tolerance, out var unused, out var glazingAreas, out var boundingWalls);

                        var faceArea = boundaryCurve.Length * height;
                        var glazingArea = glazingAreas.Sum();
                        var glazingRatio = glazingArea / faceArea;

                        // (Konrad) Number of Boundary points in the list has to match number of Window Parameters.
                        var boundaryPts = GetPoints(boundaryCurve);

                        boundary.AddRange(boundaryPts);
                        windows.AddRange(boundaryPts.Select(x => glazingRatio));
                        walls.Add(boundingWalls);
                        foundFaces.Add(face.ToProtoType().First());
                    }

                    continue;
                }

                var hole = new List<Point>();
                foreach (var bs in segments[i])
                {
                    //TODO: Floor Holes need Glazing info processed.

                    var boundaryCurve = bs.GetCurve();
                    var segmentPts = GetPoints(boundaryCurve);

                    hole.AddRange(segmentPts);
                    windows.AddRange(segmentPts.Select(segmentPt => 0d));
                }

                holes.Add(hole);
            }

            return new Dictionary<string, object>
            {
                { "Boundary", boundary },
                { "Holes", holes },
                { "GlazingRatios", windows },
                { "Height", height },
                { "Walls", walls },
                { "Faces", foundFaces }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="glazingMaterials"></param>
        /// <param name="boundaryLocation"></param>
        /// <returns></returns>
        [MultiReturn("GlazingPoints", "GlazingRatios")]
        public static Dictionary<string, object> PointsOnSurface(
            Element room, 
            [DefaultArgument("Selection.Select.GetNull()")] List<Element> glazingMaterials, 
            string boundaryLocation = "Center")
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room));

            var materialIds = new List<Autodesk.Revit.DB.ElementId>();
            if (glazingMaterials != null && glazingMaterials.Any())
            {
                materialIds = glazingMaterials.Select(x => x.InternalElement.Id).ToList();
            }

            var bLoc = (Autodesk.Revit.DB.SpatialElementBoundaryLocation)Enum.Parse(typeof(Autodesk.Revit.DB.SpatialElementBoundaryLocation), boundaryLocation);
            var bOptions = new Autodesk.Revit.DB.SpatialElementBoundaryOptions
            {
                SpatialElementBoundaryLocation = bLoc
            };
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var tolerance = doc.Application.ShortCurveTolerance;
            var rm = (Autodesk.Revit.DB.SpatialElement)room.InternalElement;
            var calculator = new Autodesk.Revit.DB.SpatialElementGeometryCalculator(doc, bOptions);
            var result = calculator.CalculateSpatialElementGeometry(rm);
            var geo = result.GetGeometry();

            var bb = geo.GetBoundingBox();
            var height = bb.Max.Z - bb.Min.Z;
            //var units = doc.GetUnits().GetFormatOptions(Autodesk.Revit.DB.UnitType.UT_Length);
            //var convertedHeight = Autodesk.Revit.DB.UnitUtils.ConvertFromInternalUnits(height, units.DisplayUnits);

            var boundaryCurves = rm.GetBoundarySegments(bOptions).First().Select(x => x.GetCurve()).ToList();

            var glazingPoints = new List<List<List<Point>>>();
            var glazingRatios = new List<double>();

            for (var j = 1; j < geo.Faces.Size; j++) // skip 0 as that's the Floor.
            {
                var face = geo.Faces.get_Item(j);
                var bottomEdge = face.GetEdgesAsCurveLoops().First().First(); // first loop is outer boundary, first curve is bottom edge
                var index = boundaryCurves.FindIndex(x => x.OverlapsWith(bottomEdge));
                if (index == -1) continue; // could be inner face/roof

                var gPoints = new List<List<Point>>();
                var gAreas = new List<double>();

                if (!(face is Autodesk.Revit.DB.PlanarFace))
                {
                    glazingPoints[index] = gPoints;
                    continue; // skip non-planar faces
                }

                var boundaryFaces = result.GetBoundaryFaceInfo(face);
                foreach (var bFace in boundaryFaces)
                {
                    var bElement = doc.GetElement(bFace.SpatialBoundaryElement.HostElementId);
                    if (bElement is Autodesk.Revit.DB.Wall wall)
                    {
                        if (wall.WallType.Kind == Autodesk.Revit.DB.WallKind.Curtain)
                        {
                            var cGrid = wall.CurtainGrid;
                            var panels = cGrid.GetPanelIds().Select(x => doc.GetElement(x));
                            foreach (var panel in panels)
                            {
                                var mat = panel.GetMaterialIds(false);
                                if (!materialIds.Any() || !materialIds.Intersect(mat).Any()) continue;

                                var winPts = new List<Autodesk.Revit.DB.XYZ>();
                                using (var opt = new Autodesk.Revit.DB.Options())
                                {
                                    opt.IncludeNonVisibleObjects = true;
                                    using (var geom = panel.get_Geometry(opt))
                                    {
                                        ExtractPtsRecursively(geom, ref winPts);
                                    }
                                }

                                var onSurface = new HashSet<Autodesk.Revit.DB.XYZ>();
                                var onSurfaceUvs = new HashSet<Autodesk.Revit.DB.UV>();
                                foreach (var pt in winPts)
                                {
                                    var intResult = face.Project(pt);
                                    if (intResult == null) continue;

                                    if (onSurface.Add(intResult.XYZPoint))
                                    {
                                        onSurfaceUvs.Add(intResult.UVPoint.Negate());
                                    }
                                }

                                if (GetHull(onSurface.ToList(), onSurfaceUvs.ToList(), tolerance, out var hPts, out var hUvs))
                                {
                                    var outerEdges = face.GetEdgesAsCurveLoops().First();
                                    foreach (var edge in outerEdges)
                                    {
                                        for (var i = 0; i < hPts.Count; i++)
                                        {
                                            var pt = hPts[i];
                                            if (edge.Distance(pt) >= 0.01) continue;

                                            var direction = (edge.GetEndPoint(1) - edge.GetEndPoint(0)).Normalize();
                                            var perpendicular = face.ComputeNormal(new Autodesk.Revit.DB.UV(0.5, 0.5)).CrossProduct(direction);
                                            var offset = 0.1 * perpendicular;
                                            var offsetPt = pt + offset;

                                            hPts[i] = offsetPt;
                                        }
                                    }

                                    gAreas.Add(PolygonArea(hUvs));
                                    gPoints.Add(hPts.Select(x => x.ToPoint()).ToList());
                                }
                            }
                        }

                        var inserts = wall.FindInserts(true, false, true, true).Select(x => doc.GetElement(x));
                        foreach (var insert in inserts)
                        {
                            if (insert.Category.Id.IntegerValue == Autodesk.Revit.DB.BuiltInCategory.OST_Windows.GetHashCode())
                            {
                                // (Konrad) We have a Window.
                                var winPts = new List<Autodesk.Revit.DB.XYZ>();
                                using (var opt = new Autodesk.Revit.DB.Options())
                                {
                                    opt.IncludeNonVisibleObjects = true;
                                    using (var geom = insert.get_Geometry(opt))
                                    {
                                        ExtractPtsRecursively(geom, ref winPts);
                                    }
                                }

                                var onSurface = new HashSet<Autodesk.Revit.DB.XYZ>();
                                var onSurfaceUvs = new HashSet<Autodesk.Revit.DB.UV>();
                                foreach (var pt in winPts)
                                {
                                    var intResult = face.Project(pt);
                                    if (intResult == null) continue;

                                    if (onSurface.Add(intResult.XYZPoint))
                                    {
                                        onSurfaceUvs.Add(intResult.UVPoint.Negate());
                                    }
                                }

                                if (GetHull(onSurface.ToList(), onSurfaceUvs.ToList(), tolerance, out var hPts, out var hUvs))
                                {
                                    var winArea = GetWindowArea(insert);
                                    var hullArea = PolygonArea(hUvs);

                                    if (hullArea > winArea * 0.5)
                                    {
                                        var outerEdges = face.GetEdgesAsCurveLoops().First();
                                        foreach (var edge in outerEdges)
                                        {
                                            for (var i = 0; i < hPts.Count; i++)
                                            {
                                                var pt = hPts[i];
                                                if (edge.Distance(pt) >= 0.01) continue;

                                                var direction = (edge.GetEndPoint(1) - edge.GetEndPoint(0)).Normalize();
                                                var perpendicular = face.ComputeNormal(new Autodesk.Revit.DB.UV(0.5, 0.5)).CrossProduct(direction);
                                                var offset = 0.03 * perpendicular;
                                                var offsetPt = pt + offset;

                                                hPts[i] = offsetPt;
                                            }
                                        }

                                        gAreas.Add(PolygonArea(hUvs));
                                        gPoints.Add(hPts.Select(x => x.ToPoint()).ToList());
                                    }
                                }
                            }
                        }
                    }
                    else if (bElement is Autodesk.Revit.DB.RoofBase roof)
                    {
                        var inserts = roof.FindInserts(true, false, true, true).Select(x => doc.GetElement(x));
                        foreach (var insert in inserts)
                        {
                            if (insert.Category.Id.IntegerValue == Autodesk.Revit.DB.BuiltInCategory.OST_Windows.GetHashCode())
                            {
                                // (Konrad) We have a Window.
                                var winPts = new List<Autodesk.Revit.DB.XYZ>();
                                using (var opt = new Autodesk.Revit.DB.Options())
                                {
                                    opt.IncludeNonVisibleObjects = true;
                                    using (var geom = insert.get_Geometry(opt))
                                    {
                                        ExtractPtsRecursively(geom, ref winPts);
                                    }
                                }

                                var onSurface = new HashSet<Autodesk.Revit.DB.XYZ>();
                                var onSurfaceUvs = new HashSet<Autodesk.Revit.DB.UV>();
                                foreach (var pt in winPts)
                                {
                                    var intResult = face.Project(pt);
                                    if (intResult == null) continue;

                                    if (onSurface.Add(intResult.XYZPoint))
                                    {
                                        onSurfaceUvs.Add(intResult.UVPoint.Negate());
                                    }
                                }

                                if (GetHull(onSurface.ToList(), onSurfaceUvs.ToList(), tolerance, out var hPts, out var hUvs))
                                {
                                    var winArea = GetWindowArea(insert);
                                    var hullArea = PolygonArea(hUvs);

                                    if (hullArea > winArea * 0.25)
                                    {
                                        gAreas.Add(hullArea);
                                        gPoints.Add(hPts.Select(x => x.ToPoint()).ToList());
                                    }
                                }
                            }
                        }
                    }
                }

                var curve = face.GetEdgesAsCurveLoops().FirstOrDefault()?.FirstOrDefault();
                if (curve == null) continue;

                var faceArea = curve.Length * height;
                glazingPoints[index] = gPoints;
                glazingRatios[index] = gAreas.Sum() / faceArea;
            }

            return new Dictionary<string, object>
            {
                { "GlazingPoints", glazingPoints },
                { "GlazingRatios", glazingRatios }
            };
        }

        #region Utilities

        private static List<Point> GetPoints(Autodesk.Revit.DB.Curve curve)
        {
            var curves = new List<Point>();
            switch (curve)
            {
                case Autodesk.Revit.DB.Line line:
                    curves.Add(line.GetEndPoint(0).ToPoint());
                    break;
                case Autodesk.Revit.DB.Arc arc:
                    curves.Add(arc.Evaluate(0, true).ToPoint());
                    curves.Add(arc.Evaluate(0.25, true).ToPoint());
                    curves.Add(arc.Evaluate(0.5, true).ToPoint());
                    curves.Add(arc.Evaluate(0.75, true).ToPoint());
                    break;
                case Autodesk.Revit.DB.CylindricalHelix unused:
                case Autodesk.Revit.DB.Ellipse unused1:
                case Autodesk.Revit.DB.HermiteSpline unused2:
                case Autodesk.Revit.DB.NurbSpline unused3:
                    break;
            }

            return curves;
        }

        //private static Autodesk.Revit.DB.Face FindFace(
        //    IEnumerable faces, 
        //    Autodesk.Revit.DB.SpatialElementGeometryResults result, 
        //    Autodesk.Revit.DB.Curve bCurve)
        //{
        //    foreach (Autodesk.Revit.DB.Face f in faces)
        //    {
        //        var boundaryFaces = result.GetBoundaryFaceInfo(f).FirstOrDefault();
        //        if (boundaryFaces != null && (boundaryFaces.SubfaceType == Autodesk.Revit.DB.SubfaceType.Top ||
        //                                      boundaryFaces.SubfaceType == Autodesk.Revit.DB.SubfaceType.Bottom))
        //        {
        //            continue; // face is either Top/Bottom so we can skip
        //        }

        //        var normal = f.ComputeNormal(new Autodesk.Revit.DB.UV(0.5, 0.5));
        //        if (normal.IsAlmostEqualTo(Autodesk.Revit.DB.XYZ.BasisZ) || normal.IsAlmostEqualTo(Autodesk.Revit.DB.XYZ.BasisZ.Negate()))
        //            continue; // face is either Top/Bottom so we can skip

        //        var edges = f.GetEdgesAsCurveLoops().First(); // first loop is outer boundary
        //        if (!edges.Any(x => x.OverlapsWithIn2D(bCurve))) // room's face might be off the floor/level above or offset. if XY matches, we are good.
        //            continue; // none of the edges of that face match our curve so we can skip

        //        return f;
        //    }

        //    return null;
        //}

        private static void GetGlazingInfo(
            Autodesk.Revit.DB.Face face,
            Autodesk.Revit.DB.Document doc,
            Autodesk.Revit.DB.SpatialElementGeometryResults result,
            double tolerance,
            out List<List<Autodesk.Revit.DB.XYZ>> glazingPoints,
            out List<double> glazingAreas,
            out List<Element> walls)
        {
            glazingPoints = new List<List<Autodesk.Revit.DB.XYZ>>();
            glazingAreas = new List<double>();
            walls = new List<Element>();

            if (!(face is Autodesk.Revit.DB.PlanarFace))
                return;

            var boundaryFaces = result.GetBoundaryFaceInfo(face);
            foreach (var bFace in boundaryFaces)
            {
                var bElement = doc.GetElement(bFace.SpatialBoundaryElement.HostElementId);
                if (bElement is Autodesk.Revit.DB.Wall wall)
                {
                    walls.Add(bElement.ToDSType(true));

                    if (wall.WallType.Kind == Autodesk.Revit.DB.WallKind.Curtain)
                    {
                        GetGlazingFromCurtainWall(wall, face, tolerance, ref glazingPoints, ref glazingAreas);
                    }
                    else
                    {
                        GetGlazingFromWindows(wall, face, tolerance, ref glazingPoints, ref glazingAreas);
                    }
                }
            }
        }

        private static void GetGlazingFromWindows(
            Autodesk.Revit.DB.Wall wall, 
            Autodesk.Revit.DB.Face face, 
            double tolerance, 
            ref List<List<Autodesk.Revit.DB.XYZ>> glazingPts, 
            ref List<double> glazingAreas)
        {
            var doc = wall.Document;
            var inserts = wall.FindInserts(true, false, true, true).Select(doc.GetElement);
            foreach (var insert in inserts)
            {
                if (insert.Category.Id.IntegerValue == Autodesk.Revit.DB.BuiltInCategory.OST_Windows.GetHashCode())
                {
                    var winPts = GetGeometryPoints(insert);
                    if (!GetPointsOnFace(face, winPts, out var ptsOnFace, out var uvsOnFace)) continue;
                    if (!GetHull(ptsOnFace, uvsOnFace, tolerance, out var hPts, out var hUvs)) continue;

                    var winArea = GetWindowArea(insert);
                    var hullArea = PolygonArea(hUvs);
                    if (hullArea < winArea * 0.5) continue;

                    var outerEdges = face.GetEdgesAsCurveLoops().First();
                    foreach (var edge in outerEdges)
                    {
                        for (var i = 0; i < hPts.Count; i++)
                        {
                            var pt = hPts[i];
                            if (edge.Distance(pt) >= 0.01) continue;

                            var direction = (edge.GetEndPoint(1) - edge.GetEndPoint(0)).Normalize();
                            var perpendicular = face.ComputeNormal(new Autodesk.Revit.DB.UV(0.5, 0.5)).CrossProduct(direction);
                            var offset = 0.1 * perpendicular;
                            var offsetPt = pt + offset;

                            hPts[i] = offsetPt;
                        }
                    }

                    glazingAreas.Add(PolygonArea(hUvs));
                    glazingPts.Add(hPts);
                }
            }
        }

        private static void GetGlazingFromCurtainWall(
            Autodesk.Revit.DB.Wall wall, 
            Autodesk.Revit.DB.Face face, 
            double tolerance, 
            ref List<List<Autodesk.Revit.DB.XYZ>> glazingPts, 
            ref List<double> glazingAreas)
        {
            var doc = wall.Document;
            var cGrid = wall.CurtainGrid;
            var panels = cGrid.GetPanelIds().Select(x => doc.GetElement(x));

            foreach (var panel in panels)
            {
                var points = GetGeometryPoints(panel);
                if (!GetPointsOnFace(face, points, out var ptsOnFace, out var uvsOnFace)) continue;
                if (!GetHull(ptsOnFace, uvsOnFace, tolerance, out var hPts, out var hUvs)) continue;

                var outerEdges = face.GetEdgesAsCurveLoops().First();
                foreach (var edge in outerEdges)
                {
                    for (var i = 0; i < hPts.Count; i++)
                    {
                        var pt = hPts[i];
                        if (edge.Distance(pt) >= 0.01) continue;

                        var direction = (edge.GetEndPoint(1) - edge.GetEndPoint(0)).Normalize();
                        var perpendicular = face.ComputeNormal(new Autodesk.Revit.DB.UV(0.5, 0.5)).CrossProduct(direction);
                        var offset = 0.1 * perpendicular;
                        var offsetPt = pt + offset;

                        hPts[i] = offsetPt;
                    }
                }

                glazingAreas.Add(PolygonArea(hUvs));
                glazingPts.Add(hPts);
            }
        }

        private static List<Autodesk.Revit.DB.XYZ> GetGeometryPoints(Autodesk.Revit.DB.Element e)
        {
            var pts = new List<Autodesk.Revit.DB.XYZ>();
            using (var opt = new Autodesk.Revit.DB.Options())
            {
                opt.IncludeNonVisibleObjects = true;
                using (var geom = e.get_Geometry(opt))
                {
                    ExtractPtsRecursively(geom, ref pts);
                }
            }

            return pts;
        }

        private static bool GetPointsOnFace(
            Autodesk.Revit.DB.Face face, 
            List<Autodesk.Revit.DB.XYZ> pts, 
            out List<Autodesk.Revit.DB.XYZ> ptsOnFace, 
            out List<Autodesk.Revit.DB.UV> uvsOnFace)
        {
            var onFace = new HashSet<Autodesk.Revit.DB.XYZ>();
            var onFaceUvs = new HashSet<Autodesk.Revit.DB.UV>();
            foreach (var pt in pts)
            {
                var intResult = face.Project(pt);
                if (intResult == null) continue;

                if (onFace.Add(intResult.XYZPoint))
                    onFaceUvs.Add(intResult.UVPoint.Negate());
            }

            ptsOnFace = onFace.ToList();
            uvsOnFace = onFaceUvs.ToList();

            return ptsOnFace.Any() && uvsOnFace.Any();
        }

        private static double GetWindowArea(Autodesk.Revit.DB.Element insert)
        {
            var winType = (Autodesk.Revit.DB.FamilySymbol)insert.Document.GetElement(insert.GetTypeId());

            var furnitureWidthInstance = insert.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FURNITURE_WIDTH)?.AsDouble() ?? 0;
            var furnitureWidthType = winType.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FURNITURE_WIDTH)?.AsDouble() ?? 0;
            var furnWidth = furnitureWidthInstance > 0 ? furnitureWidthInstance : furnitureWidthType;
            var familyWidthInstance = insert.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FAMILY_WIDTH_PARAM)?.AsDouble() ?? 0;
            var familyWidthType = winType.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FAMILY_WIDTH_PARAM)?.AsDouble() ?? 0;
            var famWidth = familyWidthInstance > 0 ? familyWidthInstance : familyWidthType;
            var roughWidthInstance = insert.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FAMILY_ROUGH_WIDTH_PARAM)?.AsDouble() ?? 0;
            var roughWidthType = winType.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FAMILY_ROUGH_WIDTH_PARAM)?.AsDouble() ?? 0;
            var rWidth = roughWidthInstance > 0 ? roughWidthInstance : roughWidthType;
            var width = rWidth > 0 ? rWidth : famWidth > 0 ? famWidth : furnWidth;

            var furnitureHeightInstance = insert.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FURNITURE_HEIGHT)?.AsDouble() ?? 0;
            var furnitureHeightType = winType.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FURNITURE_HEIGHT)?.AsDouble() ?? 0;
            var furnHeight = furnitureHeightInstance > 0 ? furnitureHeightInstance : furnitureHeightType;
            var familyHeightInstance = insert.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FAMILY_HEIGHT_PARAM)?.AsDouble() ?? 0;
            var familyHeightType = winType.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FAMILY_HEIGHT_PARAM)?.AsDouble() ?? 0;
            var famHeight = familyHeightInstance > 0 ? familyHeightInstance : familyHeightType;
            var roughHeightInstance = insert.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FAMILY_ROUGH_HEIGHT_PARAM)?.AsDouble() ?? 0;
            var roughHeightType = winType.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FAMILY_ROUGH_HEIGHT_PARAM)?.AsDouble() ?? 0;
            var rHeight = roughWidthInstance > 0 ? roughHeightInstance : roughHeightType;
            var height = rHeight > 0 ? rHeight : famHeight > 0 ? famHeight : furnHeight;

            var winArea = width * height;

            return winArea;
        }

        private static double PolygonArea(IList<Autodesk.Revit.DB.UV> polygon)
        {
            int i, j;
            double area = 0;

            for (i = 0; i < polygon.Count; i++)
            {
                j = (i + 1) % polygon.Count;

                area += polygon[i].U * polygon[j].V;
                area -= polygon[i].V * polygon[j].U;
            }

            area /= 2;
            return (area < 0 ? -area : area);
        }

        private static bool GetHull(
            List<Autodesk.Revit.DB.XYZ> pts,
            List<Autodesk.Revit.DB.UV> uvs,
            double tolerance,
            out List<Autodesk.Revit.DB.XYZ> hullPts,
            out List<Autodesk.Revit.DB.UV> hullUvs)
        {
            hullPts = new List<Autodesk.Revit.DB.XYZ>();
            hullUvs = new List<Autodesk.Revit.DB.UV>();

            if (!pts.Any() || !uvs.Any())
                return false;

            try
            {
                var hullPoints = uvs.Select(x => new HullPoint(x.U, x.V)).ToList();
                var hull = ConvexHull.MakeHull(hullPoints);

                var hUvs = hull.Select(x => new Autodesk.Revit.DB.UV(x.x, x.y)).ToList();
                var hPts = hUvs.Select(x => pts[uvs.FindIndex(y => y.IsAlmostEqualTo(x))]).ToList();

                var indexToRemove = -1;

                Restart:

                if (indexToRemove != -1)
                {
                    hPts.RemoveAt(indexToRemove);
                    hUvs.RemoveAt(indexToRemove);

                    // ReSharper disable once RedundantAssignment
                    indexToRemove = -1;
                }

                for (var i = 0; i < hPts.Count; i++)
                {
                    var start = hPts[i];
                    Autodesk.Revit.DB.XYZ middle;
                    Autodesk.Revit.DB.XYZ end;
                    int middleIndex;
                    if (i + 2 == hPts.Count)
                    {
                        middle = hPts[i + 1];
                        middleIndex = i + 1;
                        end = hPts[0];
                    }
                    else if (i + 1 == hPts.Count)
                    {
                        middle = hPts[0];
                        middleIndex = 0;
                        end = hPts[1];
                    }
                    else
                    {
                        middle = hPts[i + 1];
                        middleIndex = i + 1;
                        end = hPts[i + 2];
                    }

                    if (start.DistanceTo(end) < tolerance) continue;

                    var line = Autodesk.Revit.DB.Line.CreateBound(start, end);
                    var intResult = line.Project(middle);
                    if (intResult.Distance > 0.01) continue;

                    indexToRemove = middleIndex;
                    goto Restart;
                }

                hullPts = hPts;
                hullUvs = hUvs;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void ExtractPtsRecursively(
            Autodesk.Revit.DB.GeometryElement geo, 
            ref List<Autodesk.Revit.DB.XYZ> pts, 
            bool includeLines = false)
        {
            foreach (var g in geo)
            {
                var instGeo = g as Autodesk.Revit.DB.GeometryInstance;
                if (instGeo != null)
                {
                    ExtractPtsRecursively(instGeo.GetInstanceGeometry(), ref pts, includeLines);
                    continue;
                }

                var solidGeo = g as Autodesk.Revit.DB.Solid;
                if (solidGeo != null)
                {
                    foreach (Autodesk.Revit.DB.Face f in solidGeo.Faces)
                    {
                        ProcessFace(f, ref pts);
                    }
                    continue;
                }

                var faceGeo = g as Autodesk.Revit.DB.Face;
                if (faceGeo != null) ProcessFace(faceGeo, ref pts);

                var meshGeo = g as Autodesk.Revit.DB.Mesh;
                if (meshGeo != null) pts.AddRange(meshGeo.Vertices);

                if (!includeLines) continue;

                var lineGeo = g as Autodesk.Revit.DB.Curve;
                if (lineGeo != null && lineGeo.IsBound)
                    pts.AddRange(new List<Autodesk.Revit.DB.XYZ> { lineGeo.GetEndPoint(0), lineGeo.GetEndPoint(1) });
            }
        }

        private static readonly double[] _params = { 0d, 0.2, 0.4, 0.6, 0.8 };

        private static void ProcessFace(Autodesk.Revit.DB.Face f, ref List<Autodesk.Revit.DB.XYZ> pts)
        {
            foreach (Autodesk.Revit.DB.EdgeArray edges in f.EdgeLoops)
            {
                foreach (Autodesk.Revit.DB.Edge e in edges)
                {
                    pts.AddRange(_params.Select(p => e.Evaluate(p)));
                }
            }
        }

        #endregion
    }
}
