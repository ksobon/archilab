using archilab.Revit.Utils;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DynamoServices;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="panel"></param>
        /// <param name="type"></param>
        /// <param name="boundaryLocation"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        [MultiReturn("Curves", "Surfaces", "GlazingPoints", "Planes")]
        public static Dictionary<string, object> ExtrudeBoundary(Element room, Element panel, string type = "Window", string boundaryLocation = "Center", double height = 10.0)
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room));

            var bLoc = (Autodesk.Revit.DB.SpatialElementBoundaryLocation)Enum.Parse(typeof(Autodesk.Revit.DB.SpatialElementBoundaryLocation), boundaryLocation);
            var bOptions = new Autodesk.Revit.DB.SpatialElementBoundaryOptions
            {
                SpatialElementBoundaryLocation = bLoc
            };
            var rm = (Autodesk.Revit.DB.SpatialElement)room.InternalElement;
            var segments = rm.GetBoundarySegments(bOptions);
            var offset = rm.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.ROOM_LOWER_OFFSET).AsDouble();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var tolerance = 0.001;
            var shortCurveTolerance = doc.Application.ShortCurveTolerance;

            var result = new List<Surface>();
            var curves = new List<Line>();
            var apertures = new List<List<List<Point>>>();
            var planes = new List<Plane>();

            for (var i = 0; i < segments.Count; i++)
            {
                if (i == 0) // outer boundary
                {
                    var count = 0;
                    foreach (var bs in segments[i])
                    {
                        var bottomCurve = bs.GetCurve().Offset(offset);
                        if (bottomCurve.Length < tolerance || bottomCurve.Length < shortCurveTolerance)
                        {
                            continue; // Exclude tiny curves, they don't produce faces.
                        }

                        if (bottomCurve is Autodesk.Revit.DB.Arc arc)
                        {
                            var arcSegments = PlanarizeArc(arc, shortCurveTolerance);
                            foreach (var arcSegment in arcSegments)
                            {
                                var upperCurve = arcSegment.Offset(height);
                                var surface = Autodesk.Revit.DB.RuledSurface.Create(arcSegment, upperCurve);
                                if (!(surface is Autodesk.Revit.DB.Plane plane))
                                    continue;

                                var dSurface = Surface.ByRuledLoft(new List<Line>
                                    {arcSegment.ToProtoType() as Line, upperCurve.ToProtoType() as Line});
                                result.Add(dSurface); 
                                curves.Add(arcSegment.ToProtoType() as Line);

                                var minPt = arcSegment.GetEndPoint(0);
                                var maxPt = upperCurve.GetEndPoint(1);
                                var minMax = new Tuple<Autodesk.Revit.DB.XYZ, Autodesk.Revit.DB.XYZ>(minPt, maxPt);

                                var minPt1 = arcSegment.GetEndPoint(1);
                                var maxPt1 = upperCurve.GetEndPoint(0);

                                var c2 = Autodesk.Revit.DB.Line.CreateBound(minPt1, maxPt);
                                var c3 = Autodesk.Revit.DB.Line.CreateBound(maxPt, maxPt1);
                                var c4 = Autodesk.Revit.DB.Line.CreateBound(maxPt1, minPt);
                                var edges =
                                    new Tuple<Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line,
                                        Autodesk.Revit.DB.Line>(arcSegment, c2, c3, c4);

                                var glazingPts = new List<List<Point>>();
                                var uvPts = new List<List<UV>>();
                                var glazingLines = new List<List<List<Line>>>();
                                var glazingAreas = new List<double>();
                                var userDatas = new List<Dictionary<string, object>>();

                                // (Konrad) Set Geometry for panel if its within 1' of the wall.
                                // Saves time for efficiency.
                                var a = new ApertureWrapper(panel.InternalElement);
                                if (a.Locations.All(x => Math.Abs(plane.SignedDistanceTo(x)) > 1))
                                    continue;

                                a.SetGeometry();

                                if (type == "Window")
                                    AddWindows(panel.InternalElement, plane, minMax, edges, shortCurveTolerance, tolerance, ref glazingPts, ref uvPts, ref glazingLines, ref glazingAreas, ref userDatas);
                                else
                                    AddCurtainWallPanel(a, plane, minMax, edges, shortCurveTolerance, tolerance, ref glazingPts, ref glazingAreas, ref userDatas);

                                if (glazingPts.Any())
                                {
                                    apertures.Add(glazingPts);
                                }
                            }
                        }
                        else
                        {
                            var upperCurve = bottomCurve.Offset(height);
                            var surface = Autodesk.Revit.DB.RuledSurface.Create(bottomCurve, upperCurve);
                            if (!(surface is Autodesk.Revit.DB.Plane plane))
                                continue;

                            var dPlane = plane.ToPlane();
                            planes.Add(dPlane);

                            var dSurface = Surface.ByRuledLoft(new List<Line>
                                {bottomCurve.ToProtoType() as Line, upperCurve.ToProtoType() as Line});
                            result.Add(dSurface);
                            curves.Add(bottomCurve.ToProtoType() as Line);

                            var minPt = bottomCurve.GetEndPoint(0);
                            var maxPt = upperCurve.GetEndPoint(1);
                            var minMax = new Tuple<Autodesk.Revit.DB.XYZ, Autodesk.Revit.DB.XYZ>(minPt, maxPt);

                            var minPt1 = bottomCurve.GetEndPoint(1);
                            var maxPt1 = upperCurve.GetEndPoint(0);

                            var c2 = Autodesk.Revit.DB.Line.CreateBound(minPt1, maxPt);
                            var c3 = Autodesk.Revit.DB.Line.CreateBound(maxPt, maxPt1);
                            var c4 = Autodesk.Revit.DB.Line.CreateBound(maxPt1, minPt);
                            var edges =
                                new Tuple<Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line,
                                    Autodesk.Revit.DB.Line>(bottomCurve as Autodesk.Revit.DB.Line, c2, c3, c4);

                            var glazingPts = new List<List<Point>>();
                            var uvPts = new List<List<UV>>();
                            var glazingLines = new List<List<List<Line>>>();
                            var glazingAreas = new List<double>();
                            var userDatas = new List<Dictionary<string, object>>();

                            // (Konrad) Set Geometry for panel if its within 1' of the wall.
                            // Saves time for efficiency.
                            var a = new ApertureWrapper(panel.InternalElement);
                            if (a.Locations.All(x => Math.Abs(plane.SignedDistanceTo(x)) > 1))
                                continue;

                            a.SetGeometry();

                            if (type == "Window")
                                AddWindows(panel.InternalElement, plane, minMax, edges, shortCurveTolerance, tolerance, ref glazingPts, ref uvPts, ref glazingLines, ref glazingAreas, ref userDatas);
                            else
                                AddCurtainWallPanel(a, plane, minMax, edges, shortCurveTolerance, tolerance, ref glazingPts, ref glazingAreas, ref userDatas);

                            if (glazingPts.Any())
                            {
                                apertures.Add(glazingPts);
                            }
                        }
                    }
                }
            }

            return new Dictionary<string, object>
            {
                { "Curves", curves },
                { "Surfaces", result },
                { "GlazingPoints", apertures},
                { "Planes", planes}
            };
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="panel"></param>
        /// <param name="type"></param>
        /// <param name="boundaryLocation"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        [MultiReturn("Curves", "Surfaces", "GlazingPoints", "UVPoints", "GlazingLines", "Planes")]
        public static Dictionary<string, object> ExtrudeBoundary2(Element room, Element panel, string type = "Window", string boundaryLocation = "Center", double height = 10.0)
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room));

            var bLoc = (Autodesk.Revit.DB.SpatialElementBoundaryLocation)Enum.Parse(typeof(Autodesk.Revit.DB.SpatialElementBoundaryLocation), boundaryLocation);
            var bOptions = new Autodesk.Revit.DB.SpatialElementBoundaryOptions
            {
                SpatialElementBoundaryLocation = bLoc
            };
            var rm = (Autodesk.Revit.DB.SpatialElement)room.InternalElement;
            var segments = rm.GetBoundarySegments(bOptions);
            var offset = rm.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.ROOM_LOWER_OFFSET).AsDouble();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var tolerance = 0.001;
            var shortCurveTolerance = doc.Application.ShortCurveTolerance;

            var result = new List<Surface>();
            var curves = new List<Line>();
            var apertures = new List<List<List<Point>>>();
            var aperturesFlat = new List<List<List<UV>>>();
            var aperturesLines = new List<List<List<List<Line>>>>();
            var planes = new List<Plane>();

            for (var i = 0; i < segments.Count; i++)
            {
                if (i == 0) // outer boundary
                {
                    var count = 0;
                    foreach (var bs in segments[i])
                    {
                        if (count == 21)
                        {
                            goto Show;
                        }
                        else
                        {
                            count++;
                            continue;
                        }

                        Show:

                        count++;
                        
                        var bottomCurve = bs.GetCurve().Offset(offset);
                        if (bottomCurve.Length < tolerance || bottomCurve.Length < shortCurveTolerance)
                        {
                            continue; // Exclude tiny curves, they don't produce faces.
                        }

                        if (bottomCurve is Autodesk.Revit.DB.Arc arc)
                        {
                            var arcSegments = PlanarizeArc(arc, shortCurveTolerance);
                            foreach (var arcSegment in arcSegments)
                            {
                                var upperCurve = arcSegment.Offset(height);
                                var surface = Autodesk.Revit.DB.RuledSurface.Create(arcSegment, upperCurve);
                                if (!(surface is Autodesk.Revit.DB.Plane plane))
                                    continue;

                                var dSurface = Surface.ByRuledLoft(new List<Line>
                                    {arcSegment.ToProtoType() as Line, upperCurve.ToProtoType() as Line});
                                result.Add(dSurface); 
                                curves.Add(arcSegment.ToProtoType() as Line);

                                var minPt = arcSegment.GetEndPoint(0);
                                var maxPt = upperCurve.GetEndPoint(1);
                                var minMax = new Tuple<Autodesk.Revit.DB.XYZ, Autodesk.Revit.DB.XYZ>(minPt, maxPt);

                                var minPt1 = arcSegment.GetEndPoint(1);
                                var maxPt1 = upperCurve.GetEndPoint(0);

                                var c2 = Autodesk.Revit.DB.Line.CreateBound(minPt1, maxPt);
                                var c3 = Autodesk.Revit.DB.Line.CreateBound(maxPt, maxPt1);
                                var c4 = Autodesk.Revit.DB.Line.CreateBound(maxPt1, minPt);
                                var edges =
                                    new Tuple<Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line,
                                        Autodesk.Revit.DB.Line>(arcSegment, c2, c3, c4);

                                var glazingPts = new List<List<Point>>();
                                var uvPts = new List<List<UV>>();
                                var glazingLines = new List<List<List<Line>>>();
                                var glazingAreas = new List<double>();
                                var userDatas = new List<Dictionary<string, object>>();

                                // (Konrad) Set Geometry for panel if its within 1' of the wall.
                                // Saves time for efficiency.
                                var a = new ApertureWrapper(panel.InternalElement);
                                if (a.Locations.All(x => Math.Abs(plane.SignedDistanceTo(x)) > 1))
                                    continue;

                                a.SetGeometry();

                                if (type == "Window")
                                    AddWindows(panel.InternalElement, plane, minMax, edges, shortCurveTolerance, tolerance, ref glazingPts, ref uvPts, ref glazingLines, ref glazingAreas, ref userDatas);
                                else
                                    AddCurtainWallPanel(a, plane, minMax, edges, shortCurveTolerance, tolerance, ref glazingPts, ref glazingAreas, ref userDatas);

                                if (glazingPts.Any())
                                {
                                    apertures.Add(glazingPts);
                                    aperturesFlat.Add(uvPts);
                                    aperturesLines.Add(glazingLines);
                                }
                            }
                        }
                        else
                        {
                            var upperCurve = bottomCurve.Offset(height);
                            var surface = Autodesk.Revit.DB.RuledSurface.Create(bottomCurve, upperCurve);
                            if (!(surface is Autodesk.Revit.DB.Plane plane))
                                continue;

                            var dPlane = plane.ToPlane();
                            planes.Add(dPlane);

                            var dSurface = Surface.ByRuledLoft(new List<Line>
                                {bottomCurve.ToProtoType() as Line, upperCurve.ToProtoType() as Line});
                            result.Add(dSurface);
                            curves.Add(bottomCurve.ToProtoType() as Line);

                            var minPt = bottomCurve.GetEndPoint(0);
                            var maxPt = upperCurve.GetEndPoint(1);
                            var minMax = new Tuple<Autodesk.Revit.DB.XYZ, Autodesk.Revit.DB.XYZ>(minPt, maxPt);

                            var minPt1 = bottomCurve.GetEndPoint(1);
                            var maxPt1 = upperCurve.GetEndPoint(0);

                            var c2 = Autodesk.Revit.DB.Line.CreateBound(minPt1, maxPt);
                            var c3 = Autodesk.Revit.DB.Line.CreateBound(maxPt, maxPt1);
                            var c4 = Autodesk.Revit.DB.Line.CreateBound(maxPt1, minPt);
                            var edges =
                                new Tuple<Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line,
                                    Autodesk.Revit.DB.Line>(bottomCurve as Autodesk.Revit.DB.Line, c2, c3, c4);

                            var glazingPts = new List<List<Point>>();
                            var uvPts = new List<List<UV>>();
                            var glazingLines = new List<List<List<Line>>>();
                            var glazingAreas = new List<double>();
                            var userDatas = new List<Dictionary<string, object>>();

                            // (Konrad) Set Geometry for panel if its within 1' of the wall.
                            // Saves time for efficiency.
                            var a = new ApertureWrapper(panel.InternalElement);
                            if (a.Locations.All(x => Math.Abs(plane.SignedDistanceTo(x)) > 1))
                                continue;

                            a.SetGeometry();

                            if (type == "Window")
                                AddWindows(panel.InternalElement, plane, minMax, edges, shortCurveTolerance, tolerance, ref glazingPts, ref uvPts, ref glazingLines, ref glazingAreas, ref userDatas);
                            else
                                AddCurtainWallPanel(a, plane, minMax, edges, shortCurveTolerance, tolerance, ref glazingPts, ref glazingAreas, ref userDatas);

                            if (glazingPts.Any())
                            {
                                apertures.Add(glazingPts);
                                aperturesFlat.Add(uvPts);
                                aperturesLines.Add(glazingLines);
                            }
                        }
                    }
                }
            }

            return new Dictionary<string, object>
            {
                { "Curves", curves },
                { "Surfaces", result },
                { "GlazingPoints", apertures},
                { "UVPoints", aperturesFlat},
                { "GlazingLines", aperturesLines},
                { "Planes", planes}
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tolerance"></param>
        /// <param name="hPts"></param>
        /// <returns></returns>
        public static bool ValidatePointDistance(double tolerance, ref List<Autodesk.Revit.DB.XYZ> hPts)
        {
            var isValid = true;
            for (var i = 0; i < hPts.Count; i++)
            {
                var current = hPts[i];
                var next = hPts[i + 1 >= hPts.Count ? 0 : i + 1];
                var distance = current.DistanceTo(next);
                if (distance > tolerance)
                    continue;

                isValid = false;
            }

            return isValid;
        }

        private static void AddWindows(
            Autodesk.Revit.DB.Element w,
            Autodesk.Revit.DB.Plane plane,
            Tuple<Autodesk.Revit.DB.XYZ, Autodesk.Revit.DB.XYZ> minMax,
            Tuple<Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line> edges,
            double shortCurveTolerance,
            double tolerance,
            ref List<List<Point>> glazingPts,
            ref List<List<UV>> uvPts,
            ref List<List<List<Line>>> glazingLines,
            ref List<double> glazingAreas,
            ref List<Dictionary<string, object>> userDatas)
        {
            var location = (w.Location as Autodesk.Revit.DB.LocationPoint)?.Point;
            var distance = plane.SignedDistanceTo(location);
            if (Math.Abs(distance) > 1)
                return;

            var winEdges = GetGeometryEdges(w);
            if (!GetEdgesOnFace(plane, minMax, edges, winEdges, out var ptsOnFace, out var uvsOnFace))
                return;

            var lines = new List<List<Line>>();
            var faces = ptsOnFace.GroupBy(x => x.FaceId);
            foreach (var gFace in faces)
            {
                var faceLines = new List<Line>();
                var gLines = gFace.GroupBy(x => x.EdgeId);
                foreach (var g in gLines)
                {
                    if (g.Count() != 2)
                        continue;

                    if (g.First().Point.IsAlmostEqualTo(g.Last().Point))
                        continue;

                    var l = Line.ByStartPointEndPoint(g.First().Point.ToPoint(), g.Last().Point.ToPoint());
                    faceLines.Add(l);
                }

                lines.Add(faceLines);
            }

            //(Konrad) Make sure that we are not adding duplicate apertures.
            var dPts = ptsOnFace.Select(x => x.Point.ToPoint()).ToList();
            var dUvPts = uvsOnFace.Select(x => x.UV.ToProtoType()).ToList();
            glazingPts.Add(dPts);
            uvPts.Add(dUvPts);
            glazingLines.Add(lines);
        }

        private static void AddCurtainWallPanel(
            ApertureWrapper panel,
            Autodesk.Revit.DB.Plane plane,
            Tuple<Autodesk.Revit.DB.XYZ, Autodesk.Revit.DB.XYZ> minMax,
            Tuple<Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line> edges,
            double shortCurveTolerance,
            double tolerance,
            ref List<List<Point>> glazingPts,
            ref List<double> glazingAreas,
            ref List<Dictionary<string, object>> userDatas)
        {
            // (Konrad) If user edited Wall Profile for a Curtain Wall, the Panels that were "edited out"
            // still exist in the list of Panels for the CW. They are valid objects per se, but we should
            // exclude them from glazing calculations. Technically they don't exist. They will have no Area.
            if (!panel.HasArea)
                return;

            if (!GetPointsOnFace(plane, minMax, edges, panel.GeometryPoints, out var ptsOnFace, out var uvsOnFace))
                return;

            if (!GetHull(ptsOnFace, uvsOnFace, shortCurveTolerance, out var hPts, out var hUvs))
                return;

            if (hPts.Count < 3)
                return;

            ValidatePoints(plane, edges, ref hPts, ref hUvs);

            // (Konrad) If Curtain Wall panel's area is smaller than 10% of original panel's area
            // we can ignore it. It means we projected it onto the edge, and created a little sliver.
            var hullArea = PolygonArea(hUvs);
            if (hullArea < panel.Area * 0.1)
                return;

            // (Konrad) Make sure that we are not adding duplicate apertures.
            var dPts = hPts.Select(x => x.ToPoint()).ToList();
            glazingPts.Add(dPts);
            glazingAreas.Add(PolygonArea(hUvs));
        }

        private static void ValidatePoints(
            Autodesk.Revit.DB.Plane plane,
            Tuple<Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line> edges,
            ref List<Autodesk.Revit.DB.XYZ> ptsOnFace,
            ref List<Autodesk.Revit.DB.UV> uvsOnFace)
        {
            var (e1, e2, e3, e4) = edges;
            var outerEdges = new List<Autodesk.Revit.DB.Line>
            {
                e1, e2, e3, e4
            };

            foreach (var edge in outerEdges)
            {
                for (var i = 0; i < ptsOnFace.Count; i++)
                {
                    var pt = ptsOnFace[i];
                    if (edge.Distance(pt) >= 0.001)
                        continue;

                    var direction = (edge.GetEndPoint(1) - edge.GetEndPoint(0)).Normalize();
                    var perpendicular = plane.Normal.CrossProduct(direction);
                    var offset = 0.041 * perpendicular;
                    var offsetPt = pt + offset;
                    ptsOnFace[i] = offsetPt;
                }
            }

            uvsOnFace.Clear();
            uvsOnFace.AddRange(ptsOnFace.Select(plane.ProjectInto));
        }

        private static bool GetPointsOnFace(
            Autodesk.Revit.DB.Plane plane,
            Tuple<Autodesk.Revit.DB.XYZ, Autodesk.Revit.DB.XYZ> minMax,
            Tuple<Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line> edges,
            List<Autodesk.Revit.DB.XYZ> pts,
            out List<Autodesk.Revit.DB.XYZ> ptsOnFace,
            out List<Autodesk.Revit.DB.UV> uvsOnFace)
        {
            var (min, max) = minMax;
            var (e1, e2, e3, e4) = edges;
            var outerEdges = new List<Autodesk.Revit.DB.Line>
            {
                e1, e2, e3, e4
            };

            var onFace = new HashSet<Autodesk.Revit.DB.XYZ>();
            var notOnFace = new HashSet<Autodesk.Revit.DB.XYZ>();
            foreach (var pt in pts)
            {
                var pt1 = new Autodesk.Revit.DB.XYZ(pt.X, pt.Y, pt.Z);
                var p = plane.ProjectOnto(pt1);
                if (!p.IsWithinFace(min, max))
                {
                    notOnFace.Add(pt1);
                    continue;
                }

                if (!onFace.Contains(p, new XyzComparer()))
                    onFace.Add(p);
            }

            var onFaceList = onFace.ToList();
            var notOnFaceList = notOnFace.ToList();

            if (onFace.Any() && notOnFace.Any())
            {
                foreach (var pt in notOnFaceList)
                {
                    Autodesk.Revit.DB.Line closestEdge = outerEdges.First();
                    var distance = double.MaxValue;
                    foreach (var edge in outerEdges)
                    {
                        var d = edge.Distance(pt);
                        if (!(d < distance))
                            continue;

                        distance = d;
                        closestEdge = edge;
                    }

                    var ptOnEdge = closestEdge.Project(pt).XYZPoint;
                    onFaceList.Add(ptOnEdge);
                }
            }

            ptsOnFace = onFaceList;
            uvsOnFace = onFaceList.Select(plane.ProjectInto).ToList();

            return ptsOnFace.Any() && uvsOnFace.Any();
        }

        private static bool GetEdgesOnFace(
            Autodesk.Revit.DB.Plane plane,
            Tuple<Autodesk.Revit.DB.XYZ, Autodesk.Revit.DB.XYZ> minMax,
            Tuple<Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line, Autodesk.Revit.DB.Line> edges,
            List<List<EdgeLine>> lines,
            out List<EdgePoint> ptsOnFace,
            out List<EdgeUV> uvsOnFace)
        {
            var pts = new List<EdgePoint>();
            for (var i = 0; i < lines.Count; i++)
            {
                var faceLines = lines[i];

                for (var j = 0; j < faceLines.Count; j++)
                {
                    var l = faceLines[j];
                    pts.Add(new EdgePoint(l.Line.GetEndPoint(0), l.FaceId, l.EdgeId, 0));
                    pts.Add(new EdgePoint(l.Line.GetEndPoint(1), l.FaceId, l.EdgeId, 1));
                }
            }

            var (min, max) = minMax;
            var (e1, e2, e3, e4) = edges;
            var outerEdges = new List<Autodesk.Revit.DB.Line>
            {
                e1, e2, e3, e4
            };

            var onFace = new List<EdgePoint>();
            var notOnFace = new List<EdgePoint>();
            foreach (var pt in pts)
            {
                var pt1 = new Autodesk.Revit.DB.XYZ(pt.Point.X, pt.Point.Y, pt.Point.Z);
                var p = plane.ProjectOnto(pt1);
                if (!p.IsWithinFace(min, max))
                {
                    notOnFace.Add(pt);
                    continue;
                }

                pt.Point = p;
                onFace.Add(pt);
            }

            var onFaceList = onFace.ToList();
            var notOnFaceList = notOnFace.ToList();

            if (onFace.Any() && notOnFace.Any())
            {
                foreach (var pt in notOnFaceList)
                {
                    Autodesk.Revit.DB.Line closestEdge = outerEdges.First();
                    var distance = double.MaxValue;
                    foreach (var edge in outerEdges)
                    {
                        var d = edge.Distance(pt.Point);
                        if (!(d < distance))
                            continue;

                        distance = d;
                        closestEdge = edge;
                    }

                    var ptOnEdge = closestEdge.Project(pt.Point).XYZPoint;
                    
                    pt.Point = ptOnEdge;
                    onFaceList.Add(pt);
                }
            }

            ptsOnFace = onFaceList;
            uvsOnFace = new List<EdgeUV>();

            return ptsOnFace.Any();
        }

        private static IEnumerable<Autodesk.Revit.DB.Line> PlanarizeArc(Autodesk.Revit.DB.Arc arc, double shortCurveTolerance)
        {
            var b1 = arc.Evaluate(0, true);
            var b2 = arc.Evaluate(0.25, true);
            var b3 = arc.Evaluate(0.5, true);
            var b4 = arc.Evaluate(0.75, true);
            var b5 = arc.Evaluate(1, true);

            // (Konrad) It's possible that an Arc length is > shortCurveTolerance but when it's
            // planarized it's line components are not. If that's the case, return a single line
            // or an empty list.
            if (b1.DistanceTo(b2) < shortCurveTolerance ||
                b2.DistanceTo(b3) < shortCurveTolerance ||
                b3.DistanceTo(b4) < shortCurveTolerance ||
                b4.DistanceTo(b5) < shortCurveTolerance)
            {
                if (b1.DistanceTo(b5) < shortCurveTolerance)
                    return new List<Autodesk.Revit.DB.Line>();

                return new List<Autodesk.Revit.DB.Line>
                {
                    Autodesk.Revit.DB.Line.CreateBound(b1, b5)
                };
            }

            return new List<Autodesk.Revit.DB.Line>
            {
                Autodesk.Revit.DB.Line.CreateBound(b1, b2),
                Autodesk.Revit.DB.Line.CreateBound(b2, b3),
                Autodesk.Revit.DB.Line.CreateBound(b3, b4),
                Autodesk.Revit.DB.Line.CreateBound(b4, b5)
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
#if !Revit2019 && !Revit2020 && !Revit2021

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

        private static List<List<EdgeLine>> GetGeometryEdges(Autodesk.Revit.DB.Element e)
        {
            var edges = new List<List<EdgeLine>>();
            using (var opt = new Autodesk.Revit.DB.Options())
            {
                opt.IncludeNonVisibleObjects = true;
                using (var geom = e.get_Geometry(opt))
                {
                    ExtractEdgesRecursively(geom, ref edges);
                }
            }

            return edges;
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

        private static void ExtractEdgesRecursively(Autodesk.Revit.DB.GeometryElement geo, ref List<List<EdgeLine>> edges)
        {
            foreach (var g in geo)
            {
                var instGeo = g as Autodesk.Revit.DB.GeometryInstance;
                if (instGeo != null)
                {
                    ExtractEdgesRecursively(instGeo.GetInstanceGeometry(), ref edges);
                    continue;
                }

                var solidGeo = g as Autodesk.Revit.DB.Solid;
                if (solidGeo != null)
                {
                    foreach (Autodesk.Revit.DB.Face f in solidGeo.Faces)
                    {
                        ProcessFace(f, ref edges);
                    }
                    continue;
                }

                var faceGeo = g as Autodesk.Revit.DB.Face;
                if (faceGeo != null)
                    ProcessFace(faceGeo, ref edges);
            }
        }

        //private static readonly double[] _params = { 0d, 0.2, 0.4, 0.6, 0.8 };
        private static readonly double[] _params = { 0d, 1d };

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

        private static void ProcessFace(Autodesk.Revit.DB.Face f, ref List<List<EdgeLine>> lines)
        {
            var faceLines = new List<EdgeLine>();
            var faceId = Guid.NewGuid();
            foreach (Autodesk.Revit.DB.EdgeArray edges in f.EdgeLoops)
            {
                for (var i = 0; i < edges.Size; i++)
                {
                    var e = edges.get_Item(i);
                    var curve = e.AsCurve();
                    switch (curve)
                    {
                        case Autodesk.Revit.DB.Line line:
                            faceLines.Add(new EdgeLine(line, faceId, $"{i}"));
                            break;
                        case Autodesk.Revit.DB.Arc arc:
                            var arcLines = PlanarizeArc(arc, 0.001).ToList();
                            for (var j = 0; j < arcLines.Count; j++)
                            {
                                var aLine = arcLines[j];
                                faceLines.Add(new EdgeLine(aLine, faceId, $"{i}_{j}"));
                            }

                            break;
                    }
                }
            }

            lines.Add(faceLines);
        }

        #endregion
    }

    [SupressImportIntoVM]
    public static class GeometryExtensions
    {
        public static double SignedDistanceTo(this Autodesk.Revit.DB.Plane plane, Autodesk.Revit.DB.XYZ p)
        {
            var v = p - plane.Origin;
            return plane.Normal.DotProduct(v);
        }

        public static Autodesk.Revit.DB.XYZ ProjectOnto(this Autodesk.Revit.DB.Plane plane, Autodesk.Revit.DB.XYZ p)
        {
            var d = plane.SignedDistanceTo(p);
            var q = p - d * plane.Normal;
            return q;
        }

        public static Autodesk.Revit.DB.UV ProjectInto(this Autodesk.Revit.DB.Plane plane, Autodesk.Revit.DB.XYZ p)
        {
            var q = plane.ProjectOnto(p);
            var o = plane.Origin;
            var d = q - o;
            var u = d.DotProduct(plane.XVec);
            var v = d.DotProduct(plane.YVec);
            return new Autodesk.Revit.DB.UV(u, v);
        }

        public static bool IsWithinFace(this Autodesk.Revit.DB.XYZ pt, Autodesk.Revit.DB.XYZ min, Autodesk.Revit.DB.XYZ max)
        {
            var epsilon = new DoubleExtensions.Epsilon(0.001);
            var minX = min.X.LE(max.X, epsilon) ? min.X : max.X;
            var minY = min.Y.LE(max.Y, epsilon) ? min.Y : max.Y;
            var minZ = min.Z.LE(max.Z, epsilon) ? min.Z : max.Z;
            var maxX = max.X.GE(min.X, epsilon) ? max.X : min.X;
            var maxY = max.Y.GE(min.Y, epsilon) ? max.Y : min.Y;
            var maxZ = max.Z.GE(min.Z, epsilon) ? max.Z : min.Z;

            var xge = pt.X.GE(minX, epsilon);
            var yge = pt.Y.GE(minY, epsilon);
            var zge = pt.Z.GE(minZ, epsilon);
            var xle = pt.X.LE(maxX, epsilon);
            var yle = pt.Y.LE(maxY, epsilon);
            var zle = pt.Z.LE(maxZ, epsilon);

            return xge && yge && zge && xle && yle && zle;
        }
    }

    [SupressImportIntoVM]
    public static class DoubleExtensions
    {
        public static bool AlmostEqualTo(this double value1, double value2)
        {
            var tolerance = 0.001;
            return Math.Abs(value1 - value2) < tolerance;
        }

        public struct Epsilon
        {
            public Epsilon(double value)
            {
                _value = value;
            }

            private double _value;

            internal bool IsEqual(double a, double b)
            {
                return (a == b) || (Math.Abs(a - b) < _value);

            }

            internal bool IsNotEqual(double a, double b)
            {
                return (a != b) && !(Math.Abs(a - b) < _value);
            }
        }

        public static bool EQ(this double a, double b, Epsilon e)
        {
            return e.IsEqual(a, b);
        }

        public static bool LE(this double a, double b, Epsilon e)
        {
            return e.IsEqual(a, b) || (a < b);
        }

        public static bool GE(this double a, double b, Epsilon e)
        {
            return e.IsEqual(a, b) || (a > b);
        }

        public static bool NE(this double a, double b, Epsilon e)
        {
            return e.IsNotEqual(a, b);
        }

        public static bool LT(this double a, double b, Epsilon e)
        {
            return e.IsNotEqual(a, b) && (a < b);
        }

        public static bool GT(this double a, double b, Epsilon e)
        {
            return e.IsNotEqual(a, b) && (a > b);
        }
    }

    [SupressImportIntoVM]
    public static class ListExtensions
    {
        public static bool SafeAdd(this List<List<Autodesk.Revit.DB.XYZ>> list, List<Autodesk.Revit.DB.XYZ> toAdd)
        {
            if (list.Contains(toAdd, new ListOfXyzComparer()))
                return false;

            list.Add(toAdd);
            return true;
        }
    }

    [SupressImportIntoVM]
    public class ListOfXyzComparer : IEqualityComparer<List<Autodesk.Revit.DB.XYZ>>
    {
        public bool Equals(List<Autodesk.Revit.DB.XYZ> x, List<Autodesk.Revit.DB.XYZ> y)
        {
            if (y == null && x == null)
                return true;
            else if (x == null || y == null)
                return false;
            else if (x.All(pt => y.Contains(pt, new XyzComparer())))
                return true;
            else
                return false;
        }

        public int GetHashCode(List<Autodesk.Revit.DB.XYZ> obj)
        {
            throw new NotImplementedException();
        }
    }

    [SupressImportIntoVM]
    public class XyzComparer : IEqualityComparer<Autodesk.Revit.DB.XYZ>
    {
        public bool Equals(Autodesk.Revit.DB.XYZ x, Autodesk.Revit.DB.XYZ y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.X.AlmostEqualTo(y.X) && x.Y.AlmostEqualTo(y.Y) && x.Z.AlmostEqualTo(y.Z);
        }

        public int GetHashCode(Autodesk.Revit.DB.XYZ obj)
        {
            return obj.X.GetHashCode() ^ obj.Y.GetHashCode() ^ obj.Z.GetHashCode();
        }
    }

    [SupressImportIntoVM]
    public class EdgePointComparer : IEqualityComparer<EdgePoint>
    {
        public bool Equals(EdgePoint x, EdgePoint y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.Point.X.AlmostEqualTo(y.Point.X) &&
                   x.Point.Y.AlmostEqualTo(y.Point.Y) &&
                   x.Point.Z.AlmostEqualTo(y.Point.Z);
        }

        public int GetHashCode(EdgePoint obj)
        {
            return obj.Point.X.GetHashCode() ^
                   obj.Point.Y.GetHashCode() ^
                   obj.Point.Z.GetHashCode();
        }
    }

    [SupressImportIntoVM]
    public static class ElementExtensions
    {
        public static bool IsPrimaryDesignOption(this Autodesk.Revit.DB.Element e)
        {
            // (Konrad) Do not process non-primary design options.
            var option = e.DesignOption;
            return option == null || option.IsPrimary;
        }

        public static double GetDoorWindowArea(this Autodesk.Revit.DB.Element insert)
        {
            var winType = (Autodesk.Revit.DB.FamilySymbol)insert.Document.GetElement(insert.GetTypeId());

            var furnitureWidthInstance = insert.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FURNITURE_WIDTH)?.AsDouble() ?? 0;
            var furnitureWidthType = winType.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FURNITURE_WIDTH)?.AsDouble() ?? 0;
            var furnWidth = furnitureWidthInstance > 0 ? furnitureWidthInstance : furnitureWidthType;
            var familyWidthInstance = insert.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FAMILY_WIDTH_PARAM)?.AsDouble() ?? 0;
            var familyWidthType = winType.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FAMILY_WIDTH_PARAM)?.AsDouble() ?? 0;
            var famWidth = familyWidthInstance > 0 ? familyWidthInstance : familyWidthType;
            var width = famWidth > 0 ? famWidth : furnWidth;
            if (width <= 0)
                width = 0.5; // set min value to 6"

            var furnitureHeightInstance = insert.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FURNITURE_HEIGHT)?.AsDouble() ?? 0;
            var furnitureHeightType = winType.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FURNITURE_HEIGHT)?.AsDouble() ?? 0;
            var furnHeight = furnitureHeightInstance > 0 ? furnitureHeightInstance : furnitureHeightType;
            var familyHeightInstance = insert.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FAMILY_HEIGHT_PARAM)?.AsDouble() ?? 0;
            var familyHeightType = winType.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FAMILY_HEIGHT_PARAM)?.AsDouble() ?? 0;
            var famHeight = familyHeightInstance > 0 ? familyHeightInstance : familyHeightType;
            var height = famHeight > 0 ? famHeight : furnHeight;
            if (height <= 0)
                height = 0.5; // set min value to 6"

            var winArea = width * height;

            return winArea;
        }

        public static List<Autodesk.Revit.DB.XYZ> GetGeometryPoints(this Autodesk.Revit.DB.Element e)
        {
            var pts = new List<Autodesk.Revit.DB.XYZ>();
            using (var opt = new Autodesk.Revit.DB.Options())
            {
                opt.IncludeNonVisibleObjects = false;
                opt.ComputeReferences = false;
                opt.DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Coarse;
                using (var geom = e.get_Geometry(opt))
                {
                    ExtractPtsRecursively(geom, e, ref pts);
                }
            }

            return pts.Distinct(new XyzComparer()).ToList();
        }

        private static void ExtractPtsRecursively(
            Autodesk.Revit.DB.GeometryElement geo,
            Autodesk.Revit.DB.Element element,
            ref List<Autodesk.Revit.DB.XYZ> pts,
            bool includeLines = false)
        {
            foreach (var g in geo)
            {
                var instGeo = g as Autodesk.Revit.DB.GeometryInstance;
                if (instGeo != null)
                {
                    ExtractPtsRecursively(instGeo.GetInstanceGeometry(), element, ref pts, includeLines);
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
                if (faceGeo != null)
                {
                    ProcessFace(faceGeo, ref pts);
                }

                var meshGeo = g as Autodesk.Revit.DB.Mesh;
                if (meshGeo != null)
                    pts.AddRange(meshGeo.Vertices);

                if (!includeLines)
                    continue;

                var lineGeo = g as Autodesk.Revit.DB.Curve;
                if (lineGeo != null && lineGeo.IsBound)
                    pts.AddRange(new List<Autodesk.Revit.DB.XYZ> { lineGeo.GetEndPoint(0), lineGeo.GetEndPoint(1) });
            }
        }

        private static readonly double[] Params = { 0d, 0.2, 0.4, 0.6, 0.8, 1.0 };

        private static void ProcessFace(Autodesk.Revit.DB.Face f, ref List<Autodesk.Revit.DB.XYZ> pts)
        {
            foreach (Autodesk.Revit.DB.EdgeArray edges in f.EdgeLoops)
            {
                foreach (Autodesk.Revit.DB.Edge e in edges)
                {
                    pts.AddRange(Params.Select(p => e.Evaluate(p)));
                }
            }
        }
    }

    [SupressImportIntoVM]
    public class ApertureWrapper
    {
        public Autodesk.Revit.DB.Element Self { get; set; }
        public string UniqueId { get; }
        public List<Autodesk.Revit.DB.XYZ> Locations { get; set; }  = new List<Autodesk.Revit.DB.XYZ>();
        public bool IsPrimaryDesignOption { get; set; }
        public ApertureTypes ApertureType { get; set; }
        public double Area { get; set; }
        public bool HasArea { get; set; }
        public List<Autodesk.Revit.DB.XYZ> GeometryPoints { get; set; }

        public ApertureWrapper(Autodesk.Revit.DB.Element w)
        {
            Self = w;
            UniqueId = w.UniqueId;
            IsPrimaryDesignOption = w.IsPrimaryDesignOption();

            if (w.Category.Id.IntegerValue == Autodesk.Revit.DB.BuiltInCategory.OST_Windows.GetHashCode())
            {
                ApertureType = ApertureTypes.Window;
                Area = w.GetDoorWindowArea();
                HasArea = true;
            }
            else if (w.Category.Id.IntegerValue == Autodesk.Revit.DB.BuiltInCategory.OST_CurtainWallPanels.GetHashCode())
            {
                ApertureType = ApertureTypes.CurtainWallPanel;
                var areaParam = w.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.HOST_AREA_COMPUTED);
                if (areaParam.HasValue)
                {
                    Area = areaParam.AsDouble();
                    HasArea = true;
                }
            }
            else if (w.Category.Id.IntegerValue == Autodesk.Revit.DB.BuiltInCategory.OST_Doors.GetHashCode())
            {
                ApertureType = ApertureTypes.Door;
                Area = w.GetDoorWindowArea();
                HasArea = true;
            }
            else
            {
                ApertureType = ApertureTypes.None;
            }

            var location = (w.Location as Autodesk.Revit.DB.LocationPoint)?.Point;
            if (location == null)
            {
                if (ApertureType == ApertureTypes.Door && w is Autodesk.Revit.DB.FamilyInstance cwDoor)
                {
                    // (Konrad) This is likely a CW Door. A TotalTransform will be at the bottom/center
                    // of the CW Panel. That's more accurate than a center of the BB.
                    Locations.Add(cwDoor.GetTotalTransform().Origin);
                }
                else
                {
                    var bb = w.get_BoundingBox(null);
                    if (bb == null)
                    {
                        // (Konrad) Location/BB would be null for non-area cw panels.
                        Locations.Add(Autodesk.Revit.DB.XYZ.Zero);
                    }
                    else
                    {
                        Locations.Add(new Autodesk.Revit.DB.XYZ(bb.Min.X, bb.Min.Y, bb.Min.Z));
                        Locations.Add(new Autodesk.Revit.DB.XYZ(bb.Max.X, bb.Min.Y, bb.Min.Z));
                        Locations.Add(new Autodesk.Revit.DB.XYZ(bb.Min.X, bb.Max.Y, bb.Min.Z));
                        Locations.Add(new Autodesk.Revit.DB.XYZ(bb.Max.X, bb.Max.Y, bb.Min.Z));

                        Locations.Add(new Autodesk.Revit.DB.XYZ(bb.Min.X, bb.Min.Y, bb.Max.Z));
                        Locations.Add(new Autodesk.Revit.DB.XYZ(bb.Max.X, bb.Min.Y, bb.Max.Z));
                        Locations.Add(new Autodesk.Revit.DB.XYZ(bb.Min.X, bb.Max.Y, bb.Max.Z));
                        Locations.Add(new Autodesk.Revit.DB.XYZ(bb.Max.X, bb.Max.Y, bb.Max.Z));
                    }
                }
            }

            Locations.Add(location);
            // Location = location;
        }

        public void SetGeometry()
        {
            GeometryPoints = Self.GetGeometryPoints();
        }

        public override bool Equals(object obj)
        {
            return obj is ApertureWrapper item && UniqueId.Equals(item.UniqueId);
        }

        public override int GetHashCode()
        {
            return UniqueId.GetHashCode();
        }
    }

    [SupressImportIntoVM]
    public enum ApertureTypes
    {
        None,
        Window,
        CurtainWallPanel,
        Door
    }
}
