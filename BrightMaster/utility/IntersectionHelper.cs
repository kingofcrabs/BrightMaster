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

        public double GetLen()
        {
            return Math.Sqrt(X * X + Y * Y);
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
    class IntersectionHelper
    {
       static  public float CalculateMarginLen(List<PointF> hullPts)
        {
            float xSum = hullPts.Sum(pt => pt.X);
            float ySum = hullPts.Sum(pt => pt.Y);
            PointF centerPt = new System.Drawing.PointF(xSum / hullPts.Count, ySum / hullPts.Count);
            List<PointF> newPts = new List<PointF>();
            float xStart = hullPts.Min(pt => pt.X);
            float xEnd = hullPts.Max(pt => pt.X);
            float yStart = hullPts.Min(pt=>pt.Y);
            float yEnd = hullPts.Max(pt=>pt.Y);
            double yMid = (yStart + yEnd) / 2.0;
            //calculate the shrinked linesegments
            List<LineSegment> lineSegments = new List<LineSegment>();
            Vector q = new Vector(xStart-10, yMid);
            Vector q2 = new Vector(xEnd+10, yMid);
            List<Vector> intersects = new List<Vector>();
            for (int i = 0; i < hullPts.Count; i++)
            {
                int start = i;
                int end = (i+1) % hullPts.Count;
                Vector p = new Vector(hullPts[start]);
                Vector p2 = new Vector(hullPts[end]);
                Vector intersect;
                bool bIntersect = LineSegementsIntersect(p, p2, q, q2, out intersect);
                if (bIntersect)
                    intersects.Add(intersect);
            }
            
            if(intersects.Count != 2)
            {
                throw new Exception("无法计算外框宽度！");
            }
            double len = (intersects[0] - intersects[1]).GetLen();
            float pixels = (float)(GlobalVars.Instance.Layout.Margin * len / 100);
            return pixels;
            
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

      
    }
}
