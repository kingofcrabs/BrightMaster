using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
        private bool isN_N;
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

        public bool IsSquare
        {
            get
            {
                return isN_N;
            }
            set
            {
                isN_N = value;
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


        public List<CircleF> GetCircles(Rect boundingRect)
        {
            var layout = GlobalVars.Instance.Layout;
            List<CircleF> circles = new List<CircleF>();
            float xStart = (float)(boundingRect.X + layout.topLeft.X / layout.Width * boundingRect.Width);
            float yStart = (float)(boundingRect.Y + layout.topLeft.Y / layout.Height * boundingRect.Height);
            float xEnd = (float)(boundingRect.X + layout.bottomRight.X / layout.Width * boundingRect.Width);
            float yEnd = (float)(boundingRect.Y + layout.bottomRight.Y / layout.Height * boundingRect.Height);
            float radius = (float)(layout.Radius / layout.Width * boundingRect.Width);
            PointF ptStart = new PointF(xStart, yStart);
            PointF ptEnd = new PointF(xEnd, yEnd);
            
            for (int x = 0; x < layout.XCount; x++)
            {
                for (int y = 0; y < layout.YCount; y++)
                {
                    float xx = layout.XCount == 1 ? ptStart.X : (ptEnd.X - ptStart.X) * x / (layout.XCount - 1) + ptStart.X;
                    float yy = layout.yCount == 1 ? ptStart.Y : (ptEnd.Y - ptStart.Y) * y / (layout.YCount - 1) + ptStart.Y;
                    bool bNeedAdd =  isN_N || ((x + y) % 2 == 0);
                    if(bNeedAdd)
                        circles.Add(new CircleF(xx, yy, radius));
                }
            }
            return circles;

        }
    }


    
}
