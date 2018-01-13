using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClipperLib;

namespace BrightMaster.utility
{
    using Polygon = List<IntPoint>;
    using Polygons = List<List<IntPoint>>;
    class ShrinkHelper
    {
        public List<PointF> ShrinkConvexHull(List<PointF> hullPts)
        {
            var len = IntersectionHelper.CalculateMarginLen(hullPts);
            return Shrink(hullPts, len);
        }

        private List<PointF> Shrink(List<PointF> hullPts, float len)
        {
            ClipperOffset co = new ClipperOffset();
            Polygons solution = new Polygons();
            Polygon polygon = new Polygon();
            hullPts.ForEach(pt => polygon.Add(new IntPoint(pt.X, pt.Y)));
            solution.Add(polygon);
            co.AddPaths(solution, JoinType.jtRound, EndType.etClosedPolygon);
            co.Execute(ref solution, (double)-len);
            var newPts = solution.First();
            List<System.Drawing.PointF> pts = new List<PointF>();
            foreach (var pt in newPts)
            {
                pts.Add(new PointF(pt.X, pt.Y));
            }
            return pts;
        }



        internal List<PointF> ShrinkRect(List<PointF> pts)
        {
            float xStart = pts.Min(pt => pt.X);
            float xEnd = pts.Max(pt => pt.X);
            float width = xEnd - xStart;
            float len = GlobalVars.Instance.Layout.Margin * width / 100;
            List<PointF> offsetPts = new List<PointF>();
            offsetPts.Add(new PointF(pts[0].X + len, pts[0].Y + len));
            offsetPts.Add(new PointF(pts[1].X - len, pts[1].Y + len));
            offsetPts.Add(new PointF(pts[2].X - len, pts[2].Y - len));
            offsetPts.Add(new PointF(pts[3].X + len, pts[3].Y - len));
            return offsetPts;

        }
    }
}
