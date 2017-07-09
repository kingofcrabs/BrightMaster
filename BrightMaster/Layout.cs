using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster
{
    [Serializable]
    public class Layout : BindableBase
    {
        public PointF topLeft;
        public PointF bottomRight;

        private float width;
        private float height;
        private int xCount;
        private int yCount;
        private float radius;

        public float Width 
        { 
            get
            {
                return width;
            }
            set
            {
                SetProperty(ref width, value);
            }
            
        }
        public float Height
        {
            get
            {
                return height;
            }
            set{
                SetProperty(ref height, value);
            }
        }
        public int XCount
        {
            get
            {
                return xCount;
            }
            set
            {
                SetProperty(ref xCount, value);
            }
        }
        public int YCount
        {
            get
            {
                return yCount;
            }
            set
            {
                SetProperty(ref yCount, value);
            }
        }
        public float Radius { get
            {
                return radius;
            }
            set
            {
                SetProperty(ref radius, value);
            }
        }
        public float TopLeftX 
        { 
            get{
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
            Width = width_;
            Height = height_;
            topLeft = topLeft_;
            bottomRight = bottomRight_;
            XCount = xCount_;
            YCount = yCount_;
            Radius = radius_;
        }

    }
}
