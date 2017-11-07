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
        Layout layout;
        double width;
        double height;
        double usableWidth;
        double usableHeight; 
        bool isFakeColor = false;
        
        List<Point> pts = null;
        public Layout Layout 
        { 
            get
            {
                return layout;
            }
            set
            {
                layout = value;
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

        public void SetBkGroundImage(System.Windows.Media.Imaging.BitmapImage bmpImage, List<MPoint> mpts = null, bool isFakeColor = false)
        {
            this.isFakeColor = isFakeColor;
            if(mpts != null && mpts.Count > 0)
            {
                pts = SortPoints(mpts);
            }
            else if(!isFakeColor)
            {
                pts = new List<Point>();
                double tmpXRatio = bmpImage.Width / layout.Width;
                double tmpYRatio = bmpImage.Height / layout.Height;
                pts.Add(PointF2Point(layout.topLeft, tmpXRatio,tmpYRatio));
                pts.Add(new Point((int)(tmpXRatio * layout.bottomRight.X), (int)(layout.topLeft.Y*tmpYRatio)));
                pts.Add(PointF2Point(layout.bottomRight,tmpXRatio,tmpYRatio));
                pts.Add(new Point((int)(layout.topLeft.X *tmpXRatio), (int)(layout.bottomRight.Y*tmpYRatio)));
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
        
        Point ConvertPt(MPoint mpt)
        {
            return new Point(mpt.x, mpt.y);
        }

        private List<Point> SortPoints(List<MPoint> mpts)
        {
            List<Point> pts = new List<Point>();
            int avgX = mpts.Sum(pt=>pt.x)/4;
            int avgY = mpts.Sum(pt=>pt.y)/4;
            Point ptMassCenter = new Point(avgX, avgY);
            MPoint topLeft = mpts.Where(pt => pt.x < avgX && pt.y < avgY).First();
            MPoint topRight = mpts.Where(pt => pt.x > avgX && pt.y < avgY).First();
            MPoint bottomRight = mpts.Where(pt => pt.x > avgX && pt.y > avgY).First();
            MPoint bottomLeft = mpts.Where(pt => pt.x < avgX && pt.y > avgY).First();
            pts.Add(ConvertPt(topLeft));
            pts.Add(ConvertPt(topRight));
            pts.Add(ConvertPt(bottomRight));
            pts.Add(ConvertPt(bottomLeft));
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
                left = pts.Min(pt => pt.X) + 4;
                top = pts.Min(pt => pt.Y) + 4;
                right = pts.Max(pt => pt.X) - 3;
                bottom = pts.Max(pt => pt.Y) - 3;
                leftUICoord = (int)(usableWidth * left / width);
                topUICoord = (int)(usableHeight * top / height);
                rightUICoord = (int)(usableWidth * right / width);
                bottomUICoord = (int)(usableHeight * bottom / height);
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


        int Convert2UICoordXFromImage(float x)
        {
            return (int)(usableWidth * x / width);
        }
        int Convert2UICoordYFromImage(float y)
        {
            return (int)(usableHeight * y / height);
        }


        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            if (IsInDesignMode())
                return;
            base.OnRender(drawingContext);
           
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
                    System.Windows.Point startPt = new System.Windows.Point(Convert2UICoordXFromImage(pts[i].X), Convert2UICoordYFromImage(pts[i].Y));
                    System.Windows.Point endPt = new System.Windows.Point(Convert2UICoordXFromImage(pts[endIndex].X), Convert2UICoordYFromImage(pts[endIndex].Y));
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
                foreach(var circle in circles)
                {
                    System.Windows.Point uiPt = new System.Windows.Point(Convert2UICoordXFromImage(circle.Position.X), Convert2UICoordYFromImage(circle.Position.Y));
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
        
    }
}
