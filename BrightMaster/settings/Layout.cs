﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BrightMaster.Settings
{
    [Serializable]
    public class Layout : BindableBase
    {
        public PointF topLeftRatio;
        public PointF bottomRightRatio;

        private PointF roiTopLeftRatio;
        private PointF roiBottomRightRatio;
        private int xCount;
        private int yCount;
        private float radiusRatio;
        private bool isN_N;
      
        public Layout()
        {
            topLeftRatio = new PointF(10, 10);
            bottomRightRatio = new PointF(90, 90);
            roiTopLeftRatio = new PointF(0, 0);
            roiBottomRightRatio = new PointF(100, 100);
            xCount = 5;
            yCount = 5;
            radiusRatio = 2;
            isN_N = true;

        }

        internal void CheckSetting()
        {
            if (topLeftRatio.X < 0 || topLeftRatio.X > 100 || topLeftRatio.Y < 0 || topLeftRatio.Y > 100)
                ThrowWithInfo("TopLeftRatio必须在0~100之间!");
            if (bottomRightRatio.X < 0 || bottomRightRatio.X > 100 || bottomRightRatio.Y < 0 || bottomRightRatio.Y > 100)
                ThrowWithInfo("BottomRightRatio必须在0~100之间!");
            if (roiTopLeftRatio.X < 0 || roiTopLeftRatio.X > 100 || roiTopLeftRatio.Y < 0 || roiTopLeftRatio.Y > 100)
                ThrowWithInfo("RoiTopLeftRatio必须在0~100之间!");
            if (roiBottomRightRatio.X < 0 || roiBottomRightRatio.X > 100 || roiBottomRightRatio.Y < 0 || roiBottomRightRatio.Y > 100)
                ThrowWithInfo("RoiBottomRightRatio必须在0~100之间!");

            if (xCount <= 0)
                ThrowWithInfo("XCount必须大于0！");
            if (yCount <= 0)
                ThrowWithInfo("YCount必须大于0！");
            if(radiusRatio <= 0)
                ThrowWithInfo("RadiusRatio必须大于0！");
            
        }

        private void ThrowWithInfo(string s)
        {
            throw new Exception(s);
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


        public PointF ROITopLeftRatio
        {
            get
            {
                return roiTopLeftRatio;
            }
            set
            {
                SetProperty(ref roiTopLeftRatio, value);
            }
        }

        public PointF RoiBottomRightRatio
        {
            get
            {
                return roiBottomRightRatio;
            }
            set{
                SetProperty(ref roiBottomRightRatio, value);
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
        public float RadiusRatio
        { 
            get
            {
                return radiusRatio;
            }
            set
            {
                SetProperty(ref radiusRatio, value);
            }
        }
        public float TopLeftXRatio 
        { 
            get{
                return topLeftRatio.X;
            }
            set{
                topLeftRatio.X = value;
            }
        }
        public float TopLeftYRatio
        {
            get
            {
                return topLeftRatio.Y;
            }

            set
            {
                topLeftRatio.Y = value;
            }
        }

        public float ROITopLeftXRatio
        {
            get
            {
                return roiTopLeftRatio.X;
            }
            set
            {
                roiTopLeftRatio.X = value;
            }
        }
        public float ROITopLeftYRatio
        {
            get
            {
                return roiTopLeftRatio.Y;
            }

            set
            {
                roiTopLeftRatio.Y = value;
            }
        }

        public float ROIBottomRightXRatio
        {
            get
            {
                return roiBottomRightRatio.X;
            }
            set
            {
                roiBottomRightRatio.X = value;
            }
        }

        public float ROIBottomRightYRatio
        {
            get
            {
                return roiBottomRightRatio.Y;
            }
            set
            {
                roiBottomRightRatio.Y = value;
            }
        }

        public float BottomRightXRatio
        {
            get
            {
                return bottomRightRatio.X;
            }
            set
            {
                bottomRightRatio.X = value;
            }
        }

        public float BottomRightYRatio
        {
            get
            {
                return bottomRightRatio.Y;
            }
            set
            {
                bottomRightRatio.Y = value;
            }
        }

        

        public Layout(PointF roiTopLeft_, PointF roiBottomRight_, PointF topLeft_, PointF bottomRight_, int xCount_, int yCount_, float radius_)
        {
            this.roiTopLeftRatio = roiTopLeft_;
            this.roiBottomRightRatio = roiBottomRight_;
            topLeftRatio = roiTopLeft_;
            bottomRightRatio = roiBottomRight_;
            XCount = xCount_;
            YCount = yCount_;
            RadiusRatio = radius_;
        }


        public List<CircleF> GetCircles(Rect boundingRect)
        {
            var layout = GlobalVars.Instance.Layout;
            List<CircleF> circles = new List<CircleF>();
            float xStart = (float)(boundingRect.X + layout.TopLeftXRatio * boundingRect.Width / 100);
            float yStart = (float)(boundingRect.Y + layout.TopLeftYRatio * boundingRect.Height / 100);
            float xEnd = (float)(boundingRect.X + layout.BottomRightXRatio * boundingRect.Width / 100);
            float yEnd = (float)(boundingRect.Y + layout.BottomRightYRatio * boundingRect.Height / 100);
            float radius = (float)(layout.RadiusRatio /100 * boundingRect.Width);
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

        PointF GetPositionInImage(List<System.Drawing.Point> realPts, PointF ratioInLayout)
        {
            PointF leftTop = realPts[0];
            PointF rightTop = realPts[1];
            PointF bottomLeft = realPts[3];
            var layout = GlobalVars.Instance.Layout;
            
            Vector vec1 = new Vector(rightTop.X - leftTop.X, rightTop.Y - leftTop.Y);
            Vector vec2 = new Vector(bottomLeft.X - leftTop.X, bottomLeft.Y - leftTop.Y);
            PointF pt = leftTop;
            pt.X += (float)(vec1.X * ratioInLayout.X );
            pt.Y += (float)(vec1.Y * ratioInLayout.X );
            pt.X += (float)(vec2.X * ratioInLayout.Y);
            pt.Y += (float)(vec2.Y * ratioInLayout.Y);
            return pt;

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

            PointF ptStart = layout.topLeftRatio;
            PointF ptEnd = layout.bottomRightRatio;
            double realWidth = GetDistance(pts[0], pts[1]);
            float radius = (float)(realWidth * layout.radiusRatio/100);
            for (int x = 0; x < layout.XCount; x++)
            {
                for (int y = 0; y < layout.YCount; y++)
                {
                    float xx = layout.XCount == 1 ? ptStart.X : (ptEnd.X - ptStart.X) * x / (layout.XCount - 1) + ptStart.X;
                    float yy = layout.yCount == 1 ? ptStart.Y : (ptEnd.Y - ptStart.Y) * y / (layout.YCount - 1) + ptStart.Y;
                    PointF realPt = GetPositionInImage(pts, new PointF(xx/100, yy/100));
                    bool bNeedAdd = isN_N || ((x + y) % 2 == 0);
                    if (bNeedAdd)
                        circles.Add(new CircleF(realPt.X,realPt.Y, radius));
                }
            }
            return circles;
        }

        
    }


    
}
