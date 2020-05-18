/* 
 * Convex hull algorithm - Library (C#)
 * 
 * Copyright (c) 2017 Project Nayuki
 * https://www.nayuki.io/page/convex-hull-algorithm
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program (see COPYING.txt and COPYING.LESSER.txt).
 * If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;

namespace archilab.Revit.Utils
{
    /// <summary>
    /// 
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public sealed class ConvexHull
    {
        // Returns a new list of points representing the convex hull of
        // the given set of points. The convex hull excludes collinear points.
        // This algorithm runs in O(n log n) time.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static IList<HullPoint> MakeHull(IList<HullPoint> points)
        {
            List<HullPoint> newPoints = new List<HullPoint>(points);
            newPoints.Sort();
            return MakeHullPresorted(newPoints);
        }


        // Returns the convex hull, assuming that each points[i] <= points[i + 1]. Runs in O(n) time.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static IList<HullPoint> MakeHullPresorted(IList<HullPoint> points)
        {
            if (points.Count <= 1)
                return new List<HullPoint>(points);

            // Andrew's monotone chain algorithm. Positive y coordinates correspond to "up"
            // as per the mathematical convention, instead of "down" as per the computer
            // graphics convention. This doesn't affect the correctness of the result.

            List<HullPoint> upperHull = new List<HullPoint>();
            foreach (HullPoint p in points)
            {
                while (upperHull.Count >= 2)
                {
                    HullPoint q = upperHull[upperHull.Count - 1];
                    HullPoint r = upperHull[upperHull.Count - 2];
                    if ((q.x - r.x) * (p.y - r.y) >= (q.y - r.y) * (p.x - r.x))
                        upperHull.RemoveAt(upperHull.Count - 1);
                    else
                        break;
                }
                upperHull.Add(p);
            }
            upperHull.RemoveAt(upperHull.Count - 1);

            IList<HullPoint> lowerHull = new List<HullPoint>();
            for (int i = points.Count - 1; i >= 0; i--)
            {
                HullPoint p = points[i];
                while (lowerHull.Count >= 2)
                {
                    HullPoint q = lowerHull[lowerHull.Count - 1];
                    HullPoint r = lowerHull[lowerHull.Count - 2];
                    if ((q.x - r.x) * (p.y - r.y) >= (q.y - r.y) * (p.x - r.x))
                        lowerHull.RemoveAt(lowerHull.Count - 1);
                    else
                        break;
                }
                lowerHull.Add(p);
            }
            lowerHull.RemoveAt(lowerHull.Count - 1);

            if (!(upperHull.Count == 1 && Enumerable.SequenceEqual(upperHull, lowerHull)))
                upperHull.AddRange(lowerHull);
            return upperHull;
        }
    }
}

/// <summary>
/// 
/// </summary>
[IsVisibleInDynamoLibrary(false)]
public struct HullPoint : IComparable<HullPoint>
{
    /// <summary>
    /// 
    /// </summary>
    public double x;
    /// <summary>
    /// 
    /// </summary>
    public double y;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public HullPoint(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(HullPoint other)
    {
        if (x < other.x)
            return -1;
        else if (x > other.x)
            return +1;
        else if (y < other.y)
            return -1;
        else if (y > other.y)
            return +1;
        else
            return 0;
    }
}
