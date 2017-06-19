using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster
{
    [Serializable]
    public class Layout
    {
        public Point topLeft;
        public Point bottomRight;
        public int xCount;
        public int yCount;
        public double radius;

        public Layout()
        {

        }

        public Layout(Point topLeft_, Point bottomRight_, int xCount_, int yCount_, double radius_)
        {
            topLeft = topLeft_;
            bottomRight = bottomRight_;
            xCount = xCount_;
            yCount = yCount_;
            radius = radius_;
        }

    }
}
