using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace BrightMaster
{
    public class CircleF
    {
        public float radius;
        public float x;
        public float y;

        public CircleF(float xx, float yy, float rr)
        {
            x = xx;
            y = yy;
            radius = rr;
        }

        public PointF Position
        { 
            get
            {
                return new PointF(x, y);
            }
        }

        public PointF Size
        {
            get
            {
                return new PointF(radius, radius);
            }
        }
    }
}
