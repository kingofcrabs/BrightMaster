using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BrightMaster.data
{
    public class CustomCanvas : Canvas
    {
        
        ColorMap map;
        int[,] cmap = new int[128, 4];
        double min, max,adjustMax;

        public CustomCanvas()
        {
            map = new ColorMap();
            cmap = map.Jet();
        }
        

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            double width = this.ActualWidth;
            double height = this.ActualHeight;
            int cnt = 128;
            double dy = height / cnt;
            for (int i = 0; i < cnt; i++)
            {
                int colorIndex = i;
                SolidColorBrush brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                    (byte)cmap[colorIndex, 0], (byte)cmap[colorIndex, 1],
                    (byte)cmap[colorIndex, 2], (byte)cmap[colorIndex, 3]));
                drawingContext.DrawRectangle(brush, new Pen(brush, 1),
                    new System.Windows.Rect(0, height - dy - i * dy, width, dy));
            }


            double hUnit = (adjustMax / max) * height / 10.0;
            double vUnit = adjustMax / 10;
            double startX = width * 1.2;
            double yOffset = (max - adjustMax) / max * height;
            for (int i = 0; i < 10; i++)
            {
                double curHeight = i * hUnit + yOffset;
                double curV = adjustMax - vUnit * i;
                string sLV = ((int)curV).ToString();
                FormattedText ft = new FormattedText(sLV, CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, new Typeface("Arial"), 10, Brushes.Black);
                drawingContext.DrawLine(new Pen(Brushes.Black, 1), new Point(startX, curHeight), new Point(this.ActualWidth, curHeight));
                drawingContext.DrawText(ft, new Point(startX, curHeight));
            }


            if (this.Children.Count == 0)
                return;

            MyVisualHost visualHost = (MyVisualHost)this.Children[0];
            if (visualHost == null)
                return;
            if (visualHost.Visuals == null)
                return;

            List<Point> ptsMark = new List<Point>();
            foreach (MyDrawingVisual visual in visualHost.Visuals)
            {
                int x =  0;
                int y = visual.Name == "Higher" ? 0 : 512;
                Point pt = new Point(x + visual.Offset.X, y + visual.Offset.Y);
                ptsMark.Add(pt);
            }

            DrawMarkers(ptsMark, 120, drawingContext);
        }

        private void DrawMarkers(List<Point> ptsMark, int length, DrawingContext drawingContext)
        {
            //draw markers
            for (int i = 0; i < ptsMark.Count; i++)
            {
                Pen pen = i == 0 ? new Pen(Brushes.White, 1) : new Pen(Brushes.Black, 1);
                drawingContext.DrawLine(pen, ptsMark[i], new Point(ptsMark[i].X + length, ptsMark[i].Y + 0));
            }
        }

        internal void Update()
        {
            InvalidateVisual();
        }
    }

    public class MyDrawingVisual : DrawingVisual
    {
        public string Name { get; set; }
    }
    public class MyVisualHost : FrameworkElement
    {
        // Create a collection of child visual objects.
        private VisualCollection _children;
        MyDrawingVisual selectedVisual;
        List<Point> ptMarks = new List<Point>();

        //Dictionary<string, Point> markers = new Dictionary<string, Point>();
        public delegate void PositionChanged();
        public event PositionChanged onPositionChanged;
        CustomCanvas parent;
      


        public VisualCollection Visuals
        {
            get
            {
                return _children;
            }
        }



        public MyVisualHost(CustomCanvas parent)
        {
            this.parent = parent;
            _children = new VisualCollection(this);
            ptMarks.Add(new Point(0, 0)); // higher
            ptMarks.Add(new Point(0, 512)); // lower


            foreach (Point ptMark in ptMarks)
            {
                bool isHigher = ptMark.Y == 0;
                Visual visual = CreateDrawingVisualMarker(ptMark, isHigher);

                if (visual != null)
                    _children.Add(visual);
            }
            this.MouseMove += MyVisualHost_MouseMove;
            this.MouseLeftButtonDown += MyVisualHost_MouseLeftButtonDown;
            this.MouseLeftButtonUp += new MouseButtonEventHandler(MyVisualHost_MouseLeftButtonUp);
        }


        internal void onMouseUp()
        {
            if (selectedVisual != null)
                selectedVisual.Opacity = 1.0;
            selectedVisual = null;
            ReleaseMouseCapture();
        }


        private DrawingVisual CreateDrawingVisualMarker(Point ptMarker, bool bHigh)
        {
            MyDrawingVisual drawingVisual = new MyDrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            Point pos = ptMarker;
            Point ptUp = new Point(pos.X - 10, pos.Y - 10);
            Point ptDown = new Point(pos.X - 10, pos.Y + 10);
            Point ptRight = new Point(pos.X + 10, pos.Y);
            string s = string.Format("M {0},{1} {2},{3} {4},{5} {0},{1}", ptUp.X, ptUp.Y, ptDown.X, ptDown.Y, ptRight.X, ptRight.Y);
            Geometry geometry = Geometry.Parse(s);
            Brush brush = bHigh ? Brushes.Red : Brushes.SeaGreen;
            drawingContext.DrawGeometry(brush, new Pen(Brushes.Yellow, 1), geometry);
            drawingContext.Close();
            drawingVisual.Name = bHigh ? "Higher" : "Lower";
            return drawingVisual;

        }

    
        void MyVisualHost_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedVisual == null)
                return;

            int orgY = selectedVisual.Name == "Higher" ? 0 : 512;

            selectedVisual.Offset = new Vector(selectedVisual.Offset.X, e.GetPosition(this).Y - orgY);
            parent.Update();
            if (onPositionChanged != null)
                onPositionChanged();
        }

        void MyVisualHost_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point pt = e.GetPosition((UIElement)sender);

            // Initiate the hit test by setting up a hit test result callback method.
            VisualTreeHelper.HitTest(this, null, new HitTestResultCallback(myCallback), new PointHitTestParameters(pt));

            if (selectedVisual != null)
            {
                CaptureMouse();
            }
        }

        // Capture the mouse event and hit test the coordinate point value against
        // the child visual objects.
        void MyVisualHost_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Retreive the coordinates of the mouse button event.
            if (selectedVisual != null)
                selectedVisual.Opacity = 1.0;
            ReleaseMouseCapture();
            selectedVisual = null;
        }

        // If a child visual object is hit, toggle its opacity to visually indicate a hit.
        public HitTestResultBehavior myCallback(HitTestResult result)
        {
            selectedVisual = null;
            if (result.VisualHit.GetType() == typeof(MyDrawingVisual))
            {
                MyDrawingVisual hitViusal = (MyDrawingVisual)(result.VisualHit);

                ((DrawingVisual)result.VisualHit).Opacity = 0.2;
                selectedVisual = (MyDrawingVisual)result.VisualHit;
            }
            // Stop the hit test enumeration of objects in the visual tree.
            return HitTestResultBehavior.Stop;
        }

        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }

    }
}
