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
        
        List<MPoint> pts = null;
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



        public void SetBkGroundImage(System.Windows.Media.Imaging.BitmapImage bmpImage, List<MPoint> pts = null)
        {
            if(pts != null)
                this.pts = pts;
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

        private System.Windows.Rect GetBoundingRectInUICoordinate(List<MPoint> pts)
        {
            double left, top, right, bottom;
            int leftUICoord, topUICoord, rightUICoord, bottomUICoord;
            if(pts == null)
            {
                leftUICoord = 0;
                topUICoord = 0;
                bottomUICoord = (int)this.ActualHeight;
                rightUICoord = (int)this.ActualWidth;
            }
            else
            {
                left = pts.Min(pt => pt.x) + 4;
                top = pts.Min(pt => pt.y) + 4;
                right = pts.Max(pt => pt.x) - 3;
                bottom = pts.Max(pt => pt.y) - 3;
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



        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            if (IsInDesignMode())
                return;
            base.OnRender(drawingContext);
           
            if (layout == null)
                return;

            var boundingRectUICoord = GetBoundingRectInUICoordinate(pts);
            System.Windows.Media.Brush redBrush = System.Windows.Media.Brushes.Red;
            drawingContext.DrawRectangle(null, new System.Windows.Media.Pen(redBrush, 1), boundingRectUICoord);
            if(layout != null)
            {
                var circles = GlobalVars.Instance.Layout.GetCircles(boundingRectUICoord);
                 
                foreach(var circle in circles)
                    DrawCircle(circle.Position, circle.Size, drawingContext);
            }
        }

        private void DrawCircle(PointF pt, PointF sz, DrawingContext drawingContext)
        {
            drawingContext.DrawEllipse(null, new System.Windows.Media.Pen(System.Windows.Media.Brushes.Red,1),
                new System.Windows.Point(pt.X, pt.Y), sz.X, sz.Y);
        }
        
    }
}
