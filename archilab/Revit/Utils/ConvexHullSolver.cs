//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using archilab.Revit.Elements;

//namespace archilab.Revit.Utils
//{
//    public class ConvexHullSolver
//    {
//        private ConvexHullSolver()
//        {
//        }

//        public static bool Compute(Node2List nodes, List<int> hull)
//        {
//            if (nodes == null)
//                throw new ArgumentNullException(nameof(nodes));
//            if (hull == null)
//                throw new ArgumentNullException(nameof(hull));
//            List<bool> boolList = new List<bool>();
//            hull.Clear();
//            boolList.Clear();
//            hull.Capacity = nodes.Count;
//            boolList.Capacity = nodes.Count;
//            bool flag;
//            if (nodes.Count == 0)
//                flag = false;
//            else if (nodes.Count == 1)
//                flag = false;
//            else if (nodes.Count == 2)
//            {
//                hull.Add(0);
//                hull.Add(1);
//                flag = true;
//            }
//            else
//            {
//                int num1 = nodes.Count - 1;
//                for (int index = 0; index <= num1; ++index)
//                    boolList.Add(false);
//                int index1 = -1;
//                int num2 = -1;
//                int num3 = nodes.Count - 1;
//                for (int index2 = 0; index2 <= num3; ++index2)
//                {
//                    if (nodes[index2] != null)
//                    {
//                        index1 = index2;
//                        num2 = index2;
//                        break;
//                    }
//                }
//                if (index1 < 0)
//                {
//                    flag = false;
//                }
//                else
//                {
//                    int num4 = nodes.Count - 1;
//                    for (int index2 = 1; index2 <= num4; ++index2)
//                    {
//                        if (nodes[index2] != null)
//                        {
//                            if (nodes[index2].x < nodes[index1].x)
//                                index1 = index2;
//                            else if (nodes[index2].x == nodes[index1].x && nodes[index2].y < nodes[index1].y)
//                                index1 = index2;
//                        }
//                    }
//                    int index3 = index1;
//                    do
//                    {
//                        int index2 = -1;
//                        int num5 = nodes.Count - 1;
//                        for (int index4 = 0; index4 <= num5; ++index4)
//                        {
//                            if (nodes[index4] != null && !boolList[index4] && index4 != index3)
//                            {
//                                if (index2 == -1)
//                                {
//                                    index2 = index4;
//                                }
//                                else
//                                {
//                                    double num6 = ConvexHullSolver.CrossProduct(nodes[index4], nodes[index3], nodes[index2]);
//                                    if (num6 == 0.0)
//                                    {
//                                        if (ConvexHullSolver.DotProduct(nodes[index3], nodes[index4], nodes[index4]) > ConvexHullSolver.DotProduct(nodes[index3], nodes[index2], nodes[index2]))
//                                            index2 = index4;
//                                    }
//                                    else if (num6 < 0.0)
//                                        index2 = index4;
//                                }
//                            }
//                        }
//                        index3 = index2;
//                        boolList[index3] = true;
//                        hull.Add(index3);
//                    }
//                    while (index3 != index1);
//                    flag = true;
//                }
//            }
//            return flag;
//        }

//        private static double CrossProduct(Node2 A, Node2 B, Node2 C)
//        {
//            return (B.x - A.x) * (C.y - A.y) - (C.x - A.x) * (B.y - A.y);
//        }

//        private static double DotProduct(Node2 A, Node2 B, Node2 C)
//        {
//            return (B.x - A.x) * (C.x - A.x) + (B.y - A.y) * (C.y - A.y);
//        }
//    }
//}
