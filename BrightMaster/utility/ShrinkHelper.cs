using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BrightMaster.utility
{
    public class Vector
    {
        public double X;
        public double Y;

        // Constructors.
        public Vector(double x, double y) { X = x; Y = y; }
        public Vector() : this(double.NaN, double.NaN) { }

        public Vector(PointF pt)
        {
            X = pt.X;
            Y = pt.Y;
        }
        public static Vector operator -(Vector v, Vector w)
        {
            return new Vector(v.X - w.X, v.Y - w.Y);
        }


        public void Normalize()
        {
            double distance = Math.Sqrt(X * X + Y * Y);
            X = X / distance;
            Y = Y / distance;
            //return new Vector(X / distance, Y / distance);
        }

        public static Vector operator +(Vector v, Vector w)
        {
            return new Vector(v.X + w.X, v.Y + w.Y);
        }

        public static double operator *(Vector v, Vector w)
        {
            return v.X * w.X + v.Y * w.Y;
        }

        public static Vector operator *(Vector v, double mult)
        {
            return new Vector(v.X * mult, v.Y * mult);
        }

        public static Vector operator *(double mult, Vector v)
        {
            return new Vector(v.X * mult, v.Y * mult);
        }

        public double Cross(Vector v)
        {
            return X * v.Y - Y * v.X;
        }

        public override bool Equals(object obj)
        {
            var v = (Vector)obj;
            return (X - v.X).IsZero() && (Y - v.Y).IsZero();
        }
    }
    public static class Extensions
    {
        private const double Epsilon = 1e-10;

        public static bool IsZero(this double d)
        {
            return Math.Abs(d) < Epsilon;
        }
    }

    struct LineSegment
    {
        public PointF ptStart;
        public PointF ptEnd;
        public LineSegment(PointF start,PointF end)
        {
            ptStart = start;
            ptEnd = end;
        }
    }
    class ShrinkHelper
    {
        public List<System.Drawing.PointF> ShrinkConvexHull(List<PointF> hullPts)
        {
            float xSum = hullPts.Sum(pt => pt.X);
            float ySum = hullPts.Sum(pt => pt.Y);
            PointF centerPt = new System.Drawing.PointF(xSum / hullPts.Count, ySum / hullPts.Count);
            List<PointF> newPts = new List<PointF>();
            float xStart = hullPts.Min(pt => pt.X);
            float xEnd = hullPts.Max(pt => pt.X);
            float pixels = GlobalVars.Instance.Layout.Margin * (xEnd - xStart) / 100;

            //calculate the shrinked linesegments
            List<LineSegment> lineSegments = new List<LineSegment>();
            for (int i = 0; i < hullPts.Count; i++)
            {
                int start = i;
                int end = (i+1) % hullPts.Count;
                PointF ptStart = hullPts[start];
                PointF ptEnd = hullPts[end];
                Vector vec = new Vector(ptEnd.X - ptStart.X, ptEnd.Y - ptStart.Y);
                vec.Normalize();
                
                PointF ptMid = new PointF((ptStart.X + ptEnd.X) / 2.0f, (ptStart.Y + ptEnd.Y) / 2.0f);
                PointF ptNewMid = Rotate2Mid(ptMid, centerPt, vec, pixels);
                PointF ptStartShrink = AddVector(ptNewMid,  GetVectorFromStartToEnd(ptMid, ptStart));
                PointF ptEndShrink = AddVector(ptNewMid, GetVectorFromStartToEnd(ptMid, ptEnd));
                LineSegment lineSegment = new LineSegment(ptStartShrink, ptEndShrink);
                lineSegments.Add(lineSegment);
            }

            List<PointF> ptsIntersect = GetLineIntersections(lineSegments);
            return ptsIntersect;
        }

        private List<PointF> GetLineIntersections(List<LineSegment> lineSegments)
        {
            List<PointF> pts = new List<PointF>();
            for(int i = 0; i< lineSegments.Count;i++)
            {
                int start = i;
                int end = (i+1) % lineSegments.Count;
                LineSegment lineStart = lineSegments[start];
                LineSegment lineEnd = lineSegments[end];
                //pts.Add(GetIntersectPt)
                Vector intersect;
                Vector p = new Vector(lineStart.ptStart);
                Vector p2 = new Vector(lineStart.ptEnd);
                Vector q = new Vector(lineEnd.ptStart);
                Vector q2 = new Vector(lineEnd.ptEnd);
                bool bIntersect = LineSegementsIntersect(p,p2,q,q2, out intersect);
                if (!bIntersect)
                    throw new Exception("Unexcepted not intersecting line segments found!");
                else
                    pts.Add(new PointF((float)intersect.X, (float)intersect.Y));
            }
            return pts;
        }

        public static bool LineSegementsIntersect(Vector p, Vector p2, Vector q, Vector q2,
    out Vector intersection, bool considerCollinearOverlapAsIntersect = false)
        {
            intersection = new Vector();

            var r = p2 - p;
            var s = q2 - q;
            var rxs = r.Cross(s);
            var qpxr = (q - p).Cross(r);

            // If r x s = 0 and (q - p) x r = 0, then the two lines are collinear.
            if (rxs.IsZero() && qpxr.IsZero())
            {
                // 1. If either  0 <= (q - p) * r <= r * r or 0 <= (p - q) * s <= * s
                // then the two lines are overlapping,
                if (considerCollinearOverlapAsIntersect)
                    if ((0 <= (q - p) * r && (q - p) * r <= r * r) || (0 <= (p - q) * s && (p - q) * s <= s * s))
                        return true;

                // 2. If neither 0 <= (q - p) * r = r * r nor 0 <= (p - q) * s <= s * s
                // then the two lines are collinear but disjoint.
                // No need to implement this expression, as it follows from the expression above.
                return false;
            }

            // 3. If r x s = 0 and (q - p) x r != 0, then the two lines are parallel and non-intersecting.
            if (rxs.IsZero() && !qpxr.IsZero())
                return false;

            // t = (q - p) x s / (r x s)
            var t = (q - p).Cross(s) / rxs;

            // u = (q - p) x r / (r x s)

            var u = (q - p).Cross(r) / rxs;

            // 4. If r x s != 0 and 0 <= t <= 1 and 0 <= u <= 1
            // the two line segments meet at the point p + t r = q + u s.
            if (!rxs.IsZero() && (0 <= t && t <= 1) && (0 <= u && u <= 1))
            {
                // We can calculate the intersection point using either t or u.
                intersection = p + t * r;

                // An intersection was found.
                return true;
            }

            // 5. Otherwise, the two line segments are not parallel but do not intersect.
            return false;
        }

        private Vector GetVectorFromStartToEnd(PointF ptStart, PointF ptEnd)
        {
            return new Vector(ptEnd.X - ptStart.X, ptEnd.Y - ptStart.Y);
        }

        private PointF AddVector(PointF ptStart, Vector vector)
        {
            return new PointF((float)(ptStart.X + vector.X), (float)(ptStart.Y + vector.Y));
        }

        Vector Rotate90CW(Vector vec)
        {
            return new Vector(vec.Y, -vec.X);
        }

        Vector Rotate90CCW(Vector vec)
        {
            return new Vector(-vec.Y, vec.X);
        }

        static double GetDistance(PointF pt1, PointF pt2)
        {
            float xx = pt1.X - pt2.X;
            float yy = pt1.Y - pt2.Y;
            return Math.Sqrt(xx * xx + yy * yy);

        }

        private PointF Rotate2Mid(PointF ptMid, PointF centerPt, Vector vec, float pixels)
        {
            var tmpVec1 = Rotate90CW(vec);
            var tmpVec2 = Rotate90CCW(vec);
            PointF newPt1 = new PointF((float)(ptMid.X + tmpVec1.X * pixels), (float)(ptMid.Y + tmpVec1.Y * pixels));
            PointF newPt2 = new PointF((float)(ptMid.X + tmpVec2.X * pixels), (float)(ptMid.Y + tmpVec2.Y * pixels));
            double distance1 = GetDistance(newPt1, centerPt);
            double distance2 = GetDistance(newPt2, centerPt);
            return distance1 < distance2 ? newPt1 : newPt2;
        }
    }
}
