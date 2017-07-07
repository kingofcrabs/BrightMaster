using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;


namespace BrightMaster
{
    class MyCanvas : Canvas
    {
        Layout layout;
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
        

        public void SetBkGroundImage(System.Windows.Media.Imaging.BitmapImage bmpImage)
        {
            //shapes.Clear();
            System.Windows.Media.ImageBrush imageBrush = new System.Windows.Media.ImageBrush();
            imageBrush.ImageSource = bmpImage;
            this.Background = imageBrush;
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

            PointF ptStart = Convert2UI(layout.topLeft);
            PointF ptEnd = Convert2UI(layout.bottomRight);
            PointF sz = Convert2UI(new PointF(layout.radius,layout.radius));
            if(layout.xCount == 1 || layout.yCount == 1)
            {
                DrawCircle(ptStart, sz,drawingContext);
            }

            for(int x = 0; x< layout.xCount; x++)
            {
                for(int y = 0; y< layout.yCount; y++)
                {
                    float xx = (ptEnd.X - ptStart.X) * x / (layout.xCount - 1) + ptStart.X;
                    float yy = (ptEnd.Y - ptStart.Y) * y / (layout.yCount - 1) + ptStart.Y;
                    DrawCircle(new PointF(xx, yy), sz, drawingContext);
                }
            }

        }

        private void DrawCircle(PointF pt, PointF sz, DrawingContext drawingContext)
        {
            drawingContext.DrawEllipse(null, new System.Windows.Media.Pen(System.Windows.Media.Brushes.Red,1),
                new System.Windows.Point(pt.X, pt.Y), sz.X, sz.Y);
        }

        private PointF Convert2UI(PointF pt)
        {
            double x = pt.X * this.ActualWidth / layout.width;
            double y = pt.Y * this.ActualHeight / layout.height;
            return new PointF((float)x, (float)y);
        }

        
    }
}
