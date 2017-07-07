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
        public PointF topLeft;
        public PointF bottomRight;
        public float width { get; set; }
        public float height { get; set; }
        public int xCount { get; set; }
        public int yCount { get; set; }
        public float radius { get; set; }
        public float TopLeftX { get{
            return topLeft.X;
        }
            set{
                topLeft.X = value;
            }
        }
        public float TopLeftY
        {
            get
            {
                return topLeft.X;
            }

            set
            {
                topLeft.Y = value;
            }
        }

        public float BottomRightX
        {
            get
            {
                return bottomRight.X;
            }
            set
            {
                bottomRight.X = value;
            }
        }

        public float BottomRightY
        {
            get
            {
                return bottomRight.Y;
            }
            set
            {
                bottomRight.Y = value;
            }
        }

        public Layout()
        {

        }

        public Layout(float width_, float height_,PointF topLeft_, PointF bottomRight_, int xCount_, int yCount_, float radius_)
        {
            width = width_;
            height = height_;
            topLeft = topLeft_;
            bottomRight = bottomRight_;
            xCount = xCount_;
            yCount = yCount_;
            radius = radius_;
        }

    }
}
