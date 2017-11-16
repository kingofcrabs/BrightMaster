using BrightMaster.Settings;
using EngineDll;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace BrightMaster
{
    class MyCanvas : Canvas
    {
   
        double width;
        double height;
        double usableWidth;
        double usableHeight; 
        bool isFakeColor = false;
        
        List<Point> pts = null;
        Point ptStart = new Point(-1,-1);
        Point ptEnd = new Point(-1,-1);
        bool validMouseMove = false;
        bool userSelectROI = false;
    

        public bool IsValidMove
        {
            get
            {
                return validMouseMove;
            }
            set
            {
                validMouseMove = value;
            }
        }
        public bool UserSelectROI
        {
            get
            {
                return userSelectROI;
            }
            set
            {
                pts = new List<Point>();
                userSelectROI = value;
                InvalidateVisual();
            }
        }

        //public MyCanvas()
        //{
        //    //this.SizeChanged += MyCanvas_SizeChanged;
        //}

        void MyCanvas_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (IsInDesignMode())
                return;
            CalcuUsable(out usableWidth, out usableHeight);
        }



        protected void CalcuUsable(out double usableWidth, out double usableHeight)
        {
            usableWidth = 0;
            usableHeight = 0;

            double screenRatio = this.ActualWidth / this.ActualHeight;
            double realRatio = width/height;
            if (realRatio > screenRatio)//x方向占满
            {
                usableHeight = ActualHeight / (realRatio / screenRatio);
                usableWidth = ActualWidth;
            }
            else //y方向占满
            {
                usableWidth = ActualWidth / (screenRatio / realRatio);
                usableHeight = ActualHeight;
            }
        }

        Point PointF2Point(PointF ptF,double xRatio, double yRatio)
        {
            return new Point((int)(ptF.X*xRatio), (int)(ptF.Y*yRatio));
        }

        public void SetBoundRect(SizeF boundRectSize)
        {
            Layout layout = GlobalVars.Instance.Layout;
            pts = new List<Point>();
            pts.Add(PointF2Point(boundRectSize.ToPointF(), layout.ROITopLeftXRatio / 100, layout.ROITopLeftYRatio / 100));
            pts.Add(PointF2Point(boundRectSize.ToPointF(), layout.ROIBottomRightXRatio / 100, layout.ROITopLeftYRatio / 100));
            pts.Add(PointF2Point(boundRectSize.ToPointF(), layout.ROIBottomRightXRatio / 100, layout.ROIBottomRightYRatio / 100));
            pts.Add(PointF2Point(boundRectSize.ToPointF(), layout.ROITopLeftXRatio / 100, layout.ROIBottomRightYRatio / 100));
            width = boundRectSize.Width;
            height = boundRectSize.Height;
            CalcuUsable(out usableWidth, out usableHeight);
            InvalidateVisual();
        }


        public void SetBkGroundImage(System.Windows.Media.Imaging.BitmapImage bmpImage, List<Point> newPts = null, bool isFakeColor = false)
        {
            this.isFakeColor = isFakeColor;
            if (newPts != null && newPts.Count > 0)
            {
                pts = SortPoints(newPts);
            }
         
            width = bmpImage.Width;
            height = bmpImage.Height;
            CalcuUsable(out usableWidth, out usableHeight);
            System.Windows.Media.ImageBrush imageBrush = new System.Windows.Media.ImageBrush();
            double xRatio = usableWidth/width ;
            double yRatio = usableHeight/height ;
            
            imageBrush.ImageSource = bmpImage;
            imageBrush.Stretch = Stretch.Fill;
            imageBrush.AlignmentX = AlignmentX.Left;
            imageBrush.AlignmentY = AlignmentY.Top;
            imageBrush.ViewportUnits = BrushMappingMode.Absolute;
            imageBrush.Viewport = new System.Windows.Rect(0, 0, usableWidth, usableHeight);
            this.Background = imageBrush;
        }
        
       

        internal List<Point> SortPoints(List<Point> orgPts)
        {
            List<Point> pts = new List<Point>();
            int avgX = orgPts.Sum(pt=>pt.X)/4;
            int avgY = orgPts.Sum(pt=>pt.Y)/4;
            Point ptMassCenter = new Point(avgX, avgY);
            Point topLeft = orgPts.Where(pt => pt.X < avgX && pt.Y < avgY).First();
            Point topRight = orgPts.Where(pt => pt.X > avgX && pt.Y < avgY).First();
            Point bottomRight = orgPts.Where(pt => pt.X > avgX && pt.Y > avgY).First();
            Point bottomLeft = orgPts.Where(pt => pt.X < avgX && pt.Y > avgY).First();
            pts.Add(topLeft);
            pts.Add(topRight);
            pts.Add(bottomRight);
            pts.Add(bottomLeft);
            return pts;
        }

        private System.Windows.Rect GetBoundingRectInUICoordinate(List<Point> pts)
        {
            double left, top, right, bottom;
            int leftUICoord, topUICoord, rightUICoord, bottomUICoord;
            if(pts == null || pts.Count == 0)
            {
                leftUICoord = 0;
                topUICoord = 0;
                bottomUICoord = (int)this.ActualHeight;
                rightUICoord = (int)this.ActualWidth;
            }
            else 
            {
                if( userSelectROI)
                {
                    leftUICoord = pts.Min(pt => pt.X);
                    topUICoord = pts.Min(pt => pt.Y);
                    rightUICoord = pts.Max(pt => pt.X);
                    bottomUICoord = pts.Max(pt => pt.Y);
                }
                else
                {
                    left = pts.Min(pt => pt.X);
                    top = pts.Min(pt => pt.Y);
                    right = pts.Max(pt => pt.X);
                    bottom = pts.Max(pt => pt.Y);
                    leftUICoord = (int)(usableWidth * left / width);
                    topUICoord = (int)(usableHeight * top / height);
                    rightUICoord = (int)(usableWidth * right / width);
                    bottomUICoord = (int)(usableHeight * bottom / height);
                }
                
            }
            return new System.Windows.Rect(leftUICoord, topUICoord, rightUICoord - leftUICoord, bottomUICoord - topUICoord);
        }

        internal static bool IsInDesignMode()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location.Contains("VisualStudio");
        }
            

        //int Convert2UICoordX(float x)
        //{
        //    return (int)(usableWidth * x / width);
        //}
        //int Convert2UICoordY(float y)
        //{
        //    return (int)(usableHeight * y / height);
        //}


        int Convert2XUIFromReal(float x)
        {
            return (int)(usableWidth * x / width);
        }
        int Convert2UIYFromReal(float y)
        {
            return (int)(usableHeight * y / height);
        }

        int Convert2RealXFromUI(float x)
        {
            return (int)(width * x / usableWidth);
        }
        int Convert2RealYFromUI(float y)
        {
            return (int)(height * y / usableHeight);
        }


        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            
            if (IsInDesignMode())
                return;

            base.OnRender(drawingContext);

            Layout layout = GlobalVars.Instance.Layout;
            if (layout == null)
                return;
            
            var boundingRectUICoord = GetBoundingRectInUICoordinate(pts);
            System.Windows.Media.Brush redBrush = System.Windows.Media.Brushes.Red;
            System.Windows.Media.Pen pen = new System.Windows.Media.Pen(redBrush,1);
            

            bool validPt = pts != null && pts.Count == 4;
            if (validPt)
            {
                for (int i = 0; i < 4; i++)
                {
                    int endIndex = (i + 1) % 4;
                    System.Windows.Point startPt = new System.Windows.Point(pts[i].X,pts[i].Y);
                    System.Windows.Point endPt = new System.Windows.Point(pts[endIndex].X, pts[endIndex].Y);
                    if(!userSelectROI)
                    {
                        startPt = new System.Windows.Point(Convert2XUIFromReal(pts[i].X), Convert2UIYFromReal(pts[i].Y));
                        endPt = new System.Windows.Point(Convert2XUIFromReal(pts[endIndex].X), Convert2UIYFromReal(pts[endIndex].Y));
                    }
                    
                    drawingContext.DrawLine(pen, startPt, endPt);
                }
            }
            else
            {
                drawingContext.DrawRectangle(null, new System.Windows.Media.Pen(redBrush, 1), boundingRectUICoord);
            }

            if(layout != null)
            {
                var circles = validPt ? GlobalVars.Instance.Layout.GetCircles(pts) :
                    GlobalVars.Instance.Layout.GetCircles(boundingRectUICoord);
                int index = 0;
                if(userSelectROI && ptEnd.X == -1)
                    return;
                foreach(var circle in circles)
                {
                    System.Windows.Point uiPt = userSelectROI ? new System.Windows.Point(circle.Position.X,circle.Position.Y):
                        new System.Windows.Point(Convert2XUIFromReal(circle.Position.X), Convert2UIYFromReal(circle.Position.Y));
                    DrawCircle(uiPt, circle.Size,  drawingContext);
                    DrawText((index + 1).ToString(), uiPt, drawingContext, 12);
                    index++;
                }
            }
        }


        private void DrawText(string str, System.Windows.Point point, DrawingContext drawingContext, int fontSize = 16)
        {
            if (str == null)
                return;

            var txt = new FormattedText(
               str,
               System.Globalization.CultureInfo.CurrentCulture,
               0,
               new Typeface("Courier new"),
               fontSize,
               System.Windows.Media.Brushes.Black);

            drawingContext.DrawText(txt, point);
        }

        private void DrawCircle(System.Windows.Point pt, PointF sz, DrawingContext drawingContext)
        {
            drawingContext.DrawEllipse(null, new System.Windows.Media.Pen(System.Windows.Media.Brushes.Red,1),
                pt, sz.X, sz.Y);
        }


        internal void OnLeftButtonDown(System.Windows.Point mousePos)
        {
            validMouseMove = true;
            ptStart = new Point((int)mousePos.X, (int)mousePos.Y);
            InvalidateVisual();
        }

        internal void OnLeftButtonMove(System.Windows.Point mousePos)
        {
            if (!validMouseMove)
                return;
            ptEnd = new Point((int)mousePos.X, (int)mousePos.Y);
            pts = GeneratePts(ptStart, ptEnd);
            InvalidateVisual();
        }

        internal void OnLeftButtonUp()
        {
            pts = GeneratePts(ptStart, ptEnd);
            InvalidateVisual();
            validMouseMove = false;
        }

        private List<Point> GeneratePts(Point ptStart, Point ptEnd)
        {
            List<Point> tmpPts = new List<Point>();
            tmpPts.Add(ptStart);
            tmpPts.Add(new Point(ptEnd.X,ptStart.Y));
            tmpPts.Add(ptEnd);
            tmpPts.Add(new Point(ptStart.X, ptEnd.Y));
            return tmpPts;
        }


        public List<Point> GeneratePtsImageCoord()
        {
            List<Point> tmpPts = new List<Point>();
            tmpPts.Add(ptStart);
            tmpPts.Add(new Point(ptEnd.X, ptStart.Y));
            tmpPts.Add(ptEnd);
            tmpPts.Add(new Point(ptStart.X, ptEnd.Y));

            List<Point> tmpRealPts = new List<Point>();
            tmpPts.ForEach(pt => tmpRealPts.Add(new Point(Convert2RealXFromUI(pt.X), Convert2RealYFromUI(pt.Y))));
            return tmpRealPts;
        }

       
    }
}
