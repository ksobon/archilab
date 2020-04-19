using System;
using System.Collections.Generic;
using System.Linq;
using archilab.Revit.Utils;
using Autodesk.DesignScript.Runtime;
using DynamoServices;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
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
        public static List<Surface> Faces(Element room, string boundaryLocation = "Center")
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

            return (from Autodesk.Revit.DB.Face face in result.GetGeometry().Faces select face.ToProtoType().First()).ToList();
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="room"></param>
        ///// <returns></returns>
        //[MultiReturn("Pts", "UVs", "Surfaces", "Lines")]
        //public static Dictionary<string, object> PointsOnSurface(Element room)
        //{
        //    if (room == null)
        //        throw new ArgumentNullException(nameof(room));

        //    var bOptions = new Autodesk.Revit.DB.SpatialElementBoundaryOptions
        //    {
        //        SpatialElementBoundaryLocation = Autodesk.Revit.DB.SpatialElementBoundaryLocation.Center
        //    };
        //    var doc = DocumentManager.Instance.CurrentDBDocument;
        //    var rm = (Autodesk.Revit.DB.SpatialElement)room.InternalElement;
        //    var calculator = new Autodesk.Revit.DB.SpatialElementGeometryCalculator(doc, bOptions);
        //    var result = calculator.CalculateSpatialElementGeometry(rm);

        //    var pts = new List<List<PointWrapper>>();
        //    var uvs = new List<List<Autodesk.Revit.DB.UV>>();
        //    var dSurfaces = new List<Surface>();
        //    var dLines = new List<Autodesk.DesignScript.Geometry.Curve>();
        //    foreach (Autodesk.Revit.DB.Face face in result.GetGeometry().Faces)
        //    {
        //        var planes = new List<Autodesk.Revit.DB.Plane>();
        //        var surfaces = new List<Autodesk.Revit.DB.Surface>();
        //        var lines = new List<Autodesk.Revit.DB.Line>();

        //        if (face is Autodesk.Revit.DB.CylindricalFace cFace)
        //        {
        //            var bottomPoints = new List<Autodesk.Revit.DB.XYZ>();
        //            var topPoints = new List<Autodesk.Revit.DB.XYZ>();

        //            foreach (var cLoop in cFace.GetEdgesAsCurveLoops())
        //            {
        //                var index = 0;
        //                foreach (var curve in cLoop)
        //                {
        //                    if (index == 0)
        //                    {
        //                        bottomPoints = new List<Autodesk.Revit.DB.XYZ>
        //                        {
        //                            curve.Evaluate(0, true),
        //                            curve.Evaluate(0.25, true),
        //                            curve.Evaluate(0.50, true),
        //                            curve.Evaluate(0.75, true),
        //                            curve.Evaluate(1, true),
        //                        };
        //                    }
        //                    else if (index == 2)
        //                    {
        //                        topPoints = new List<Autodesk.Revit.DB.XYZ>
        //                        {
        //                            curve.Evaluate(1, true),
        //                            curve.Evaluate(0.75, true),
        //                            curve.Evaluate(0.50, true),
        //                            curve.Evaluate(0.25, true),
        //                            curve.Evaluate(0, true),
        //                        };
        //                    }

        //                    index++;
        //                }

        //                break;
        //            }

        //            if (!bottomPoints.Any() || !topPoints.Any()) continue;
        //            for (var i = 0; i < bottomPoints.Count; i++)
        //            {
        //                if (i == bottomPoints.Count - 1) continue;

        //                var locLine = Autodesk.Revit.DB.Line.CreateBound(bottomPoints[i], bottomPoints[i + 1]);
        //                lines.Add(locLine);



        //                var plane = Autodesk.Revit.DB.Plane.CreateByThreePoints(bottomPoints[i], bottomPoints[i + 1], topPoints[i + 1]);
        //                var bottomCurve = Autodesk.Revit.DB.Line.CreateBound(bottomPoints[i], bottomPoints[i + 1]);
        //                var topCurve = Autodesk.Revit.DB.Line.CreateBound(topPoints[i], topPoints[i + 1]);
        //                //var surface = Autodesk.Revit.DB.RuledSurface.Create(bottomCurve, topCurve);
        //                var dSurface = Surface.ByRuledLoft(new List<Line>
        //                    {topCurve.ToProtoType() as Line, bottomCurve.ToProtoType() as Line});
        //                planes.Add(plane);
        //                //surfaces.Add(surface);
        //                dSurfaces.Add(dSurface);
        //            }
        //        }

        //        var boundaryFaces = result.GetBoundaryFaceInfo(face);
        //        foreach (var bFace in boundaryFaces)
        //        {
        //            var bElement = doc.GetElement(bFace.SpatialBoundaryElement.HostElementId);
        //            if (bElement is Autodesk.Revit.DB.Wall wall)
        //            {
        //                var inserts = wall.FindInserts(true, false, true, true).Select(x => doc.GetElement(x));

        //                foreach (var insert in inserts)
        //                {
        //                    if (insert.Category.Id.IntegerValue ==
        //                        Autodesk.Revit.DB.BuiltInCategory.OST_Windows.GetHashCode())
        //                    {
        //                        // (Konrad) We have a Window.
        //                        var winPts = new List<Autodesk.Revit.DB.XYZ>();
        //                        using (var opt = new Autodesk.Revit.DB.Options())
        //                        {
        //                            opt.IncludeNonVisibleObjects = true;
        //                            using (var geom = insert.get_Geometry(opt))
        //                            {
        //                                ExtractPtsRecursively(geom, ref winPts);
        //                            }
        //                        }

                                
        //                        if (lines.Any())
        //                        {
        //                            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

                                    

        //                            for (var i = 0; i < lines.Count; i++)
        //                            {
        //                                var locLine = lines[i];
        //                                var plane = planes[i];

        //                                var type = doc.GetElement(wall.GetTypeId());
        //                                var width = type
        //                                    .get_Parameter(Autodesk.Revit.DB.BuiltInParameter.WALL_ATTR_WIDTH_PARAM)
        //                                    .AsDouble() / 2;

        //                                //var start = locLine.GetEndPoint(0) + (wall.Orientation.Normalize() * width);
        //                                //var end = locLine.GetEndPoint(1) + (wall.Orientation.Normalize() * width);
        //                                //var line = Autodesk.Revit.DB.Line.CreateBound(start, end);
        //                                //dLines.Add(line.ToProtoType());
        //                                var offsetLine = locLine.CreateOffset(width, plane.Normal.Negate());
        //                                dLines.Add(offsetLine.ToProtoType(true));
        //                                var offset = wall
        //                                    .get_Parameter(Autodesk.Revit.DB.BuiltInParameter.WALL_BASE_OFFSET)
        //                                    .AsDouble();
        //                                var height = wall
        //                                    .get_Parameter(Autodesk.Revit.DB.BuiltInParameter.WALL_USER_HEIGHT_PARAM)
        //                                    .AsDouble();
        //                                var level = wall.LevelId;

                                       
        //                                var newWall = Autodesk.Revit.DB.Wall.Create(doc, offsetLine, type.Id, level, height, offset, true, true);
        //                                //Autodesk.Revit.DB.ElementTransformUtils.MoveElement(doc, newWall.Id,
        //                                //        wall.Orientation * width);
        //                                //newWall.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.WALL_KEY_REF_PARAM).Set(2);
        //                            }

        //                            TransactionManager.Instance.TransactionTaskDone();

                                    
        //                            //for (var i = 0; i < planes.Count; i++)
        //                            //{
        //                            //    var plane = planes[i];
        //                            //    var surface = surfaces[i];
        //                            //    var dSurface = dSurfaces[i];

        //                            //    var onSurface = new HashSet<PointWrapper>();
        //                            //    var onSurfaceUvs = new HashSet<Autodesk.Revit.DB.UV>();

        //                            //    foreach (var pt in winPts)
        //                            //    {
        //                            //        try
        //                            //        {
        //                            //            var intersection = dSurface.ProjectInputOnto(pt.ToPoint(), dSurface.NormalAtParameter()).FirstOrDefault() as Point;

        //                            //            surface.Project(pt, out var uv1, out var unused1);
        //                            //            if (uv1 == null) continue;

        //                            //            plane.Project(pt, out var uv, out var unused);
        //                            //            if (uv == null) continue;

        //                            //            if (onSurface.Add(new PointWrapper(intersection.ToXyz())))
        //                            //            {
        //                            //                onSurfaceUvs.Add(uv);
        //                            //            }
        //                            //        }
        //                            //        catch (Exception e)
        //                            //        {
        //                            //            // ignored
        //                            //        }
        //                            //    }

        //                            //    pts.Add(onSurface.ToList());
        //                            //    uvs.Add(onSurfaceUvs.ToList());

        //                            //    break;
        //                            //}
        //                        }
        //                        else
        //                        {
        //                            continue;

        //                            var onSurface = new HashSet<PointWrapper>();
        //                            var onSurfaceUvs = new HashSet<Autodesk.Revit.DB.UV>();

        //                            foreach (var pt in winPts)
        //                            {
        //                                var intResult = face.Project(pt);
        //                                if (intResult == null) continue;

        //                                var pt1 = face.Evaluate(new Autodesk.Revit.DB.UV(0, 0));
        //                                var pt2 = face.Evaluate(new Autodesk.Revit.DB.UV(1, 0));
        //                                var pt3 = face.Evaluate(new Autodesk.Revit.DB.UV(1, 1));
        //                                var plane = Autodesk.Revit.DB.Plane.CreateByThreePoints(pt1, pt2, pt3);

        //                                plane.Project(pt, out var uv, out var unused);
        //                                if (uv == null) continue;

        //                                if (onSurface.Add(new PointWrapper(intResult.XYZPoint)))
        //                                {
        //                                    onSurfaceUvs.Add(uv);
        //                                }
        //                            }

        //                            pts.Add(onSurface.ToList());
        //                            uvs.Add(onSurfaceUvs.ToList());
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    var ptsOutput = pts.Select(x => x.Select(y => y.ToXyz().ToPoint()).ToList()).ToList();
        //    var uvsOutput = uvs.Select(x => x.Select(y => Point.ByCoordinates(y.U, y.V)).ToList()).ToList();
        //    return new Dictionary<string, object>
        //    {
        //        { "Pts", ptsOutput },
        //        { "UVs", uvsOutput },
        //        { "Surfaces", dSurfaces},
        //        { "Lines", dLines}
        //    };
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="boundaryLocation"></param>
        /// <returns></returns>
        [MultiReturn("Pts", "EdgePts")]
        public static Dictionary<string, object> PointsOnSurface(Element room, string boundaryLocation = "Center")
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room));

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

            var boundarySegments = rm.GetBoundarySegments(bOptions).First().ToList();


            var hullPoints = new List<List<Point>>();
            var edgePoints = new List<List<Point>>();
            foreach (Autodesk.Revit.DB.Face face in result.GetGeometry().Faces)
            {
                if (!(face is Autodesk.Revit.DB.PlanarFace))
                    continue; // skip non-planar faces

                

                var boundaryFaces = result.GetBoundaryFaceInfo(face);
                if (!boundaryFaces.Any())
                {
                    // (Konrad) If there is no information about the elements that generated this face,
                    // it's likely that it was a Model Line ie. Room Separation Line that created it.
                    // Just to be safe here, we can create a Bounding Box around this face, and check,
                    // if any Curtain Wall Panels overlap with it.

                    //TODO: Get BB and find out if Curtain Wall Panels intersect it.
                    //var outerBoundary = face.GetEdgesAsCurveLoops().First();
                    //foreach (var curve in outerBoundary)
                    //{
                    //    var index = boundarySegments.FindIndex(x => x.GetCurve().OverlapsWith(curve));
                    //    var segment = boundarySegments[index];
                    //    var wall = doc.GetElement(segment.ElementId);
                    //}
                }

                foreach (var bFace in boundaryFaces)
                {
                    var bElement = doc.GetElement(bFace.SpatialBoundaryElement.HostElementId);
                    if (bElement is Autodesk.Revit.DB.Wall wall)
                    {
                        try
                        {
                            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

                            ICollection<Autodesk.Revit.DB.ElementId> ids;
                            using (var subTrans = new Autodesk.Revit.DB.SubTransaction(wall.Document))
                            {
                                subTrans.Start();
                                ids = doc.Delete(wall.Id);
                                subTrans.RollBack();
                            }

                            var foundSketch = ids.Select(id => doc.GetElement(id)).OfType<Autodesk.Revit.DB.Sketch>().Any();
                            if (foundSketch)
                            {
                                var bbBox = wall.get_BoundingBox(null);
                            }

                            TransactionManager.Instance.TransactionTaskDone();
                        }
                        catch (Exception e)
                        {
                        }

                        if (wall.WallType.Kind == Autodesk.Revit.DB.WallKind.Curtain)
                        {
                            var cGrid = wall.CurtainGrid;
                            var panels = cGrid.GetPanelIds().Select(x => doc.GetElement(x));
                            foreach (var panel in panels)
                            {
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

                                    hullPoints.Add(hPts.Select(x => x.ToPoint()).ToList());
                                }
                            }
                        }

                        var inserts = wall.FindInserts(true, false, true, true).Select(x => doc.GetElement(x));
                        foreach (var insert in inserts)
                        {
                            //if (insert is Autodesk.Revit.DB.Wall nestedWall)
                            //{
                            //    if (nestedWall.WallType.Kind == Autodesk.Revit.DB.WallKind.Curtain)
                            //    {
                            //        var cGrid = nestedWall.CurtainGrid;
                            //        var panels = cGrid.GetPanelIds().Select(x => doc.GetElement(x));
                            //        foreach (var panel in panels)
                            //        {
                            //            var winPts = new List<Autodesk.Revit.DB.XYZ>();
                            //            using (var opt = new Autodesk.Revit.DB.Options())
                            //            {
                            //                opt.IncludeNonVisibleObjects = true;
                            //                using (var geom = panel.get_Geometry(opt))
                            //                {
                            //                    ExtractPtsRecursively(geom, ref winPts);
                            //                }
                            //            }

                            //            var onSurface = new HashSet<Autodesk.Revit.DB.XYZ>();
                            //            var onSurfaceUvs = new HashSet<Autodesk.Revit.DB.UV>();
                            //            foreach (var pt in winPts)
                            //            {
                            //                var intResult = face.Project(pt);
                            //                if (intResult == null) continue;

                            //                if (onSurface.Add(intResult.XYZPoint))
                            //                {
                            //                    onSurfaceUvs.Add(intResult.UVPoint.Negate());
                            //                }
                            //            }

                            //            if (GetHull(onSurface.ToList(), onSurfaceUvs.ToList(), tolerance, out var hPts, out var hUvs))
                            //            {
                            //                var outerEdges = face.GetEdgesAsCurveLoops().First();
                            //                foreach (var edge in outerEdges)
                            //                {
                            //                    for (var i = 0; i < hPts.Count; i++)
                            //                    {
                            //                        var pt = hPts[i];
                            //                        if (edge.Distance(pt) >= 0.01) continue;

                            //                        var direction = (edge.GetEndPoint(1) - edge.GetEndPoint(0)).Normalize();
                            //                        var perpendicular = face.ComputeNormal(new Autodesk.Revit.DB.UV(0.5, 0.5)).CrossProduct(direction);
                            //                        var offset = 0.1 * perpendicular;
                            //                        var offsetPt = pt + offset;

                            //                        hPts[i] = offsetPt;
                            //                    }
                            //                }

                            //                hullPoints.Add(hPts.Select(x => x.ToPoint()).ToList());
                            //            }
                            //        }
                            //    }
                            //}
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
                                                var offset = 0.1 * perpendicular;
                                                var offsetPt = pt + offset;

                                                hPts[i] = offsetPt;
                                            }
                                        }

                                        hullPoints.Add(hPts.Select(x => x.ToPoint()).ToList());
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
                                        hullPoints.Add(hPts.Select(x => x.ToPoint()).ToList());
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return new Dictionary<string, object>
            {
                { "Pts", hullPoints },
                { "EdgePts", edgePoints }
            };
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

        private static double GetPtsWidth(IEnumerable<Autodesk.Revit.DB.XYZ> pts)
        {
            var xCoordinate = pts.Select(p => p.X).ToArray();
            Array.Sort(xCoordinate);

            return Math.Round(Math.Abs(xCoordinate[xCoordinate.Length - 1] - xCoordinate[0]), 2);
        }

        private static double GetPtsHeight(IEnumerable<Autodesk.Revit.DB.XYZ> pts)
        {
            var yCoordinate = pts.Select(p => p.Y).ToArray();
            Array.Sort(yCoordinate);

            return Math.Round(Math.Abs(yCoordinate[yCoordinate.Length - 1] - yCoordinate[0]), 2);
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
            catch (Exception e)
            {
                return false;
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="pts"></param>
        ///// <returns></returns>
        //public static List<Point> ConvexHull(List<Point> pts, List<Point> uvs)
        //{
        //    try
        //    {
        //        var hullPoints = uvs.Select(x => new HullPoint(x.X, x.Y)).ToList();
        //        var hull1 = Utils.ConvexHull.MakeHull(hullPoints);

        //        var plPoints = new List<Point>();
        //        foreach (var hullPoint in hull1)
        //        {
        //            var index = uvs.IndexOf(Point.ByCoordinates(hullPoint.x, hullPoint.y, 0));
        //            plPoints.Add(pts[index]);
        //        }
                
        //        var indexToRemove = -1;

        //        Restart:

        //        if (indexToRemove != -1)
        //        {
        //            plPoints.RemoveAt(indexToRemove);
        //            indexToRemove = -1;
        //        }

        //        for (var i = 0; i < plPoints.Count; i++)
        //        {
        //            var start = plPoints[i];
        //            Point middle;
        //            Point end;
        //            int middleIndex;
        //            if (i + 2 == plPoints.Count)
        //            {
        //                middle = plPoints[i + 1];
        //                middleIndex = i + 1;
        //                end = plPoints[0];
        //            }
        //            else if (i + 1 == plPoints.Count)
        //            {
        //                middle = plPoints[0];
        //                middleIndex = 0;
        //                end = plPoints[1];
        //            }
        //            else
        //            {
        //                middle = plPoints[i + 1];
        //                middleIndex = i + 1;
        //                end = plPoints[i + 2];
        //            }

        //            var line = Line.ByStartPointEndPoint(start, end);
        //            var closestPoint = line.ClosestPointTo(middle);
        //            if (!(closestPoint.DistanceTo(middle) < 0.01)) continue;

        //            indexToRemove = middleIndex;
        //            goto Restart;
        //        }

        //        return plPoints;
        //    }
        //    catch (Exception e)
        //    {
        //        return new List<Point>();
        //    }
        //}

        private static void ExtractPtsRecursively(Autodesk.Revit.DB.GeometryElement geo, ref List<Autodesk.Revit.DB.XYZ> pts, bool includeLines = false)
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
    }

    //[IsVisibleInDynamoLibrary(false)]
    //public class PointWrapper : IEquatable<PointWrapper>
    //{
    //    public double X { get; set; }
    //    public double Y { get; set; }
    //    public double Z { get; set; }

    //    public PointWrapper()
    //    {
    //    }

    //    public PointWrapper(Autodesk.Revit.DB.XYZ pt)
    //    {
    //        X = pt.X;
    //        Y = pt.Y;
    //        Z = pt.Z;
    //    }

    //    public Autodesk.Revit.DB.XYZ ToXyz()
    //    {
    //        return new Autodesk.Revit.DB.XYZ(X, Y, Z);
    //    }

    //    public override bool Equals(object obj)
    //    {
    //        if (obj is PointWrapper other)
    //        {
    //            return Equals(other);
    //        }
    //        return false;
    //    }

    //    public override int GetHashCode()
    //    {
    //        return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
    //    }

    //    public bool Equals(PointWrapper other)
    //    {
    //        if (other == null) return false;
    //        if (ReferenceEquals(this, other)) return true;

    //        return X.AlmostEqualTo(other.X)
    //               && Y.AlmostEqualTo(other.Y)
    //               && Z.AlmostEqualTo(other.Z);
    //    }
    //}

    //[IsVisibleInDynamoLibrary(false)]
    //public class Node2 : IComparable<Node2>
    //{
    //    private static double m_coincidence_tolerance = 1E-12;
    //    public double x;
    //    public double y;
    //    public int tag;

    //    public Node2()
    //    {
    //    }

    //    public Node2(double nx, double ny)
    //    {
    //        this.x = nx;
    //        this.y = ny;
    //        this.tag = -1;
    //    }

    //    public Node2(double nx, double ny, int n_tag)
    //    {
    //        this.x = nx;
    //        this.y = ny;
    //        this.tag = n_tag;
    //    }

    //    public Node2(Node2 other)
    //    {
    //        this.Set(other);
    //    }

    //    //public Node2(Node2 other, double dx, double dy)
    //    //{
    //    //    this.Set(other);
    //    //    this.Offset(dx, dy);
    //    //}

    //    public Node2(Node2 A, Node2 B, double f, int n_tag)
    //    {
    //        this.tag = n_tag;
    //        this.x = A.x + f * (B.x - A.x);
    //        this.y = A.y + f * (B.y - A.y);
    //    }

    //    public Node2 Duplicate()
    //    {
    //        return new Node2(this);
    //    }

    //    public void Set(Node2 other)
    //    {
    //        this.x = other.x;
    //        this.y = other.y;
    //        this.tag = other.tag;
    //    }

    //    public void Set(double nX, double nY)
    //    {
    //        this.x = nX;
    //        this.y = nY;
    //    }

    //    public static Vec2 operator -(Node2 A, Node2 B)
    //    {
    //        return new Vec2(A.x - B.x, A.y - B.y);
    //    }

    //    public static Node2 operator +(Node2 P, Vec2 V)
    //    {
    //        return new Node2(P.x + V.x, P.y + V.y, P.tag);
    //    }

    //    public static Node2 operator +(Node2 A, Node2 B)
    //    {
    //        return new Node2(A.x + B.x, A.y + B.y, A.tag);
    //    }

    //    public static Node2 operator -(Node2 P, Vec2 V)
    //    {
    //        return new Node2(P.x - V.x, P.y - V.y, P.tag);
    //    }

    //    public static Node2 operator *(Node2 N, double f)
    //    {
    //        return new Node2(N.x * f, N.y * f, N.tag);
    //    }

    //    public static Node2 operator *(double f, Node2 N)
    //    {
    //        return new Node2(N.x * f, N.y * f, N.tag);
    //    }

    //    //public void Offset(double dx, double dy)
    //    //{
    //    //    // ISSUE: variable of a reference type
    //    //    double&local1;
    //    //    // ISSUE: explicit reference operation
    //    //    double num1 = ^ (local1 = ref this.x) + dx;
    //    //    local1 = num1;
    //    //    // ISSUE: variable of a reference type
    //    //    double&local2;
    //    //    // ISSUE: explicit reference operation
    //    //    double num2 = ^ (local2 = ref this.y) + dy;
    //    //    local2 = num2;
    //    //}

    //    public double DistanceSquared(Node2 other)
    //    {
    //        return this.DistanceSquared(other.x, other.y);
    //    }

    //    public double DistanceSquared(double nx, double ny)
    //    {
    //        return (this.x - nx) * (this.x - nx) + (this.y - ny) * (this.y - ny);
    //    }

    //    public double Distance(Node2 other)
    //    {
    //        return Math.Sqrt(this.DistanceSquared(other));
    //    }

    //    public double Distance(double nx, double ny)
    //    {
    //        return Math.Sqrt(this.DistanceSquared(nx, ny));
    //    }

    //    public bool IsCoincident(Node2 other)
    //    {
    //        return this.IsCoincident(other.x, other.y);
    //    }

    //    public bool IsCoincident(double ox, double oy)
    //    {
    //        return Math.Abs(this.x - ox) <= Node2.m_coincidence_tolerance && Math.Abs(this.y - oy) <= Node2.m_coincidence_tolerance;
    //    }

    //    public bool IsValid
    //    {
    //        get
    //        {
    //            return !double.IsNaN(this.x) && !double.IsNaN(this.y);
    //        }
    //    }

    //    public int CompareTo(Node2 other)
    //    {
    //        return other != null ? (this.x != other.x ? this.x.CompareTo(other.x) : (this.y != other.y ? this.y.CompareTo(other.y) : 0)) : 1;
    //    }

    //    public int CompareTo(Node2 other, double tolerance)
    //    {
    //        return other != null ? (Math.Abs(this.x - other.x) >= tolerance ? this.x.CompareTo(other.x) : (Math.Abs(this.y - other.y) >= tolerance ? this.y.CompareTo(other.y) : 0)) : 1;
    //    }

    //    //public override string ToString()
    //    //{
    //    //    return string.Format("{0:0.00},{1:0.00} ({2})", (object)this.x, (object)this.y, (object)this.tag);
    //    //}

    //    public string DebuggerDisplay
    //    {
    //        get
    //        {
    //            return this.ToString();
    //        }
    //    }
    //}

    //public class Vec2 : IComparable<Vec2>
    //{
    //    private static double m_angle_tolerance = 0.00174532925199433;
    //    private static double m_unit_tolerance = 1E-32;
    //    public double x;
    //    public double y;

    //    public Vec2()
    //    {
    //    }

    //    public Vec2(double nX, double nY)
    //    {
    //        this.x = nX;
    //        this.y = nY;
    //    }

    //    public Vec2(Vec2 other)
    //    {
    //        this.x = other.x;
    //        this.y = other.y;
    //    }

    //    //public Vec2(Vector2d other)
    //    //{
    //    //    this.x = ((Vector2d) ref other).get_X();
    //    //    this.y = ((Vector2d) ref other).get_Y();
    //    //}

    //    //public Vec2(Vector2f other)
    //    //{
    //    //    this.x = (double)((Vector2f) ref other).get_X();
    //    //    this.y = (double)((Vector2f) ref other).get_Y();
    //    //}

    //    public Vec2 Duplicate()
    //    {
    //        return new Vec2(this);
    //    }

    //    public void Set(Vec2 other)
    //    {
    //        this.x = other.x;
    //        this.y = other.y;
    //    }

    //    public void Set(double nX, double nY)
    //    {
    //        this.x = nX;
    //        this.y = nY;
    //    }

    //    public static Vec2 operator +(Vec2 A, Vec2 B)
    //    {
    //        return new Vec2(A.x + B.x, A.y + B.y);
    //    }

    //    public static Vec2 operator -(Vec2 A, Vec2 B)
    //    {
    //        return new Vec2(A.x - B.x, A.y - B.y);
    //    }

    //    public static Vec2 operator *(Vec2 V, double F)
    //    {
    //        return new Vec2(V.x * F, V.y * F);
    //    }

    //    public static Vec2 Unit_X
    //    {
    //        get
    //        {
    //            return new Vec2(1.0, 0.0);
    //        }
    //    }

    //    public static Vec2 Unit_Y
    //    {
    //        get
    //        {
    //            return new Vec2(0.0, 1.0);
    //        }
    //    }

    //    public double Length()
    //    {
    //        return Math.Sqrt(this.LengthSquared());
    //    }

    //    public double LengthSquared()
    //    {
    //        return this.x * this.x + this.y * this.y;
    //    }

    //    public bool IsValid
    //    {
    //        get
    //        {
    //            return !double.IsNaN(this.x) && !double.IsNaN(this.y);
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        return string.Format("{0:0.00}, {1:0.00}", (object)this.x, (object)this.y);
    //    }

    //    public string DebuggerDisplay
    //    {
    //        get
    //        {
    //            return this.ToString();
    //        }
    //    }

    //    public int CompareTo(Vec2 other)
    //    {
    //        return other != null ? (this.x != other.x ? this.x.CompareTo(other.x) : (this.y != other.y ? this.y.CompareTo(other.y) : 0)) : 1;
    //    }

    //    public Vec2 CreatePerpendicular()
    //    {
    //        return this.LengthSquared() >= Vec2.m_unit_tolerance ? new Vec2(this.y, this.x) : new Vec2(0.0, 0.0);
    //    }

    //    public bool PerpendicularTo(Vec2 v)
    //    {
    //        return this.PerpendicularTo(v, Vec2.m_angle_tolerance);
    //    }

    //    public bool PerpendicularTo(Vec2 v, double angle_tol)
    //    {
    //        double num = this.Length() * v.Length();
    //        return num > 0.0 && Math.Abs((this.x * v.x + this.y * v.y) / num) <= Math.Sin(angle_tol);
    //    }

    //    //public Parallax ParallelTo(Vec2 v)
    //    //{
    //    //    return this.ParallelTo(v, Math.PI / Vec2.m_angle_tolerance);
    //    //}

    //    //public Parallax ParallelTo(Vec2 v, double angle_tol)
    //    //{
    //    //    Parallax parallax1 = Parallax.Divergent;
    //    //    double num1 = this.Length() * v.Length();
    //    //    Parallax parallax2;
    //    //    if (num1 <= 0.0)
    //    //    {
    //    //        parallax2 = parallax1;
    //    //    }
    //    //    else
    //    //    {
    //    //        double num2 = (this.x * v.x + this.y * v.y) / num1;
    //    //        double num3 = Math.Cos(angle_tol);
    //    //        if (num2 >= num3)
    //    //            parallax1 = Parallax.Parallel;
    //    //        else if (num2 <= -num3)
    //    //            parallax1 = Parallax.AntiParallel;
    //    //        parallax2 = parallax1;
    //    //    }
    //    //    return parallax2;
    //    //}

    //    //public void Unitize()
    //    //{
    //    //    double num1 = this.Length();
    //    //    if (num1 < Vec2.m_unit_tolerance)
    //    //    {
    //    //        this.x = 1.0;
    //    //        this.y = 0.0;
    //    //    }
    //    //    else
    //    //    {
    //    //        double num2 = 1.0 / num1;
    //    //        // ISSUE: variable of a reference type
    //    //        double&local1;
    //    //        // ISSUE: explicit reference operation
    //    //        double num3 = ^ (local1 = ref this.x) * num2;
    //    //        local1 = num3;
    //    //        // ISSUE: variable of a reference type
    //    //        double&local2;
    //    //        // ISSUE: explicit reference operation
    //    //        double num4 = ^ (local2 = ref this.y) * num2;
    //    //        local2 = num4;
    //    //    }
    //    //}
    //}

    //public class Node2List : IEnumerable<Node2>
    //{
    //    private List<Node2> m_nodes;
    //    private Node2List.NodeListSort m_sort;

    //    public Node2List()
    //    {
    //        this.m_nodes = new List<Node2>();
    //        this.m_sort = Node2List.NodeListSort.none;
    //    }

    //    public Node2List(IEnumerable<Node2> L)
    //    {
    //        this.m_nodes = new List<Node2>();
    //        this.m_sort = Node2List.NodeListSort.none;
    //        this.m_nodes.AddRange(L);
    //    }

    //    public Node2List(Node2List L)
    //    {
    //        this.m_nodes = new List<Node2>();
    //        this.m_sort = Node2List.NodeListSort.none;
    //        this.m_nodes.Capacity = L.m_nodes.Count;
    //        this.m_sort = L.m_sort;
    //        int num = L.m_nodes.Count - 1;
    //        for (int index = 0; index <= num; ++index)
    //        {
    //            if (L.m_nodes[index] == null)
    //                this.m_nodes.Add((Node2)null);
    //            else
    //                this.m_nodes.Add(new Node2(L.m_nodes[index]));
    //        }
    //    }

    //    //public Node2List(IEnumerable<GH_Point> pts)
    //    //{
    //    //    this.m_nodes = new List<Node2>();
    //    //    this.m_sort = Node2List.NodeListSort.none;
    //    //    if (pts == null)
    //    //        return;
    //    //    IEnumerator<GH_Point> enumerator;
    //    //    try
    //    //    {
    //    //        enumerator = pts.GetEnumerator();
    //    //        while (enumerator.MoveNext())
    //    //        {
    //    //            GH_Point current = enumerator.Current;
    //    //            if (!current.IsValid)
    //    //            {
    //    //                this.m_nodes.Add((Node2)null);
    //    //            }
    //    //            else
    //    //            {
    //    //                List<Node2> nodes = this.m_nodes;
    //    //                Point3d point3d = current.Value;
    //    //                double x = ((Point3d) ref point3d).get_X();
    //    //                point3d = current.Value;
    //    //                double y = ((Point3d) ref point3d).get_Y();
    //    //                Node2 node2 = new Node2(x, y);
    //    //                nodes.Add(node2);
    //    //            }
    //    //        }
    //    //    }
    //    //    finally
    //    //    {
    //    //        enumerator?.Dispose();
    //    //    }
    //    //}

    //    //public Node2List(IEnumerable<Point3d> pts)
    //    //{
    //    //    this.m_nodes = new List<Node2>();
    //    //    this.m_sort = Node2List.NodeListSort.none;
    //    //    if (pts == null)
    //    //        return;
    //    //    try
    //    //    {
    //    //        foreach (Point3d pt in pts)
    //    //            this.m_nodes.Add(new Node2(((Point3d) ref pt).get_X(), ((Point3d) ref pt).get_Y()));
    //    //    }
    //    //    finally
    //    //    {
    //    //        IEnumerator<Point3d> enumerator;
    //    //        ((IDisposable)enumerator)?.Dispose();
    //    //    }
    //    //}

    //    /// <summary>
    //    /// Add a single node to this list. This will reset all sorting flags and caches.
    //    /// </summary>
    //    /// <param name="node">Node to add</param>
    //    public void Append(Node2 node)
    //    {
    //        this.m_nodes.Add(node);
    //        this.ExpireSequence();
    //    }

    //    /// <summary>
    //    /// Add a range of nodes to this list. This will reset all sorting flags and caches.
    //    /// </summary>
    //    /// <param name="nodes">Nodes to add</param>
    //    public void AppendRange(IEnumerable<Node2> nodes)
    //    {
    //        this.m_nodes.AddRange(nodes);
    //        this.ExpireSequence();
    //    }

    //    /// <summary>
    //    /// Insert a single node into this list. This will reset all sorting flags and caches.
    //    /// </summary>
    //    /// <param name="node">Node to add</param>
    //    /// <param name="index">Index at which to insert the node</param>
    //    public void Insert(int index, Node2 node)
    //    {
    //        this.m_nodes.Insert(index, node);
    //        this.ExpireSequence();
    //    }

    //    /// <summary>
    //    /// Insert a range of nodes into this list. This will reset all sorting flags and caches.
    //    /// </summary>
    //    /// <param name="nodes">Nodes to add</param>
    //    /// <param name="index">Index at which insertion begins</param>
    //    public void InsertRange(int index, IEnumerable<Node2> nodes)
    //    {
    //        this.m_nodes.InsertRange(index, nodes);
    //        this.ExpireSequence();
    //    }

    //    /// <summary>
    //    /// Remove a single node from this list. Sorting flags are maintained but caches are destroyed.
    //    /// </summary>
    //    /// <param name="node">Node to remove.</param>
    //    /// <returns>True on success, false if the provided Node does not occur in this list.</returns>
    //    public bool Remove(Node2 node)
    //    {
    //        return this.m_nodes.Remove(node);
    //    }

    //    /// <summary>
    //    /// Remove the node at the specified index. Sorting flags are maintained but caches are destroyed.
    //    /// </summary>
    //    /// <param name="index">Index of node to remove</param>
    //    public void RemoveAt(int index)
    //    {
    //        this.m_nodes.RemoveAt(index);
    //    }

    //    /// <summary>
    //    /// Get or set a node. Be sure to call ExpireSequence() if your change affects sorting caches.
    //    /// </summary>
    //    /// <param name="i">Index of node</param>
    //    [IndexerName("Node")]
    //    public Node2 this[int i]
    //    {
    //        get
    //        {
    //            return this.m_nodes[i];
    //        }
    //        set
    //        {
    //            this.m_nodes[i] = value;
    //        }
    //    }

    //    /// <summary>Get the number of nodes contained in this list.</summary>
    //    public int Count
    //    {
    //        get
    //        {
    //            return this.m_nodes.Count;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets or sets the capacity of this list (i.e. the number of items this list can contain without resizing)
    //    /// </summary>
    //    public int Capacity
    //    {
    //        get
    //        {
    //            return this.m_nodes.Capacity;
    //        }
    //        set
    //        {
    //            this.m_nodes.Capacity = value;
    //        }
    //    }

    //    /// <summary>Removes all duplicates from this list. It also removes ALL null references.</summary>
    //    public int CullDuplicates()
    //    {
    //        SortedDictionary<Node2, int> sortedDictionary = new SortedDictionary<Node2, int>((IComparer<Node2>)new Node2List.FuzzyNode2Comparer(1E-12));
    //        int index1 = 0;
    //        int num1 = 0;
    //        int num2 = this.m_nodes.Count - 1;
    //        for (int index2 = 0; index2 <= num2; ++index2)
    //        {
    //            if (this.m_nodes[index2] == null || sortedDictionary.ContainsKey(this.m_nodes[index2]))
    //            {
    //                ++num1;
    //            }
    //            else
    //            {
    //                sortedDictionary.Add(this.m_nodes[index2], 1);
    //                this.m_nodes[index1] = this.m_nodes[index2];
    //                ++index1;
    //            }
    //        }
    //        if (index1 > 0)
    //            this.m_nodes.RemoveRange(index1, this.m_nodes.Count - index1);
    //        return num1;
    //    }

    //    /// <summary>Set all duplicate nodes to NULL</summary>
    //    public int NullifyDuplicates()
    //    {
    //        SortedDictionary<Node2, int> sortedDictionary = new SortedDictionary<Node2, int>((IComparer<Node2>)new Node2List.FuzzyNode2Comparer(1E-12));
    //        int num1 = 0;
    //        int num2 = this.m_nodes.Count - 1;
    //        for (int index = 0; index <= num2; ++index)
    //        {
    //            if (this.m_nodes[index] != null)
    //            {
    //                if (sortedDictionary.ContainsKey(this.m_nodes[index]))
    //                {
    //                    ++num1;
    //                    this.m_nodes[index] = (Node2)null;
    //                }
    //                else
    //                    sortedDictionary.Add(this.m_nodes[index], 1);
    //            }
    //        }
    //        return num1;
    //    }

    //    /// <summary>Remove all null references from this list.</summary>
    //    public int CullNullRefs()
    //    {
    //        int index1 = 0;
    //        int num1 = 0;
    //        int num2 = this.m_nodes.Count - 1;
    //        for (int index2 = 0; index2 <= num2; ++index2)
    //        {
    //            if (this.m_nodes[index2] == null)
    //            {
    //                ++num1;
    //            }
    //            else
    //            {
    //                this.m_nodes[index1] = this.m_nodes[index2];
    //                ++index1;
    //            }
    //        }
    //        if (index1 > 0)
    //            this.m_nodes.RemoveRange(index1, this.m_nodes.Count - index1);
    //        return num1;
    //    }

    //    public List<Node2> InternalList
    //    {
    //        get
    //        {
    //            return this.m_nodes;
    //        }
    //        set
    //        {
    //            this.m_nodes = value;
    //            this.ExpireSequence();
    //        }
    //    }

    //    ///// <summary>Randomly mutate all node locations</summary>
    //    ///// <param name="amount">Maximum distance to move in x and y directions.</param>
    //    //public void JitterNodes(double amount)
    //    //{
    //    //    Random random = new Random(623);
    //    //    int num1 = this.m_nodes.Count - 1;
    //    //    for (int index = 0; index <= num1; ++index)
    //    //    {
    //    //        if (this.m_nodes[index] != null)
    //    //        {
    //    //            // ISSUE: variable of a reference type
    //    //            double&local1;
    //    //            // ISSUE: explicit reference operation
    //    //            double num2 = ^ (local1 = ref this.m_nodes[index].x) + (amount * random.NextDouble() - 0.5 * amount);
    //    //            local1 = num2;
    //    //            // ISSUE: variable of a reference type
    //    //            double&local2;
    //    //            // ISSUE: explicit reference operation
    //    //            double num3 = ^ (local2 = ref this.m_nodes[index].y) + (amount * random.NextDouble() - 0.5 * amount);
    //    //            local2 = num3;
    //    //        }
    //    //    }
    //    //    this.ExpireSequence();
    //    //}

    //    /// <summary>
    //    /// Call this method when you made a change that potentially invalidates the sorting flags and caches.
    //    /// </summary>
    //    public void ExpireSequence()
    //    {
    //        this.m_sort = Node2List.NodeListSort.none;
    //    }

    //    /// <summary>Sort the list using a sorting type.</summary>
    //    /// <param name="type">Type of sorting algorithm.</param>
    //    public void Sort(Node2List.NodeListSort type)
    //    {
    //        if (this.m_sort == type)
    //            return;
    //        this.m_sort = type;
    //        switch (type)
    //        {
    //            case Node2List.NodeListSort.X:
    //                this.m_nodes.Sort(new Comparison<Node2>(Node2List.Comparison_XAscending));
    //                break;
    //            case Node2List.NodeListSort.Y:
    //                this.m_nodes.Sort(new Comparison<Node2>(Node2List.Comparison_YAscending));
    //                break;
    //            case Node2List.NodeListSort.Index:
    //                this.m_nodes.Sort(new Comparison<Node2>(Node2List.Comparison_IAscending));
    //                break;
    //        }
    //    }

    //    /// <summary>
    //    /// Reset all indices of all nodes by renumbering them in their current order.
    //    /// Nulls are ignored but they do affect the numbering.
    //    /// </summary>
    //    public void RenumberNodes()
    //    {
    //        if (this.m_nodes.Count == 0)
    //            return;
    //        int num = this.m_nodes.Count - 1;
    //        for (int index = 0; index <= num; ++index)
    //        {
    //            if (this.m_nodes[index] != null)
    //                this.m_nodes[index].tag = index;
    //        }
    //    }

    //    private static int Comparison_XAscending(Node2 A, Node2 B)
    //    {
    //        int num1;
    //        if (A == null)
    //            num1 = B != null ? -1 : 0;
    //        else if (B == null)
    //        {
    //            num1 = 1;
    //        }
    //        else
    //        {
    //            int num2 = A.x.CompareTo(B.x);
    //            num1 = num2 != 0 ? num2 : A.y.CompareTo(B.y);
    //        }
    //        return num1;
    //    }

    //    private static int Comparison_YAscending(Node2 A, Node2 B)
    //    {
    //        int num1;
    //        if (A == null)
    //            num1 = B != null ? -1 : 0;
    //        else if (B == null)
    //        {
    //            num1 = 1;
    //        }
    //        else
    //        {
    //            int num2 = A.y.CompareTo(B.y);
    //            num1 = num2 != 0 ? num2 : A.x.CompareTo(B.x);
    //        }
    //        return num1;
    //    }

    //    private static int Comparison_IAscending(Node2 A, Node2 B)
    //    {
    //        return A != null ? (B != null ? A.tag.CompareTo(B.tag) : 1) : (B != null ? -1 : 0);
    //    }

    //    public int BinarySearch_X(double x)
    //    {
    //        if (this.m_sort != Node2List.NodeListSort.X)
    //            throw new Exception("Invalid BinarySearch operation for sort cache");
    //        return this.m_nodes.BinarySearch(new Node2(x, 0.0, 0), (IComparer<Node2>)new Node2List.Comparer_X());
    //    }

    //    public int BinarySearch_Y(double y)
    //    {
    //        if (this.m_sort != Node2List.NodeListSort.Y)
    //            throw new Exception("Invalid BinarySearch operation for sort cache");
    //        return this.m_nodes.BinarySearch(new Node2(0.0, y, 0), (IComparer<Node2>)new Node2List.Comparer_Y());
    //    }

    //    public int BinarySearch_I(int i)
    //    {
    //        if (this.m_sort != Node2List.NodeListSort.Index)
    //            throw new Exception("Invalid BinarySearch operation for sort cache");
    //        return this.m_nodes.BinarySearch(new Node2(0.0, 0.0, i), (IComparer<Node2>)new Node2List.Comparer_I());
    //    }

    //    ///// <summary>Perform a brute force node search for the N nearest nodes in the set.</summary>
    //    ///// <param name="x">X coordinate of search start.</param>
    //    ///// <param name="y">Y coordinate of search start.</param>
    //    ///// <param name="N"></param>
    //    ///// <param name="min_dist_squared">Minimum distance threshold, use any negative value to ignore this setting.</param>
    //    ///// <param name="max_dist_squared">Maximum distance threshold.</param>
    //    ///// <returns>The N (or fewer) results, sorted by ascending distance.</returns>
    //    //public List<int> NearestNodes(
    //    //  double x,
    //    //  double y,
    //    //  int N,
    //    //  double min_dist_squared = -1.79769313486232E+308,
    //    //  double max_dist_squared = 1.79769313486232E+308)
    //    //{
    //    //    List<double> doubleList = new List<double>();
    //    //    List<int> intList = new List<int>();
    //    //    int num1 = this.m_nodes.Count - 1;
    //    //    for (int index = 0; index <= num1; ++index)
    //    //    {
    //    //        if (this.m_nodes[index] != null)
    //    //        {
    //    //            double num2 = this.m_nodes[index].DistanceSquared(x, y);
    //    //            if (num2 <= max_dist_squared && num2 >= min_dist_squared)
    //    //            {
    //    //                doubleList.Add(num2);
    //    //                intList.Add(index);
    //    //            }
    //    //        }
    //    //    }
    //    //    double[] array1 = doubleList.ToArray();
    //    //    int[] array2 = intList.ToArray();
    //    //    int[] items = array2;
    //    //    Array.Sort<double, int>(array1, items);
    //    //    intList.Clear();
    //    //    intList.Capacity = array2.Length;
    //    //    intList.AddRange((IEnumerable<int>)array2);
    //    //    if (intList.Count > N)
    //    //        intList.RemoveRange(N, intList.Count - N);
    //    //    return intList;
    //    //}

    //    /// <summary>Compute the bounding box of all Nodes.</summary>
    //    /// <param name="GrowthFactor">Factor by which to grow the boundingbox</param>
    //    /// <param name="ForceSquareLeaves">If True, the boundingbox will be renormalized</param>
    //    public bool BoundingBox(
    //      double GrowthFactor,
    //      bool ForceSquareLeaves,
    //      ref double x0,
    //      ref double x1,
    //      ref double y0,
    //      ref double y1)
    //    {
    //        bool flag;
    //        if (this.m_nodes.Count == 0)
    //        {
    //            flag = false;
    //        }
    //        else
    //        {
    //            x0 = double.MaxValue;
    //            x1 = double.MinValue;
    //            y0 = double.MaxValue;
    //            y1 = double.MinValue;
    //            int num1 = this.m_nodes.Count - 1;
    //            for (int index = 0; index <= num1; ++index)
    //            {
    //                if (this.m_nodes[index] != null)
    //                {
    //                    x0 = Math.Min(x0, this.m_nodes[index].x);
    //                    x1 = Math.Max(x1, this.m_nodes[index].x);
    //                    y0 = Math.Min(y0, this.m_nodes[index].y);
    //                    y1 = Math.Max(y1, this.m_nodes[index].y);
    //                }
    //            }
    //            double num2 = x1 - x0;
    //            double num3 = y1 - y0;
    //            x0 -= GrowthFactor * num2;
    //            x1 += GrowthFactor * num2;
    //            y0 -= GrowthFactor * num3;
    //            y1 += GrowthFactor * num3;
    //            if (ForceSquareLeaves)
    //            {
    //                if (num3 > num2)
    //                {
    //                    x0 -= 0.5 * (num3 - num2);
    //                    x1 += 0.5 * (num3 - num2);
    //                }
    //                if (num2 > num3)
    //                {
    //                    y0 -= 0.5 * (num2 - num3);
    //                    y1 += 0.5 * (num2 - num3);
    //                }
    //            }
    //            flag = true;
    //        }
    //        return flag;
    //    }

    //    ///// <summary>Create a VorNodeTree that contains all the Nodes in this list.</summary>
    //    ///// <param name="GrowthFactor">Factor by which to grow the boundingbox of the topmost Leaf</param>
    //    ///// <param name="SquareLeaves">If True, the boundingbox will be normalized</param>
    //    ///// <param name="GroupLimit">Maximum number of Nodes that are allowed to share a single Leaf</param>
    //    //public Node2Tree CreateTree(double GrowthFactor, bool SquareLeaves, int GroupLimit)
    //    //{
    //    //    Node2Tree node2Tree = new Node2Tree(this);
    //    //    node2Tree.RecreateTree(GrowthFactor, SquareLeaves, GroupLimit);
    //    //    return node2Tree;
    //    //}

    //    public IEnumerator<Node2> GetEnumerator()
    //    {
    //        return (IEnumerator<Node2>)this.m_nodes.GetEnumerator();
    //    }

    //    public IEnumerator GetEnumerator1()
    //    {
    //        return (IEnumerator)this.m_nodes.GetEnumerator();
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    /// <summary>Represents the different sort types that a VorNodeList can maintain.</summary>
    //    public enum NodeListSort
    //    {
    //        /// <summary>
    //        /// No specific sorting. When nodes are added or inserted the sort is always set back to none.
    //        /// </summary>
    //        none,
    //        /// <summary>Nodes are sorted by ascending x-coordinate</summary>
    //        X,
    //        /// <summary>Nodes are sorted by ascending y-coordinate</summary>
    //        Y,
    //        /// <summary>Nodes are sorted by ascending index</summary>
    //        Index,
    //    }

    //    private class FuzzyNode2Comparer : IComparer<Node2>
    //    {
    //        private double m_fuzz;

    //        public FuzzyNode2Comparer(double fuzz)
    //        {
    //            this.m_fuzz = fuzz;
    //        }

    //        public int Compare(Node2 A, Node2 B)
    //        {
    //            int num1;
    //            if (A == null)
    //                num1 = B != null ? -1 : 0;
    //            else if (B == null)
    //            {
    //                num1 = 1;
    //            }
    //            else
    //            {
    //                double num2 = Math.Abs(A.x - B.x);
    //                double num3 = Math.Abs(A.y - B.y);
    //                double fuzz = this.m_fuzz;
    //                num1 = num2 >= fuzz || num3 >= this.m_fuzz ? A.CompareTo(B) : 0;
    //            }
    //            return num1;
    //        }
    //    }

    //    private class Comparer_X : IComparer<Node2>
    //    {
    //        public int Compare(Node2 x, Node2 y)
    //        {
    //            return x != null ? (y != null ? x.x.CompareTo(y.x) : 1) : (y != null ? -1 : 0);
    //        }
    //    }

    //    private class Comparer_Y : IComparer<Node2>
    //    {
    //        public int Compare(Node2 x, Node2 y)
    //        {
    //            return x != null ? (y != null ? x.y.CompareTo(y.y) : 1) : (y != null ? -1 : 0);
    //        }
    //    }

    //    private class Comparer_I : IComparer<Node2>
    //    {
    //        public int Compare(Node2 x, Node2 y)
    //        {
    //            return x != null ? (y != null ? x.tag.CompareTo(y.tag) : 1) : (y != null ? -1 : 0);
    //        }
    //    }
    //}
}
