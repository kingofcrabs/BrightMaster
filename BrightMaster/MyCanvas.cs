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
            base.OnRender(drawingContext);
            if (IsInDesignMode())
                return;
            if (layout == null)
                return;

            var boundingRectUICoord = GetBoundingRectInUICoordinate(pts);
            System.Windows.Media.Brush redBrush = System.Windows.Media.Brushes.Red;
            drawingContext.DrawRectangle(null, new System.Windows.Media.Pen(redBrush, 1), boundingRectUICoord);
            if(layout != null)
            {
                float xStart = (float)(boundingRectUICoord.X +  layout.topLeft.X / layout.Width *  boundingRectUICoord.Width);
                float yStart = (float)(boundingRectUICoord.Y + layout.topLeft.Y / layout.Height * boundingRectUICoord.Height);
                float xEnd = (float)(boundingRectUICoord.X + layout.bottomRight.X / layout.Width * boundingRectUICoord.Width);
                float yEnd = (float)(boundingRectUICoord.Y + layout.bottomRight.Y / layout.Height * boundingRectUICoord.Height);
                float radius =(float)(layout.Radius / layout.Width * boundingRectUICoord.Width);
                PointF sz = new PointF(radius,radius);
                PointF ptStart = new PointF(xStart, yStart);
                PointF ptEnd = new PointF(xEnd, yEnd);
                if (layout.XCount == 1 || layout.YCount == 1)
                {
                    DrawCircle(ptStart, sz, drawingContext);
                }
                else
                {
                    for (int x = 0; x < layout.XCount; x++)
                    {
                        for (int y = 0; y < layout.YCount; y++)
                        {
                            float xx = (ptEnd.X - ptStart.X) * x / (layout.XCount - 1) + ptStart.X;
                            float yy = (ptEnd.Y - ptStart.Y) * y / (layout.YCount - 1) + ptStart.Y;
                            DrawCircle(new PointF(xx, yy), sz, drawingContext);
                        }
                    }
                }
            }

            //double xxx = Mouse.GetPosition(this).X;
            //double yyy = Mouse.GetPosition(this).Y;
            //drawingContext.DrawText(new FormattedText(string.Format("x:{0} y:{1}", xxx, yyy),
            //           CultureInfo.GetCultureInfo("en-us"),
            //           0,
            //           new Typeface("Verdana"),
            //           15, System.Windows.Media.Brushes.DarkBlue),
            //           new System.Windows.Point(ActualWidth - 150, ActualHeight - 100));

        }

        private void DrawCircle(PointF pt, PointF sz, DrawingContext drawingContext)
        {
            drawingContext.DrawEllipse(null, new System.Windows.Media.Pen(System.Windows.Media.Brushes.Red,1),
                new System.Windows.Point(pt.X, pt.Y), sz.X, sz.Y);
        }

        private PointF Convert2UI(PointF pt)
        {
            double x = pt.X * this.usableWidth / layout.Width;
            double y = pt.Y * this.usableHeight / layout.Height;
            return new PointF((float)x, (float)y);
        }

        
    }
}
