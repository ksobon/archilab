using archilab.Revit.Geometry;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using DynamoServices;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Element = Revit.Elements.Element;
using FamilyInstance = Autodesk.Revit.DB.FamilyInstance;
using FamilyType = Revit.Elements.FamilyType;
using Level = Revit.Elements.Level;
using Line = Autodesk.DesignScript.Geometry.Line;
using Point = Autodesk.DesignScript.Geometry.Point;
using Surface = Autodesk.DesignScript.Geometry.Surface;
using View = Revit.Elements.Views.View;

// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Wrapper class for Family Instances.
    /// </summary>
    [RegisterForTrace]
    public class FamilyInstances : AbstractFamilyInstance
    {
        internal Autodesk.Revit.DB.FamilyInstance InternalFamilyInstance { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        protected FamilyInstances(Autodesk.Revit.DB.FamilyInstance instance)
        {
            SafeInit(() => InitFamilyInstance(instance));
        }

        internal FamilyInstances(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.Line line, Autodesk.Revit.DB.Level level)
        {
            SafeInit(() => InitFamilyInstance(fs, line, level));
        }

        internal FamilyInstances(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.XYZ point, Autodesk.Revit.DB.View view)
        {
            SafeInit(() => InitFamilyInstance(fs, point, view));
        } 
        
        internal FamilyInstances(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.XYZ point, Autodesk.Revit.DB.Element host, Autodesk.Revit.DB.Level level)
        {
            SafeInit(() => InitFamilyInstance(fs, point, host, level));
        }
        
        internal FamilyInstances(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.XYZ point, Autodesk.Revit.DB.Reference reference, Autodesk.Revit.DB.XYZ referenceDir)
        {
            SafeInit(() => InitFamilyInstance(fs, point, reference, referenceDir));
        }

        private void InitFamilyInstance(Autodesk.Revit.DB.FamilyInstance instance)
        {
            InternalSetFamilyInstance(instance);
        }

        private void InitFamilyInstance(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.Line line, Autodesk.Revit.DB.Level level)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldFam = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(DocumentManager.Instance.CurrentDBDocument);

            //There was a point, rebind to that, and adjust its position
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                InternalSetLevel(level);
                InternalSetFamilySymbol(fs);
                InternalSetPosition(line);
                return;
            }

            //Phase 2- There was no existing point, create one
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            //If the symbol is not active, then activate it
            if (!fs.IsActive) fs.Activate();

            var fi = DocumentManager.Instance.CurrentDBDocument.Create.NewFamilyInstance(line, fs, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            ((Autodesk.Revit.DB.LocationCurve)fi.Location).Curve = line;

            InternalSetFamilyInstance(fi);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        private void InitFamilyInstance(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.XYZ point, Autodesk.Revit.DB.View view)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldFam = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(DocumentManager.Instance.CurrentDBDocument);

            //There was a point, rebind to that, and adjust its position
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                InternalSetFamilySymbol(fs);
                return;
            }

            //Phase 2- There was no existing point, create one
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            //If the symbol is not active, then activate it
            if (!fs.IsActive)
                fs.Activate();

            var fi = DocumentManager.Instance.CurrentDBDocument.Create.NewFamilyInstance(point, fs, view);

            InternalSetFamilyInstance(fi);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        private void InitFamilyInstance(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.XYZ point, Autodesk.Revit.DB.Element host, Autodesk.Revit.DB.Level level) 
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var oldFam = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(doc);

            //There was a point, rebind to that, and adjust its position
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                InternalSetFamilySymbol(fs);
                return;
            }

            //Phase 2- There was no existing point, create one
            TransactionManager.Instance.EnsureInTransaction(doc);

            //If the symbol is not active, then activate it
            if (!fs.IsActive)
                fs.Activate();

            var fi = doc.Create.NewFamilyInstance(point, fs, host, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            InternalSetFamilyInstance(fi);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        private void InitFamilyInstance(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.XYZ point, Autodesk.Revit.DB.Reference reference, Autodesk.Revit.DB.XYZ referenceDir)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var oldFam = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(doc);

            //There was a point, rebind to that, and adjust its position
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                InternalSetFamilySymbol(fs);
                return;
            }

            //Phase 2- There was no existing point, create one
            TransactionManager.Instance.EnsureInTransaction(doc);

            //If the symbol is not active, then activate it
            if (!fs.IsActive)
                fs.Activate();

            var fi = doc.Create.NewFamilyInstance(reference, point, referenceDir, fs);

            InternalSetFamilyInstance(fi);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        private void InternalSetLevel(Autodesk.Revit.DB.Level level)
        {
            if (InternalFamilyInstance.LevelId.Compare(level.Id) == 0) return;

            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            InternalFamilyInstance.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FAMILY_LEVEL_PARAM).Set(level.Id);

            TransactionManager.Instance.TransactionTaskDone();
        }

        private void InternalSetPosition(Autodesk.Revit.DB.Curve pos)
        {
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            if (InternalFamilyInstance.Location is Autodesk.Revit.DB.LocationCurve lp && lp.Curve != pos) lp.Curve = pos;

            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// New Family Instance by Curve
        /// </summary>
        /// <param name="familyType">Family Type to be applied to new Family Instance.</param>
        /// <param name="line">Line to place Family Instance at.</param>
        /// <param name="level">Level to associate Family Instance with.</param>
        /// <returns>New Family Instance.</returns>
        [NodeCategory("Action")]
        public static Element ByLine(FamilyType familyType, Line line, Level level)
        {
            if (familyType == null)
            {
                throw new ArgumentNullException(nameof(familyType));
            }

            var symbol = familyType.InternalElement as Autodesk.Revit.DB.FamilySymbol;
            var locationLine = line.ToRevitType() as Autodesk.Revit.DB.Line;
            var hostLevel = level.InternalElement as Autodesk.Revit.DB.Level;

            return new FamilyInstances(symbol, locationLine, hostLevel).InternalElement.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="familyType"></param>
        /// <param name="point"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element ByView(FamilyType familyType, Point point, View view)
        {
            if (familyType == null)
                throw new ArgumentNullException(nameof(familyType));
            if (point == null)
                throw new ArgumentNullException(nameof(point));
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var symbol = familyType.InternalElement as Autodesk.Revit.DB.FamilySymbol;
            var pt = point.ToRevitType();
            var v = view.InternalElement as Autodesk.Revit.DB.View;

            return new FamilyInstances(symbol, pt, v).InternalElement.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="familyType"></param>
        /// <param name="point"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element ByHostAndPoint(FamilyType familyType, Point point, Element host)
        {
            if (familyType == null)
                throw new ArgumentNullException(nameof(familyType));
            if (point == null)
                throw new ArgumentNullException(nameof(point));
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var symbol = familyType.InternalElement as Autodesk.Revit.DB.FamilySymbol;
            var pt = point.ToRevitType();
            var h = host.InternalElement;
            var level = doc.GetElement(h.LevelId) as Autodesk.Revit.DB.Level;

            return new FamilyInstances(symbol, pt, h, level).InternalElement.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="familyType"></param>
        /// <param name="point"></param>
        /// <param name="surface"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element ByFaceAndPoint(FamilyType familyType, Point point, Surface surface)
        {
            if (familyType == null)
                throw new ArgumentNullException(nameof(familyType));
            if (point == null)
                throw new ArgumentNullException(nameof(point));
            if (surface == null)
                throw new ArgumentNullException(nameof(surface));

            var symbol = familyType.InternalElement as Autodesk.Revit.DB.FamilySymbol;
            var pt = point.ToRevitType();
            var reference = surface.Tags.LookupTag("RevitFaceReference") as Autodesk.Revit.DB.Reference;
            var faceNormal = surface.NormalAtPoint(point);
            var up = Vector.ZAxis();

            Autodesk.Revit.DB.XYZ referenceDir;
            if (Math.Abs(faceNormal.Dot(up)) > 0.9999) // horizontal
                referenceDir = Autodesk.Revit.DB.XYZ.BasisX;
            else
                referenceDir = faceNormal.Cross(up).ToXyz();

            return new FamilyInstances(symbol, pt, reference, referenceDir).InternalElement.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element FlipFacingOrientation(Element element)
        {
            if (!(element.InternalElement is Autodesk.Revit.DB.FamilyInstance e))
                throw new ArgumentNullException(nameof(element));

            if (!e.CanFlipFacing)
                return element;

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            e.flipFacing();
            TransactionManager.Instance.TransactionTaskDone();

            return element;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element FlipHandOrientation(Element element)
        {
            if (!(element.InternalElement is Autodesk.Revit.DB.FamilyInstance e))
                throw new ArgumentNullException(nameof(element));

            if (!e.CanFlipHand)
                return element;

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            e.flipHand();
            TransactionManager.Instance.TransactionTaskDone();

            return element;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static List<Connectors> Connectors(Element element)
        {
            if (!(element.InternalElement is Autodesk.Revit.DB.FamilyInstance e))
                throw new ArgumentNullException(nameof(element));

            return (from Autodesk.Revit.DB.Connector conn in e.MEPModel.ConnectorManager.Connectors
                select new Connectors(conn)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static List<Element> SubComponents(Element element)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var e = element.InternalElement;
            switch (e)
            {
                case Autodesk.Revit.DB.FamilyInstance fi:
                    return fi.GetSubComponentIds().Select(x => doc.GetElement(x).ToDSType(true)).ToList();
                case Autodesk.Revit.DB.Architecture.Stairs s:
                    var stairComponents = s.GetStairsLandings().Select(x => doc.GetElement(x).ToDSType(true)).ToList();
                    stairComponents.AddRange(s.GetStairsRuns().Select(x => doc.GetElement(x).ToDSType(true)));
                    stairComponents.AddRange(s.GetStairsSupports().Select(x => doc.GetElement(x).ToDSType(true)));
                    return stairComponents;
                case Autodesk.Revit.DB.Architecture.Railing r:
                    var railComponents = r.GetHandRails().Select(x => doc.GetElement(x).ToDSType(true)).ToList();
                    railComponents.Add(doc.GetElement(r.TopRail).ToDSType(true));
                    return railComponents;
                case Autodesk.Revit.DB.BeamSystem b:
                    return b.GetBeamIds().Select(x => doc.GetElement(x).ToDSType(true)).ToList();
                default:
                    return new List<Element>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool HasSuperComponent(Element element)
        {
            if (!(element.InternalElement is Autodesk.Revit.DB.FamilyInstance e))
                throw new ArgumentNullException(nameof(element));

            return e.SuperComponent != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static Element SuperComponent(Element element)
        {
            if (!(element.InternalElement is Autodesk.Revit.DB.FamilyInstance e))
                throw new ArgumentNullException(nameof(element));

            return e.SuperComponent?.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static List<Point> Points(Element element)
        {
            if (!(element.InternalElement is Autodesk.Revit.DB.FamilyInstance e))
                throw new ArgumentNullException(nameof(element));

            var points = FamilyInstanceUtilities.GetGeometryPoints(e);
            return points.SelectMany(x => x.ToPoints()).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static List<Point> FlatPoints(Element element)
        {
            if (!(element.InternalElement is Autodesk.Revit.DB.FamilyInstance e))
                throw new ArgumentNullException(nameof(element));

            var results = new List<Point>();
            var points = FamilyInstanceUtilities.GetGeometryPoints(e);
            foreach (var ptList in points)
            {
                foreach (var pt in ptList)
                {
                    results.Add(Point.ByCoordinates(pt.X, pt.Y, 0));
                }
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static List<Point> HullPoints(Element element)
        {
            if (!(element.InternalElement is Autodesk.Revit.DB.FamilyInstance e))
                throw new ArgumentNullException(nameof(element));

            var vectors = new List<Vector2d>();
            var points = FamilyInstanceUtilities.GetGeometryPoints(e);
            foreach (var ptList in points)
            {
                foreach (var pt in ptList)
                {
                    var v = new Vector2d();
                    v.X = pt.X;
                    v.Y = pt.Y;
                    vectors.Add(v);
                }
            }

            var hullPoints = GeoAlgos.MonotoneChainConvexHull(vectors.ToArray());

            var results = new HashSet<Point>();
            foreach (var hp in hullPoints)
            {
                var pt = Point.ByCoordinates(hp.X, hp.Y, 0);
                if (results.Contains(pt, new PointComparer()))
                    continue;
                results.Add(pt);
            }

            return results.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static List<Point> MinBoundingBox(Element element)
        {
            if (!(element.InternalElement is Autodesk.Revit.DB.FamilyInstance e))
                throw new ArgumentNullException(nameof(element));

            var vectors = new List<Vector2d>();
            var points = FamilyInstanceUtilities.GetGeometryPoints(e);

            var minZ = double.MaxValue;
            foreach (var ptList in points)
            {
                foreach (var pt in ptList)
                {
                    var v = new Vector2d();
                    v.X = pt.X;
                    v.Y = pt.Y;
                    vectors.Add(v);

                    if (pt.Z < minZ)
                        minZ = pt.Z;
                }
            }

            var results = new HashSet<Point>();
            var bbBox = MinimalBoundingBox.Calculate(vectors.ToArray());

            foreach (var v in bbBox.Points)
            {
                var pt = Point.ByCoordinates(v.X, v.Y, minZ);
                if (results.Contains(pt, new PointComparer()))
                    continue;

                results.Add(pt);
            }

            return results.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static double Height(Element element)
        {
            if (!(element.InternalElement is Autodesk.Revit.DB.FamilyInstance e))
                throw new ArgumentNullException(nameof(element));

            var points = FamilyInstanceUtilities.GetGeometryPoints(e);

            var minZ = double.MaxValue;
            var maxZ = double.MinValue;
            foreach (var ptList in points)
            {
                foreach (var pt in ptList)
                {
                    if (pt.Z < minZ)
                        minZ = pt.Z;
                    if (pt.Z > maxZ)
                        maxZ = pt.Z;
                }
            }

            return maxZ - minZ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static List<Point> LongEdge(List<Point> bb)
        {
            var pt1 = bb[0];
            var pt2 = bb[1];
            var pt3 = bb[2];
            var pt4 = bb[3];

            if (pt1.DistanceTo(pt2) > pt1.DistanceTo(pt4))
            {
                var start = FamilyInstanceUtilities.GetMidpoint(pt1, pt4);
                var end = FamilyInstanceUtilities.GetMidpoint(pt2, pt3);
                return new List<Point> { start, end };
            }
            else
            {
                var start = FamilyInstanceUtilities.GetMidpoint(pt1, pt2);
                var end = FamilyInstanceUtilities.GetMidpoint(pt3, pt4);
                return new List<Point> { start, end };
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [SupressImportIntoVM]
    public class PointComparer : IEqualityComparer<Point>
    {
        public bool Equals(Point x, Point y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.X.AlmostEqualTo(y.X) && x.Y.AlmostEqualTo(y.Y) && x.Z.AlmostEqualTo(y.Z);
        }

        public int GetHashCode(Point obj)
        {
            return obj.X.GetHashCode() ^ obj.Y.GetHashCode() ^ obj.Z.GetHashCode();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [SupressImportIntoVM]
    public class FamilyInstanceUtilities
    {
        public static Point GetMidpoint(Point p1, Point p2)
        {
            return Point.ByCoordinates(
                (p1.X + p2.X) / 2.0,
                (p1.Y + p2.Y) / 2.0,
                (p1.Z + p2.Z) / 2.0
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static List<List<Autodesk.Revit.DB.XYZ>> GetGeometryPoints(
            Autodesk.Revit.DB.Element e)
        {
            var pts = new List<List<Autodesk.Revit.DB.XYZ>>();
            using (var opt = new Autodesk.Revit.DB.Options())
            {
                opt.IncludeNonVisibleObjects = false;
                opt.ComputeReferences = false;
                opt.DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Medium;
                using (var geom = e.get_Geometry(opt))
                {
                    ExtractPtsRecursively(geom, ref pts);
                }

                var doc = e.Document;
                if (!(e is Autodesk.Revit.DB.FamilyInstance fi))
                    return pts;

                // (Konrad) To save some time (performance) I am only looking for nested
                // families if the original search for geometry didn't return any points.
                if (pts.Any())
                    return pts;

                foreach (var id in fi.GetSubComponentIds())
                {
                    var current = doc.GetElement(id);
                    using (var geom = current.get_Geometry(opt))
                    {
                        ExtractPtsRecursively(geom, ref pts);
                    }
                }
            }

            return pts;
        }

        private static void ExtractPtsRecursively(
            Autodesk.Revit.DB.GeometryElement geo,
            ref List<List<Autodesk.Revit.DB.XYZ>> pts)
        {
            foreach (var g in geo)
            {
                var instGeo = g as Autodesk.Revit.DB.GeometryInstance;
                if (instGeo != null)
                {
                    ExtractPtsRecursively(instGeo.GetInstanceGeometry(), ref pts);
                    continue;
                }

                var solidGeo = g as Autodesk.Revit.DB.Solid;
                if (solidGeo != null && solidGeo.Faces.Size > 0)
                {
                    var solidPts = new List<Autodesk.Revit.DB.XYZ>();
                    foreach (Autodesk.Revit.DB.Face f in solidGeo.Faces)
                    {
                        ProcessFace(f, ref solidPts);
                    }

                    if (solidPts.Any())
                        pts.Add(solidPts);
                }
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

    /// <summary>
    /// 
    /// </summary>
    [SupressImportIntoVM]
    public static class GeoAlgos
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Vector2d[] MonotoneChainConvexHull(Vector2d[] points)
        {
            //sort vectors
            Array.Sort<Vector2d>(points);
            var hullPoints = new Vector2d[2 * points.Length];

            //break if only one point as input
            if (points.Length <= 1)
                return points;

            int pointLength = points.Length;
            int counter = 0;

            //iterate for lowerHull
            for (var i = 0; i < pointLength; ++i)
            {
                while (counter >= 2 && Cross(hullPoints[counter - 2],
                           hullPoints[counter - 1],
                           points[i]) <= 0)
                    counter--;
                hullPoints[counter++] = points[i];
            }

            //iterate for upperHull
            for (int i = pointLength - 2, j = counter + 1; i >= 0; i--)
            {
                while (counter >= j && Cross(hullPoints[counter - 2],
                           hullPoints[counter - 1],
                           points[i]) <= 0)
                    counter--;
                hullPoints[counter++] = points[i];
            }

            //remove duplicate start points
            var result = new Vector2d[counter - 1];
            Array.Copy(hullPoints, 0, result, 0, counter - 1);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Cross(Vector2d o, Vector2d a, Vector2d b)
        {
            return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
        }

        public static bool IsDoubleEqual(double d1, double d2)
        {
            return Math.Abs(d1 - d2) < double.Epsilon;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [SupressImportIntoVM]
    public class Vector2d : IComparable
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector2d()
        {
        }

        public Vector2d(Vector2d v) : this()
        {
            this.X = v.X;
            this.Y = v.Y;
        }

        public Vector2d(double x, double y) : this()
        {
            this.X = x;
            this.Y = y;
        }

        public double this[int i]
        {
            get
            {
                if (i == 0)
                    return X;
                else
                    return Y;
            }
            set
            {
                if (i == 0)
                    X = value;
                else
                    Y = value;
            }
        }

        public Vector2d Copy()
        {
            return new Vector2d(X, Y);
        }

        public override string ToString()
        {
            return string.Format("X={0}, Y={1}", X, Y);
        }

        public override bool Equals(object obj)
        {
            var v = obj as Vector2d;
            if (v == null)
                return false;

            var result = true;
            if (GeoAlgos.IsDoubleEqual(X, v.X)
                && GeoAlgos.IsDoubleEqual(Y, v.Y))
                result = false;

            return result;
        }

        public override int GetHashCode()
        {
            var result = 1;
            result = 31 * result + BitConverter.ToInt32(BitConverter.GetBytes(this.X), 0);
            result = 31 * result + BitConverter.ToInt32(BitConverter.GetBytes(this.Y), 0);
            return result;
        }

        // math
        public Vector2d Add(Vector2d v)
        {
            return new Vector2d(X + v.X, Y + v.Y);
        }

        public Vector2d Subtract(Vector2d v)
        {
            return new Vector2d(X - v.X, Y - v.Y);
        }

        public Vector2d Multiply(double s)
        {
            return new Vector2d(X * s, Y * s);
        }

        public Vector2d Divide(double s)
        {
            return new Vector2d(X / s, Y / s);
        }

        public double Length()
        {
            var sum = X * X + Y * Y;
            return (double)Math.Sqrt(sum);
        }

        public double Dot(Vector2d v)
        {
            return X * v.X + Y * v.Y;
        }

        public double Cross(Vector2d v)
        {
            return X * v.Y - Y * v.X;
        }

        public double Distance(Vector2d v)
        {
            var dx = X - v.X;
            var dy = Y - v.Y;
            return (double)Math.Sqrt(dx * dx + dy * dy);
        }

        public double Coincidence(Vector2d v)
        {
            return (X - v.X) * (X - v.X) + (Y - v.Y) * (Y - v.Y);
        }

        public Vector2d Normalize()
        {
            var m = Length();
            if (Math.Abs(m) > double.Epsilon && Math.Abs(m - 1d) > double.Epsilon)
            {
                return Divide(m);
            }
            return new Vector2d(this);
        }

        public Vector2d Pow(double power = 2)
        {
            return new Vector2d(Math.Pow(X, power), Math.Pow(Y, power));
        }

        // operation overloading
        public static Vector2d operator +(Vector2d v1, Vector2d v2)
        {
            return v1.Add(v2);
        }

        public static Vector2d operator -(Vector2d v1, Vector2d v2)
        {
            return v1.Subtract(v2);
        }

        public static Vector2d operator *(Vector2d v1, double s)
        {
            return v1.Multiply(s);
        }

        public static Vector2d operator /(Vector2d v1, double s)
        {
            return v1.Divide(s);
        }

        public static double operator *(Vector2d v1, Vector2d v2)
        {
            return v1.Dot(v2);
        }

        public int CompareTo(object obj)
        {
            var v = obj as Vector2d;
            if (v == null)
                throw new ArgumentException();

            if (GeoAlgos.IsDoubleEqual(X, v.X))
            {
                //check y
                if (GeoAlgos.IsDoubleEqual(Y, v.Y))
                    return 0;

                if (Y < v.Y)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }

            if (X < v.X)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [SupressImportIntoVM]
    public class Polygon2d
    {
        public List<Vector2d> Points { get; set; } = new List<Vector2d>();

        public Polygon2d()
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [SupressImportIntoVM]
    public class Rectangle2d
    {
        public Vector2d Location { get; set; }
        public Vector2d Size { get; set; }

        public Rectangle2d()
        {
        }

        public Rectangle2d(Vector2d a, Vector2d c) : this()
        {
            Location = a;
            Size = c - a;
        }

        public double Area()
        {
            return Size.X * Size.Y;
        }

        public Vector2d[] Points
        {
            get
            {
                return new[] {
                    new Vector2d (Location.X, Location.Y),
                    new Vector2d (Location.X + Size.X, Location.Y),
                    new Vector2d (Location.X + Size.X, Location.Y + Size.Y),
                    new Vector2d (Location.X, Location.Y + Size.Y)
                };
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [SupressImportIntoVM]
    public class Line2d
    {
        /// <summary>
        /// Position Vector of line.
        /// </summary>
        /// <value>Position Vector</value>
        public Vector2d A { get; set; }

        /// <summary>
        /// Direction Vector of the line.
        /// B - A
        /// </summary>
        /// <value>The direction.</value>
        public Vector2d R { get; set; }

        public Line2d(Vector2d position, Vector2d direction)
        {
            A = position;
            R = direction;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [SupressImportIntoVM]
    public class Segment2d : Line2d
    {
        /// <summary>
        /// Second Point of Line.
        /// </summary>
        /// <value>The b.</value>
        public Vector2d B { get; set; }

        public Segment2d(Vector2d point1, Vector2d point2) :
            base(point1, point2 - point1)
        {
            B = point2;
        }

        public double Length()
        {
            return Math.Abs(A.Distance(B));
        }

        public override string ToString()
        {
            return string.Format("[Segment2d: A={0} B={1}]", A, B);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [SupressImportIntoVM]
    public static class MinimalBoundingBox
    {
        /// <summary>
        /// Calculates the minimum bounding box.
        /// </summary>
        /// <param name="points">Bounding Box.</param>
        public static Polygon2d Calculate(Vector2d[] points)
        {
            //calculate the convex hull
            var hullPoints = GeoAlgos.MonotoneChainConvexHull(points);

            //check if no bounding box available
            if (hullPoints.Length <= 1)
                return new Polygon2d { Points = hullPoints.ToList() };

            Rectangle2d minBox = null;
            var minAngle = 0d;

            //foreach edge of the convex hull
            for (var i = 0; i < hullPoints.Length; i++)
            {
                var nextIndex = i + 1;

                var current = hullPoints[i];
                var next = hullPoints[nextIndex % hullPoints.Length];

                var segment = new Segment2d(current, next);

                //min / max points
                var top = double.MinValue;
                var bottom = double.MaxValue;
                var left = double.MaxValue;
                var right = double.MinValue;

                //get angle of segment to x-axis
                var angle = AngleToXAxis(segment);

                //rotate every point and get min and max values for each direction
                foreach (var p in hullPoints)
                {
                    var rotatedPoint = RotateToXAxis(p, angle);

                    top = Math.Max(top, rotatedPoint.Y);
                    bottom = Math.Min(bottom, rotatedPoint.Y);

                    left = Math.Min(left, rotatedPoint.X);
                    right = Math.Max(right, rotatedPoint.X);
                }

                //create axis aligned bounding box
                var box = new Rectangle2d(new Vector2d(left, bottom), new Vector2d(right, top));

                if (minBox == null || minBox.Area() > box.Area())
                {
                    minBox = box;
                    minAngle = angle;
                }
            }

            //rotate axis aligned box back
            var minimalBoundingBox = new Polygon2d
            {
                Points = minBox.Points.Select(p => RotateToXAxis(p, -minAngle)).ToList()
            };

            return minimalBoundingBox;
        }

        /// <summary>
        /// Calculates the angle to the X axis.
        /// </summary>
        /// <returns>The angle to the X axis.</returns>
        /// <param name="s">The segment to get the angle from.</param>
        static double AngleToXAxis(Segment2d s)
        {
            var delta = s.A - s.B;
            return -Math.Atan(delta.Y / delta.X);
        }

        /// <summary>
        /// Rotates vector by an angle to the x-Axis
        /// </summary>
        /// <returns>Rotated vector.</returns>
        /// <param name="v">Vector to rotate.</param>
        /// <param name="angle">Angle to trun by.</param>
        static Vector2d RotateToXAxis(Vector2d v, double angle)
        {
            var newX = v.X * Math.Cos(angle) - v.Y * Math.Sin(angle);
            var newY = v.X * Math.Sin(angle) + v.Y * Math.Cos(angle);

            return new Vector2d(newX, newY);
        }
    }
}
