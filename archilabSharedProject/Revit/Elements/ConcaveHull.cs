using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace archilab.Revit.Elements
{
    public class EdgeLine
    {
        public Autodesk.Revit.DB.Line Line { get; set; }
        public Guid FaceId { get; set; }
        public string EdgeId { get; set; }

        public EdgeLine(Autodesk.Revit.DB.Line l, Guid faceId, string edgeId)
        {
            Line = l;
            FaceId = faceId;
            EdgeId = edgeId;
        }
    }

    public class EdgePoint
    {
        public Autodesk.Revit.DB.XYZ Point { get; set; }
        public Guid FaceId { get; set; }
        public string EdgeId { get; set; }
        public int EndId { get; set; } // 0 = Start, 1 = End

        public EdgePoint(Autodesk.Revit.DB.XYZ pt, Guid faceId, string edgeId, int endId)
        {
            Point = pt;
            FaceId = faceId;
            EdgeId = edgeId;
            EndId = endId;
        }
    }

    public class EdgeUV
    {
        public Autodesk.Revit.DB.UV UV { get; set; }
        public int FaceId { get; set; }
        public int EdgeId { get; set; }
        public int EndId { get; set; } // 0 = Start, 1 = End

        public EdgeUV(Autodesk.Revit.DB.UV uv, int faceId, int edgeId, int endId)
        {
            UV = uv;
            FaceId = faceId;
            EdgeId = edgeId;
            EndId = endId;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ConcaveHull
    {
        /// <summary>
        /// 
        /// </summary>
        internal ConcaveHull()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="concavity"></param>
        /// <param name="scaleFactor"></param>
        /// <returns></returns>
        public static List<Curve> ConcaveHullFromPoints(List<UV> pts, double concavity, int scaleFactor)
        {
            var nodes = new List<Node>();
            for (var i = 0; i < pts.Count; i++)
            {
                var pt = pts[i];
                nodes.Add(new Node(pt.U, pt.V, i));
            }

            var hull = new Hull();
            hull.SetConvexHull(nodes);
            var lines = hull.SetConcaveHull(concavity, scaleFactor);

            var result = new List<Curve>();
            foreach (var l in lines)
            {
                var start = Point.ByCoordinates(l.nodes[0].x, l.nodes[0].y);
                var end = Point.ByCoordinates(l.nodes[1].x, l.nodes[1].y);
                var dsLine = Line.ByStartPointEndPoint(start, end);
                result.Add(dsLine);
            }

            return result;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static List<Curve> ConvexHullFromPoints(List<UV> pts)
        {
            var nodes = new List<Node>();
            for (var i = 0; i < pts.Count; i++)
            {
                var pt = pts[i];
                nodes.Add(new Node(pt.U, pt.V, i));
            }

            var hull = new Hull();
            hull.SetConvexHull(nodes);

            var result = new List<Curve>();
            foreach (var l in hull.HullEdges)
            {
                var start = Point.ByCoordinates(l.nodes[0].x, l.nodes[0].y);
                var end = Point.ByCoordinates(l.nodes[1].x, l.nodes[1].y);
                var dsLine = Line.ByStartPointEndPoint(start, end);
                result.Add(dsLine);
            }

            return result;
        }
    }

    [SupressImportIntoVM]
    public class Hull
    {
        public List<Node> UnusedNodes = new List<Node>();
        public List<HullLine> HullEdges = new List<HullLine>();
        public List<HullLine> HullConcaveEdges = new List<HullLine>();

        public List<HullLine> GetHull(List<Node> nodes)
        {
            var convexH = new List<Node>();
            var exitLines = new List<HullLine>();

            convexH = new List<Node>();
            convexH.AddRange(GrahamScan.convexHull(nodes));
            for (var i = 0; i < convexH.Count - 1; i++)
            {
                exitLines.Add(new HullLine(convexH[i], convexH[i + 1]));
            }
            exitLines.Add(new HullLine(convexH[0], convexH[convexH.Count - 1]));
            return exitLines;
        }

        public void SetConvexHull(List<Node> nodes)
        {
            UnusedNodes.AddRange(nodes);
            HullEdges.AddRange(GetHull(nodes));
            foreach (var line in HullEdges)
            {
                foreach (var node in line.nodes)
                {
                    UnusedNodes.RemoveAll(a => a.id == node.id);
                }
            }
        }

        public List<HullLine> SetConcaveHull(double concavity, int scaleFactor)
        {
            /* Run setConvHull before! 
             * Concavity is a value used to restrict the concave angles 
             * It can go from -1 (no concavity) to 1 (extreme concavity) 
             * Avoid concavity == 1 if you don't want 0º angles
             * */
            bool aLineWasDividedInTheIteration;
            HullConcaveEdges.AddRange(HullEdges);
            do
            {
                aLineWasDividedInTheIteration = false;
                for (var linePositionInHull = 0; linePositionInHull < HullConcaveEdges.Count && !aLineWasDividedInTheIteration; linePositionInHull++)
                {
                    var line = HullConcaveEdges[linePositionInHull];
                    var nearbyPoints = HullFunctions.getNearbyPoints(line, UnusedNodes, scaleFactor);
                    var dividedLine = HullFunctions.getDividedLine(line, nearbyPoints, HullConcaveEdges, concavity);
                    if (dividedLine.Count > 0)
                    { // Line divided!
                        aLineWasDividedInTheIteration = true;
                        UnusedNodes.Remove(UnusedNodes.Where(n => n.id == dividedLine[0].nodes[1].id).FirstOrDefault()); // Middlepoint no longer free
                        HullConcaveEdges.AddRange(dividedLine);
                        HullConcaveEdges.RemoveAt(linePositionInHull); // Divided line no longer exists
                    }
                }

                HullConcaveEdges = HullConcaveEdges.OrderByDescending(a => HullLine.getLength(a.nodes[0], a.nodes[1])).ToList();
            } while (aLineWasDividedInTheIteration);

            return HullConcaveEdges;
        }
    }

    [SupressImportIntoVM]
    public class Node
    {
        public int id;
        public double x;
        public double y;
        public double cos; // Used for middlepoint calculations
        public Node(double x, double y, int id)
        {
            this.x = x;
            this.y = y;
            this.id = id;
        }
    }

    [SupressImportIntoVM]
    public class HullLine
    {
        public Node[] nodes = new Node[2];
        public HullLine(Node n1, Node n2)
        {
            nodes[0] = n1;
            nodes[1] = n2;
        }
        public static double getLength(Node node1, Node node2)
        {
            /* It actually calculates relative length */
            double length;
            length = Math.Pow(node1.y - node2.y, 2) + Math.Pow(node1.x - node2.x, 2);
            //length = Math.sqrt(Math.Pow(node1.y - node2.y, 2) + Math.Pow(node1.x - node2.x, 2));
            return length;
        }
    }

    [SupressImportIntoVM]
    public static class GrahamScan
    {
        const int TURN_LEFT = 1;
        const int TURN_RIGHT = -1;
        const int TURN_NONE = 0;
        public static int turn(Node p, Node q, Node r)
        {
            return ((q.x - p.x) * (r.y - p.y) - (r.x - p.x) * (q.y - p.y)).CompareTo(0);
        }

        public static void keepLeft(List<Node> hull, Node r)
        {
            while (hull.Count > 1 && turn(hull[hull.Count - 2], hull[hull.Count - 1], r) != TURN_LEFT)
            {
                hull.RemoveAt(hull.Count - 1);
            }
            if (hull.Count == 0 || hull[hull.Count - 1] != r)
            {
                hull.Add(r);
            }
        }

        public static double getAngle(Node p1, Node p2)
        {
            var xDiff = p2.x - p1.x;
            var yDiff = p2.y - p1.y;
            return Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
        }

        public static List<Node> MergeSort(Node p0, List<Node> arrPoint)
        {
            if (arrPoint.Count == 1)
            {
                return arrPoint;
            }
            var arrSortedInt = new List<Node>();
            var middle = (int)arrPoint.Count / 2;
            var leftArray = arrPoint.GetRange(0, middle);
            var rightArray = arrPoint.GetRange(middle, arrPoint.Count - middle);
            leftArray = MergeSort(p0, leftArray);
            rightArray = MergeSort(p0, rightArray);
            var leftptr = 0;
            var rightptr = 0;
            for (var i = 0; i < leftArray.Count + rightArray.Count; i++)
            {
                if (leftptr == leftArray.Count)
                {
                    arrSortedInt.Add(rightArray[rightptr]);
                    rightptr++;
                }
                else if (rightptr == rightArray.Count)
                {
                    arrSortedInt.Add(leftArray[leftptr]);
                    leftptr++;
                }
                else if (getAngle(p0, leftArray[leftptr]) < getAngle(p0, rightArray[rightptr]))
                {
                    arrSortedInt.Add(leftArray[leftptr]);
                    leftptr++;
                }
                else
                {
                    arrSortedInt.Add(rightArray[rightptr]);
                    rightptr++;
                }
            }
            return arrSortedInt;
        }

        public static List<Node> convexHull(List<Node> points)
        {
            Node p0 = null;
            foreach (var value in points)
            {
                if (p0 == null)
                    p0 = value;
                else
                {
                    if (p0.y > value.y)
                        p0 = value;
                }
            }
            var order = new List<Node>();
            foreach (var value in points)
            {
                if (p0 != value)
                    order.Add(value);
            }

            order = MergeSort(p0, order);
            var result = new List<Node>();
            result.Add(p0);
            result.Add(order[0]);
            result.Add(order[1]);
            order.RemoveAt(0);
            order.RemoveAt(0);
            foreach (var value in order)
            {
                keepLeft(result, value);
            }
            return result;
        }
    }

    [SupressImportIntoVM]
    public static class HullFunctions
    {
        public static List<HullLine> getDividedLine(HullLine line, List<Node> nearbyPoints, List<HullLine> concave_hull, double concavity)
        {
            // returns two lines if a valid middlePoint is found
            // returns empty list if the line can't be divided
            var dividedLine = new List<HullLine>();
            var okMiddlePoints = new List<Node>();
            foreach (var middlePoint in nearbyPoints)
            {
                var _cos = getCos(line.nodes[0], line.nodes[1], middlePoint);
                if (_cos < concavity)
                {
                    var newLineA = new HullLine(line.nodes[0], middlePoint);
                    var newLineB = new HullLine(middlePoint, line.nodes[1]);
                    if (!lineCollidesWithHull(newLineA, concave_hull) && !lineCollidesWithHull(newLineB, concave_hull))
                    {
                        middlePoint.cos = _cos;
                        okMiddlePoints.Add(middlePoint);
                    }
                }
            }
            if (okMiddlePoints.Count > 0)
            {
                // We want the middlepoint to be the one with the widest angle (smallest cosine)
                okMiddlePoints = okMiddlePoints.OrderBy(p => p.cos).ToList();
                dividedLine.Add(new HullLine(line.nodes[0], okMiddlePoints[0]));
                dividedLine.Add(new HullLine(okMiddlePoints[0], line.nodes[1]));
            }
            return dividedLine;
        }

        public static bool lineCollidesWithHull(HullLine line, List<HullLine> concave_hull)
        {
            foreach (var hullLine in concave_hull)
            {
                // We don't want to check a collision with this point that forms the hull AND the line
                if (line.nodes[0].id != hullLine.nodes[0].id && line.nodes[0].id != hullLine.nodes[1].id
                    && line.nodes[1].id != hullLine.nodes[0].id && line.nodes[1].id != hullLine.nodes[1].id)
                {
                    // Avoid line interesections with the rest of the hull
                    if (LineIntersectionFunctions.doIntersect(line.nodes[0], line.nodes[1], hullLine.nodes[0], hullLine.nodes[1]))
                        return true;
                }
            }
            return false;
        }

        private static double getCos(Node A, Node B, Node O)
        {
            /* Law of cosines */
            var aPow2 = Math.Pow(A.x - O.x, 2) + Math.Pow(A.y - O.y, 2);
            var bPow2 = Math.Pow(B.x - O.x, 2) + Math.Pow(B.y - O.y, 2);
            var cPow2 = Math.Pow(A.x - B.x, 2) + Math.Pow(A.y - B.y, 2);
            var cos = (aPow2 + bPow2 - cPow2) / (2 * Math.Sqrt(aPow2 * bPow2));
            return Math.Round(cos, 4);
        }

        public static List<Node> getNearbyPoints(HullLine line, List<Node> nodeList, int scaleFactor)
        {
            /* The bigger the scaleFactor the more points it will return
             * Inspired by this precious algorithm:
             * http://www.it.uu.se/edu/course/homepage/projektTDB/ht13/project10/Project-10-report.pdf
             * Be carefull: if it's too small it will return very little points (or non!), 
             * if it's too big it will add points that will not be used and will consume time
             * */
            var nearbyPoints = new List<Node>();
            double[] boundary;
            var tries = 0;
            double node_x_rel_pos;
            double node_y_rel_pos;

            while (tries < 2 && nearbyPoints.Count == 0)
            {
                boundary = getBoundary(line, scaleFactor);
                foreach (var node in nodeList)
                {
                    //Not part of the line
                    if (!(node.x == line.nodes[0].x && node.y == line.nodes[0].y ||
                        node.x == line.nodes[1].x && node.y == line.nodes[1].y))
                    {
                        node_x_rel_pos = Math.Floor(node.x / scaleFactor);
                        node_y_rel_pos = Math.Floor(node.y / scaleFactor);
                        //Inside the boundary
                        if (node_x_rel_pos >= boundary[0] && node_x_rel_pos <= boundary[2] &&
                            node_y_rel_pos >= boundary[1] && node_y_rel_pos <= boundary[3])
                        {
                            nearbyPoints.Add(node);
                        }
                    }
                }
                //if no points are found we increase the area
                scaleFactor = scaleFactor * 4 / 3;
                tries++;
            }
            return nearbyPoints;
        }

        private static double[] getBoundary(HullLine line, int scaleFactor)
        {
            /* Giving a scaleFactor it returns an area around the line 
             * where we will search for nearby points 
             * */
            var boundary = new double[4];
            var aNode = line.nodes[0];
            var bNode = line.nodes[1];
            var min_x_position = Math.Floor(Math.Min(aNode.x, bNode.x) / scaleFactor);
            var min_y_position = Math.Floor(Math.Min(aNode.y, bNode.y) / scaleFactor);
            var max_x_position = Math.Floor(Math.Max(aNode.x, bNode.x) / scaleFactor);
            var max_y_position = Math.Floor(Math.Max(aNode.y, bNode.y) / scaleFactor);

            boundary[0] = min_x_position;
            boundary[1] = min_y_position;
            boundary[2] = max_x_position;
            boundary[3] = max_y_position;

            return boundary;
        }
    }

    [SupressImportIntoVM]
    public static class LineIntersectionFunctions
    {
        // The main function that returns true if line segment 'p1q1' 
        // and 'p2q2' intersect. 
        public static Boolean doIntersect(Node p1, Node q1, Node p2, Node q2)
        {
            // Find the four orientations needed for general and 
            // special cases 
            var o1 = orientation(p1, q1, p2);
            var o2 = orientation(p1, q1, q2);
            var o3 = orientation(p2, q2, p1);
            var o4 = orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases 
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1 
            if (o1 == 0 && onSegment(p1, p2, q1)) return true;

            // p1, q1 and q2 are colinear and q2 lies on segment p1q1 
            if (o2 == 0 && onSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2 
            if (o3 == 0 && onSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2 
            if (o4 == 0 && onSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases 
        }

        // Given three colinear points p, q, r, the function checks if 
        // point q lies on line segment 'pr' 
        private static bool onSegment(Node p, Node q, Node r)
        {
            if (q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) &&
                q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y))
                return true;

            return false;
        }

        // To find orientation of ordered triplet (p, q, r). 
        // The function returns following values 
        // 0 --> p, q and r are colinear 
        // 1 --> Clockwise 
        // 2 --> Counterclockwise 
        private static int orientation(Node p, Node q, Node r)
        {
            // See https://www.geeksforgeeks.org/orientation-3-ordered-points/ 
            // for details of below formula. 
            var val = (q.y - p.y) * (r.x - q.x) -
                      (q.x - p.x) * (r.y - q.y);

            if (val == 0) return 0; // colinear 

            return (val > 0) ? 1 : 2; // clock or counterclock wise 
        }
    }
}
