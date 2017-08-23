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
                return topLeft.Y;
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

        //#region firefly

        //public float FireFlyTopLeftX
        //{
        //    get
        //    {
        //        return fireflyTopLeft.X;
        //    }
        //    set
        //    {
        //        fireflyTopLeft.X = value;
        //    }
        //}
        //public float FireFlyTopLeftY
        //{
        //    get
        //    {
        //        return fireflyTopLeft.Y;
        //    }

        //    set
        //    {
        //        fireflyTopLeft.Y = value;
        //    }
        //}

        //public float FireFlyBottomRightX
        //{
        //    get
        //    {
        //        return fireflybottomRight.X;
        //    }
        //    set
        //    {
        //        fireflybottomRight.X = value;
        //    }
        //}

        //public float FireFlyBottomRightY
        //{
        //    get
        //    {
        //        return fireflybottomRight.Y;
        //    }
        //    set
        //    {
        //        fireflybottomRight.Y = value;
        //    }
        //}
        //#endregion

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

        PointF GetPositionInImage(List<System.Drawing.Point> realPts, PointF ptInLayout)
        {
            PointF leftTop = realPts[0];
            PointF rightTop = realPts[1];
            PointF bottomLeft = realPts[3];
            var layout = GlobalVars.Instance.Layout;
            
            Vector vec1 = new Vector(rightTop.X - leftTop.X, rightTop.Y - leftTop.Y);
            Vector vec2 = new Vector(bottomLeft.X - leftTop.X, bottomLeft.Y - leftTop.Y);
            PointF pt = leftTop;
            pt.X += (float)(vec1.X * ptInLayout.X / layout.Width);
            pt.Y += (float)(vec1.Y * ptInLayout.X / layout.Width);
            pt.X += (float)(vec2.X * ptInLayout.Y / layout.Height);
            pt.Y += (float)(vec2.Y * ptInLayout.Y / layout.Height);
            return pt;
            //topBoundInterset.X = leftTop.X + (rightTop.X - leftTop.X) * ptInLayout.X / layout.Width;
            //topBoundInterset.Y = leftTop.Y + (rightTop.Y - leftTop.Y) * ptInLayout.Y / layout.Height;

            //PointF leftBoundInterset = new PointF(100,100);
            //leftBoundInterset.X = leftTop.X + (bottomLeft.X - leftTop.X) * ptInLayout.X / layout.Width;
            //leftBoundInterset.Y = leftTop.Y + (bottomLeft.Y - leftTop.Y) * ptInLayout.Y / layout.Height;

            //PointF pt = new PointF(0,0);
            //pt.X = leftBoundInterset.X + topBoundInterset.X - leftTop.X;
            //pt.Y = leftBoundInterset.Y + topBoundInterset.Y - leftTop.Y;
            //return pt;

        }

        double GetDistance(PointF pt1, PointF pt2)
        {
            float xx = pt2.X - pt1.X;
            float yy = pt2.Y - pt1.Y;
            return Math.Sqrt(xx * xx + yy * yy);
        }
        internal List<CircleF> GetCircles(List<System.Drawing.Point> pts)
        {
            var layout = GlobalVars.Instance.Layout;
            List<CircleF> circles = new List<CircleF>();

            PointF ptStart = layout.topLeft;
            PointF ptEnd = layout.bottomRight;
            double realWidth = GetDistance(pts[0], pts[1]);
            float radius = (float)(realWidth / layout.width * layout.radius);
            for (int x = 0; x < layout.XCount; x++)
            {
                for (int y = 0; y < layout.YCount; y++)
                {
                    float xx = layout.XCount == 1 ? ptStart.X : (ptEnd.X - ptStart.X) * x / (layout.XCount - 1) + ptStart.X;
                    float yy = layout.yCount == 1 ? ptStart.Y : (ptEnd.Y - ptStart.Y) * y / (layout.YCount - 1) + ptStart.Y;
                    PointF realPt = GetPositionInImage(pts, new PointF(xx, yy));
                    bool bNeedAdd = isN_N || ((x + y) % 2 == 0);
                    if (bNeedAdd)
                        circles.Add(new CircleF(realPt.X,realPt.Y, radius));
                }
            }
            return circles;
        }
    }


    
}
