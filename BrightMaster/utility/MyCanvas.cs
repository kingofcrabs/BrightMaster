using BrightMaster.Settings;
using BrightMaster.utility;
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
    class BrightCanvas : Canvas
    {
   
        double width;
        double height;
        double usableWidth;
        double usableHeight; 
        
        List<PointF> pts = null;
        Point ptStart = new Point(-1,-1);
        Point ptEnd = new Point(-1,-1);
        Point ptCalculateCurve = new Point(-1, -1);
        bool validMouseMove = false;
        bool userSelectROI = false;

        Point minPt = new Point(-1, -1);
        Point maxPt = new Point(-1, -1);

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
                pts = new List<PointF>();
                userSelectROI = value;
                InvalidateVisual();
            }
        }

        public BrightCanvas()
        {
            this.SizeChanged += MyCanvas_SizeChanged;
        }

        void MyCanvas_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (IsInDesignMode())
                return;
            CalcuUsable(out usableWidth, out usableHeight);
            if(this.Background is ImageBrush)
            {
                ImageBrush imgBrush = (ImageBrush)this.Background;
                imgBrush.Viewport = new System.Windows.Rect(0, 0, usableWidth, usableHeight);
            }
            InvalidateVisual();
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

        PointF PointFMulRatio(PointF ptF,float xRatio, float yRatio)
        {
            return new PointF(ptF.X*xRatio, ptF.Y*yRatio);
        }

        public void SetBoundRect(SizeF boundRectSize)
        {
            Layout layout = GlobalVars.Instance.Layout;
            pts = new List<PointF>();
            pts.Add(PointFMulRatio(boundRectSize.ToPointF(), layout.ROITopLeftXRatio / 100, layout.ROITopLeftYRatio / 100));
            pts.Add(PointFMulRatio(boundRectSize.ToPointF(), layout.ROIBottomRightXRatio / 100, layout.ROITopLeftYRatio / 100));
            pts.Add(PointFMulRatio(boundRectSize.ToPointF(), layout.ROIBottomRightXRatio / 100, layout.ROIBottomRightYRatio / 100));
            pts.Add(PointFMulRatio(boundRectSize.ToPointF(), layout.ROITopLeftXRatio / 100, layout.ROIBottomRightYRatio / 100));
            width = boundRectSize.Width;
            height = boundRectSize.Height;
            CalcuUsable(out usableWidth, out usableHeight);
            InvalidateVisual();
        }


        public void SetBkGroundImage(System.Windows.Media.Imaging.BitmapImage bmpImage, List<PointF> newPts = null, bool isFakeColor = false)
        {
            if(!isFakeColor) //for fake color, we don't change the points
                pts = newPts;
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
        
       

      

        private System.Windows.Rect GetBoundingRectInUICoordinate(List<PointF> pts)
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
                    leftUICoord = (int)(pts.Min(pt => pt.X)+0.5);
                    topUICoord = (int)(pts.Min(pt => pt.Y)+0.5);
                    rightUICoord = (int)(pts.Max(pt => pt.X)+0.5);
                    bottomUICoord = (int)(pts.Max(pt => pt.Y)+0.5);
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
            return (int)(usableWidth * x / width+0.5);
        }
        int Convert2UIYFromReal(float y)
        {
            return (int)(usableHeight * y / height+0.5);
        }

        int Convert2RealXFromUI(float x)
        {
            return (int)(width * x / usableWidth+0.5);
        }
        int Convert2RealYFromUI(float y)
        {
            return (int)(height * y / usableHeight+0.5);
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
            System.Windows.Media.Brush blueBrush = System.Windows.Media.Brushes.Blue;
            System.Windows.Media.Pen pen = new System.Windows.Media.Pen(redBrush,1);
            System.Windows.Media.Pen bluePen = new System.Windows.Media.Pen(blueBrush, 1);
            System.Windows.Media.Pen dashPen = new System.Windows.Media.Pen(blueBrush, 1);
            dashPen.DashStyle = DashStyles.Dash;
            bool validPt = pts != null && pts.Count == 4;
            if (validPt)
            {

                //draw bound rect
                var hullPts = GlobalVars.Instance.MiscSettings.HullPts;
                if(hullPts != null && hullPts.Count > 0)
                {
                    DrawConvexHull(hullPts, drawingContext);
                }
                else //user selected roi
                {
                    DrawUserSelectedROI(pts, drawingContext);
                }
                
                //draw circles or marks
                if (GlobalVars.Instance.AnalysisRegions) //regions analysis
                {
                    var roiPts = Layout.Convert2ROI(pts);
                    var circles = validPt ? GlobalVars.Instance.Layout.GetCircles(roiPts) :
                    GlobalVars.Instance.Layout.GetCircles(boundingRectUICoord);
                    
                    int index = 0;
                    if (userSelectROI && ptEnd.X == -1)
                        return;
                    foreach (var circle in circles)
                    {
                        System.Windows.Point uiPt = new System.Windows.Point(Convert2XUIFromReal(circle.Position.X), Convert2UIYFromReal(circle.Position.Y));
                        float radiusUI = Convert2XUIFromReal(circle.Size.X);
                        DrawCircle(uiPt, new PointF(radiusUI, radiusUI), drawingContext);
                        DrawText((index + 1).ToString(), uiPt, drawingContext, 12);
                        index++;
                    }
                }
                else //global analysis
                {
                    if(maxPt.X != -1)
                    {
                        DrawCross(new System.Windows.Point(Convert2XUIFromReal(maxPt.X), Convert2XUIFromReal(maxPt.Y)), new PointF(10, 10), drawingContext);
                        DrawCross(new System.Windows.Point(Convert2XUIFromReal(minPt.X), Convert2XUIFromReal(minPt.Y)), new PointF(10, 10), drawingContext, false);
                    }
                }
            }
            else
            {
                drawingContext.DrawRectangle(null, new System.Windows.Media.Pen(redBrush, 1), boundingRectUICoord);
            }

            if(ptCalculateCurve.X != -1)
            {
                pen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.Blue, 1);
                drawingContext.DrawLine(pen, new  System.Windows.Point(0,ptCalculateCurve.Y),new System.Windows.Point(this.ActualWidth,ptCalculateCurve.Y));
                drawingContext.DrawLine(pen, new System.Windows.Point(ptCalculateCurve.X,0), new System.Windows.Point(ptCalculateCurve.X,this.ActualHeight));
            }

            
        }

        private void DrawConvexHull(List<PointF> hullPts, DrawingContext drawingContext)
        {
            System.Windows.Media.Brush redBrush = System.Windows.Media.Brushes.Red;
            System.Windows.Media.Brush blueBrush = System.Windows.Media.Brushes.Blue;
            System.Windows.Media.Pen pen = new System.Windows.Media.Pen(redBrush, 1);
            System.Windows.Media.Pen bluePen = new System.Windows.Media.Pen(blueBrush, 1);
            for (int i = 0; i < hullPts.Count; i++)
            {
                int endIndex = (i + 1) % hullPts.Count;
                var startPt = new System.Windows.Point(Convert2XUIFromReal(hullPts[i].X), Convert2UIYFromReal(hullPts[i].Y));
                var endPt = new System.Windows.Point(Convert2XUIFromReal(hullPts[endIndex].X), Convert2UIYFromReal(hullPts[endIndex].Y));
                drawingContext.DrawLine(pen, startPt, endPt);
            }
            ShrinkHelper shrinkHelper = new ShrinkHelper();
            var shrinkedHullPts = shrinkHelper.ShrinkConvexHull(hullPts);
            for (int i = 0; i < shrinkedHullPts.Count; i++)
            {
                int endIndex = (i + 1) % shrinkedHullPts.Count;
                var startPt = new System.Windows.Point(Convert2XUIFromReal(shrinkedHullPts[i].X), Convert2UIYFromReal(shrinkedHullPts[i].Y));
                var endPt = new System.Windows.Point(Convert2XUIFromReal(shrinkedHullPts[endIndex].X), Convert2UIYFromReal(shrinkedHullPts[endIndex].Y));
                drawingContext.DrawLine(bluePen, startPt, endPt);
            }
        }

        private void DrawUserSelectedROI(List<PointF> pts, DrawingContext drawingContext)
        {
            System.Windows.Media.Brush redBrush = System.Windows.Media.Brushes.Red;
            System.Windows.Media.Brush blueBrush = System.Windows.Media.Brushes.Blue;
            System.Windows.Media.Pen pen = new System.Windows.Media.Pen(redBrush, 1);
            System.Windows.Media.Pen bluePen = new System.Windows.Media.Pen(blueBrush, 1);
            System.Windows.Media.Pen dashPen = new System.Windows.Media.Pen(blueBrush, 1);
            if(GlobalVars.Instance.AnalysisRegions)//no shrink for regions analysis
            {
                //draw org select boundary
                for (int i = 0; i < 4; i++)
                {
                    int endIndex = (i + 1) % 4;
                    var startPt = new System.Windows.Point(Convert2XUIFromReal(pts[i].X), Convert2UIYFromReal(pts[i].Y));
                    var endPt = new System.Windows.Point(Convert2XUIFromReal(pts[endIndex].X), Convert2UIYFromReal(pts[endIndex].Y));
                    drawingContext.DrawLine(bluePen, startPt, endPt);
                }
                var roiPts = Layout.Convert2ROI(pts);
                for (int i = 0; i < 4; i++)
                {
                    int endIndex = (i + 1) % 4;
                    var startPt = new System.Windows.Point(Convert2XUIFromReal(roiPts[i].X), Convert2UIYFromReal(roiPts[i].Y));
                    var endPt = new System.Windows.Point(Convert2XUIFromReal(roiPts[endIndex].X), Convert2UIYFromReal(roiPts[endIndex].Y));
                    drawingContext.DrawLine(pen, startPt, endPt);
                }
            }
            else
            {
                ShrinkHelper shrinkHelper = new ShrinkHelper();
                var shrinkPts = shrinkHelper.ShrinkRect(pts);
                for (int i = 0; i < 4; i++)
                {
                    int endIndex = (i + 1) % 4;
                    var startPt = new System.Windows.Point(Convert2XUIFromReal(pts[i].X), Convert2UIYFromReal(pts[i].Y));
                    var endPt = new System.Windows.Point(Convert2XUIFromReal(pts[endIndex].X), Convert2UIYFromReal(pts[endIndex].Y));
                    var startPtOffSet = new System.Windows.Point(Convert2XUIFromReal(shrinkPts[i].X), Convert2UIYFromReal(shrinkPts[i].Y));
                    var endPtOffset = new System.Windows.Point(Convert2XUIFromReal(shrinkPts[endIndex].X), Convert2UIYFromReal(shrinkPts[endIndex].Y));
                    drawingContext.DrawLine(pen, startPt, endPt);

                    //draw dot line
                    double[] dashValues = { 5, 2 };
                    dashPen.DashStyle = new DashStyle(dashValues, 1);
                    drawingContext.DrawLine(dashPen, startPtOffSet, endPtOffset);
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

        private void DrawCross(System.Windows.Point pt, PointF sz, DrawingContext drawingContext, bool red = true)
        {
            System.Windows.Media.Brush brush = red ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Green;
            System.Windows.Media.Pen pen = new System.Windows.Media.Pen(brush,1);
            var ptUp = new System.Windows.Point(pt.X,pt.Y+5);
            var ptDown = new System.Windows.Point(pt.X,pt.Y-5);
            drawingContext.DrawLine(pen, ptUp, ptDown);
            var ptLeft = new System.Windows.Point(pt.X-5, pt.Y);
            var ptRight = new System.Windows.Point(pt.X+5, pt.Y);
            drawingContext.DrawLine(pen, ptLeft, ptRight);
        }
        private void DrawCircle(System.Windows.Point pt, PointF sz, DrawingContext drawingContext,bool red = true)
        {
            System.Windows.Media.Brush brush = red ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Blue;
            drawingContext.DrawEllipse(null, new System.Windows.Media.Pen(brush, 1),
                pt, sz.X, sz.Y);
        }

        public void ClearMinMaxPoint()
        {
            this.maxPt = new Point(-1, -1);
            this.minPt = new Point(-1, -1);
            InvalidateVisual();
        }

        public void SetMaxMinPosition(PointF maxPt, PointF minPt)
        {
            this.maxPt = new Point((int)maxPt.X, (int)maxPt.Y);
            this.minPt = new Point((int)minPt.X, (int)minPt.Y);
            InvalidateVisual();
        }

        internal void OnLeftButtonDown(System.Windows.Point mousePos)
        {
            ptEnd.X = -1;
            validMouseMove = true;
            ptStart = new Point((int)mousePos.X, (int)mousePos.Y);
            InvalidateVisual();
        }

        internal void OnLeftButtonMove(System.Windows.Point mousePos)
        {
            if (!validMouseMove)
                return;
            ptEnd = new Point((int)mousePos.X, (int)mousePos.Y);
            if (ptEnd.X == ptStart.X || ptEnd.Y == ptStart.Y)
                return;
            pts = GeneratePtsRealImageCoord();
            InvalidateVisual();
        }

        internal List<PointF> OnLeftButtonUp()
        {
            if (ptEnd.X == ptStart.X || ptEnd.Y == ptStart.Y)
                return null;

            pts = GeneratePtsRealImageCoord();
            GlobalVars.Instance.MiscSettings.BoundaryPts = pts;
            InvalidateVisual();
            validMouseMove = false;
            return pts;
        }

        private List<PointF> GeneratePts(PointF ptStart, PointF ptEnd)
        {
            List<PointF> tmpPts = new List<PointF>();
            tmpPts.Add(ptStart);
            tmpPts.Add(new PointF(ptEnd.X,ptStart.Y));
            tmpPts.Add(ptEnd);
            tmpPts.Add(new PointF(ptStart.X, ptEnd.Y));
            return tmpPts;
        }


        public List<PointF> GeneratePtsRealImageCoord()
        {
            List<PointF> tmpPts = new List<PointF>();
            tmpPts.Add(ptStart);
            tmpPts.Add(new PointF(ptEnd.X, ptStart.Y));
            tmpPts.Add(ptEnd);
            tmpPts.Add(new PointF(ptStart.X, ptEnd.Y));

            List<PointF> tmpRealPts = new List<PointF>();
            tmpPts.ForEach(pt => tmpRealPts.Add(new PointF(Convert2RealXFromUI(pt.X), Convert2RealYFromUI(pt.Y))));
            return tmpRealPts;
        }



        internal void OnCurveMouseDown(System.Windows.Point pt)
        {
            ptCalculateCurve = new Point((int)pt.X, (int)pt.Y);
            InvalidateVisual();
        }
    }
}
