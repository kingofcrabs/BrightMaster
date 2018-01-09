using EngineDll;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BrightMaster
{
    public class Brightness
    {
        public LightPixelInfo[,] _allPixels;
        public byte[,] grayVals;
        public byte[,] sparseGrayVals;
        public byte[] grayValsInArray;
        
        public string ImagePath { get; set; }
        public int[] GrayLevelCounts = new int[256];
        public int Width { get; set; }
        public int Height { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }
        public double MaxROI { get; set; }
        public double MinROI { get; set; }
        public double Avg { get; set; }
        public PixelInfo Center { get; set; }
        public System.Drawing.Point MaxPosition { get; set; }
        public System.Drawing.Point MinPosition { get; set; }
        public Brightness(LightPixelInfo[,] pixels)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            _allPixels = pixels;
            Height = pixels.GetLength(0);
            Width = pixels.GetLength(1);

            grayVals = new byte[Height, Width];
            sparseGrayVals = new byte[Height, Width];
            Min = 999999;
            Max = 0;

            float[] maxArray = new float[4];
            float[] minArray = new float[4];
            float[] avg = new float[4];
            
            for(int i = 0; i< 4; i++)
            {
                maxArray[i] = 0;
                minArray[i] = 999999;
                avg[i] = 0;
            }
            unsafe
            {
                System.Drawing.Point maxPos = new System.Drawing.Point(-1,-1);
                System.Drawing.Point minPos = new System.Drawing.Point(-1,-1);
                Parallel.Invoke(() =>
                {
                    GetMaxMin(0, Height / 4,0,Width, ref maxArray[0], ref minArray[0],ref avg[0],ref maxPos,ref minPos);
                },
                 () =>
                 {
                     GetMaxMin(Height / 4, Height / 2, 0, Width, ref maxArray[1], ref minArray[1],ref avg[1], ref maxPos, ref minPos);
                 },
                 () =>
                 {
                     GetMaxMin(Height / 2, Height * 3 / 4, 0, Width, ref maxArray[2], ref minArray[2], ref avg[2],ref maxPos,ref minPos);
                 },
                 () =>
                 {
                     GetMaxMin(Height * 3 / 4, Height, 0, Width, ref maxArray[3], ref minArray[3], ref avg[3],ref maxPos,ref minPos);
                 });
                float sum = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (Max < maxArray[i])
                        Max = maxArray[i];
                    if (Min > minArray[i])
                        Min = minArray[i];
                    sum += avg[i];
                }
                Avg = sum / 4;
            }
            Debug.WriteLine(string.Format("Brightness find max min elapsed:{0}", watch.ElapsedMilliseconds));
            grayVals = Convert2Gray();
            Debug.WriteLine(string.Format("Brightness convert2Gray elapsed:{0}", watch.ElapsedMilliseconds));
            //SaveImage();
            //Debug.WriteLine(string.Format("Brightness save image elapsed:{0}", watch.ElapsedMilliseconds));
            watch.Stop();
        }


        internal void UpdateROI(List<System.Drawing.Point> pts)
        {
            MinROI = 999999;
            MaxROI = 0;
            float[] maxArray = new float[4];
            float[] minArray = new float[4];
            float[] avg = new float[4];
            PointF topLeft =     pts[0];
            PointF topRight =    pts[1];
            PointF bottomRight = pts[2];
            PointF bottomLeft =  pts[3];
            double height = bottomLeft.Y - topLeft.Y;
            double width = topRight.X - topLeft.X;
            var circle = GlobalVars.Instance.Layout.GetCenterCircle(new Rect(topLeft.X,topLeft.Y,bottomRight.X - topLeft.X,bottomRight.Y - topLeft.Y));
            Center = GetAvgVals(circle.x, circle.y, circle.radius, 0);

            for(int i = 0; i< 4; i++)
            {
                maxArray[i] = 0;
                minArray[i] = 999999;
                avg[i] = 0;
            }
            unsafe
            {
                double xMargin = GlobalVars.Instance.Layout.XMargin * width /100;
                double yMargin = GlobalVars.Instance.Layout.YMargin * height / 100;
                int xStart = (int)(topLeft.X + xMargin);
                int xEnd = (int)(topRight.X - xMargin);
                int yStart = (int)(topLeft.Y + yMargin);
                int yEnd = (int)(bottomRight.Y - yMargin);
                double roiHeight = yEnd - yStart;
                System.Drawing.Point[] maxPts = new System.Drawing.Point[4];
                System.Drawing.Point[] minPts = new System.Drawing.Point[4];
                Parallel.Invoke(() =>
                {
                    GetMaxMin(yStart, (int)(yStart + roiHeight / 4), xStart, xEnd, ref maxArray[0], ref minArray[0], ref avg[0], ref minPts[0],ref maxPts[0],pts);
                },
                () =>
                {
                    GetMaxMin((int)(yStart + roiHeight / 4), (int)(yStart + roiHeight / 2), xStart, xEnd, ref maxArray[1], ref minArray[1], ref avg[1], ref minPts[1], ref maxPts[1], pts);
                },
                () =>
                {
                    GetMaxMin((int)(yStart + roiHeight / 2), (int)(yStart + roiHeight * 3 / 4), xStart, xEnd, ref maxArray[2], ref minArray[2], ref avg[2], ref minPts[2], ref maxPts[2], pts);
                },
                () =>
                {
                    GetMaxMin((int)(yStart + roiHeight * 3 / 4), yEnd, xStart, xEnd, ref maxArray[3], ref minArray[3], ref avg[3], ref minPts[3], ref maxPts[3], pts);
                });
                float sum = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (MaxROI < maxArray[i])
                    {
                        MaxROI = maxArray[i];
                        MaxPosition = maxPts[i];
                    }
                        
                    if (MinROI > minArray[i])
                    {
                        MinROI = minArray[i];
                        MinPosition = minPts[i];
                    }
                        
                    sum += avg[i];
                }
                Avg = sum / 4;

            }
        }

        public static bool IsPointInPolygon4(PointF[] polygon, PointF testPoint)
        {
            bool result = false;
            int j = polygon.Count() - 1;
            for (int i = 0; i < polygon.Count(); i++)
            {
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        private void GetMaxMin(int yStart, int yEnd, int xStart, int xEnd, ref float max, ref float min, ref float avg, 
            ref System.Drawing.Point minPosition , ref System.Drawing.Point maxPosition,List<System.Drawing.Point> pts = null)
        {
            int cnt = 0;
            double sum = 0;
            PointF[] polygon = new PointF[4];
            if(pts != null)
            {
                float top = pts.Min(pt => pt.Y);
                float bottom = pts.Max(pt => pt.Y);
                float left = pts.Min(pt => pt.X);
                float right = pts.Max(pt => pt.X);
                //var polygon = pts.ToArray();
                
                for (int i = 0; i < 4; i++)
                    polygon[i] = pts[i];

                if (yStart < top)
                    yStart = (int)top;
                if (yEnd > bottom)
                    yEnd = (int)bottom;
                if (xStart < left)
                    xStart = (int)left;
                if (xEnd > right)
                    xEnd = (int)right;
            }
            

            for (int y = yStart; y < yEnd; y++)
            {
                for (int x = xStart; x <xEnd; x++)
                {
                    if(pts != null)
                    {
                        if (!IsPointInPolygon4(polygon, new PointF(x, y)))
                            continue;
                    }
                    
                    if (max < _allPixels[y, x].Y)
                    {
                        max = _allPixels[y, x].Y;
                        maxPosition = new System.Drawing.Point(x, y);
                    }
                        
                    if (min > _allPixels[y, x].Y)
                    {
                        min = _allPixels[y, x].Y;
                        minPosition = new System.Drawing.Point(x, y);
                    }
                        
                    sum += _allPixels[y, x].Y;
                    cnt++;
                }
            }
            avg = (float)(sum / cnt);
        }


        //private void GetMaxMinFromX(int xStart, int xEnd, ref float max, ref float min)
        //{

        //    for (int y = 0; y < Height; y++)
        //    {
        //        for (int x = xStart; x < xEnd; x++)
        //        {
        //            if (max < _allPixels[y, x].Y)
        //                max = _allPixels[y, x].Y;
        //            if (min > _allPixels[y, x].Y)
        //                min = _allPixels[y, x].Y;
        //        }
        //    }
        //}

        public List<PixelInfo> GetPixelInfos(List<System.Drawing.Point> pts)
        {
            if (pts.Count != 4)
            {
                return null;
            }
           
            List<PixelInfo> pixelInfos = new List<PixelInfo>();
            var circles = GlobalVars.Instance.Layout.GetCircles(pts);
            int id = 1;
            foreach(var circle in circles)
            {
                pixelInfos.Add(GetAvgVals(circle.x, circle.y, circle.radius,id++));
            }
            return pixelInfos;
        }

        private PixelInfo GetAvgVals(float xx, float yy, float radius, int ID)
        {
            List<LightPixelInfo> vals = new List<LightPixelInfo>();
            int xStart = (int)(xx - radius);
            int yStart = (int)(yy - radius);
            int xEnd = (int)(xx + radius);
            int yEnd = (int)(yy + radius);
            unsafe
            {
                for (int x = xStart; x < xEnd; x++)
                {
                    for (int y = yStart; y < yEnd; y++)
                    {
                        if (GetDistance(x, y, xx, yy) <= radius)
                            vals.Add(_allPixels[y, x]);
                    }
                }
            }
          
            float X,Y,Z;

            X = vals.Average(val => val.X);
            Y = vals.Average(val => val.Y);
            Z = vals.Average(val => val.Z);
            return new PixelInfo(ID,X, Y, Z);
        }

        private float GetDistance(int x, int y, float xx, float yy)
        {
            float xDis = xx- x;
            float yDis = yy - y;
            return (float)Math.Sqrt(xDis * xDis + yDis * yDis);
        }

        public Bitmap GetBitmap()
        {
            Bitmap bmp = new Bitmap(Width, Height);
            LockBitmap lockBmp = new LockBitmap(bmp);
            unsafe
            {
                lockBmp.LockBits();
                for (int y = 0; y < lockBmp.Height; y++)
                {
                    List<double> vals = new List<double>();
                    for (int x = 0; x < lockBmp.Width; x++)
                    {
                        byte grayVal = grayVals[y, x];
                        lockBmp.SetPixel(x, y, Color.FromArgb(grayVal, grayVal, grayVal));
                    }
                }
                lockBmp.UnlockBits();
            }
            return bmp;
        }
        public void SaveImage()
        {
            if (ImagePath != null && ImagePath != "")
                return;

            Bitmap bmp = new Bitmap(Width,Height);
            LockBitmap lockBmp = new LockBitmap(bmp);
            unsafe
            {
                lockBmp.LockBits();
                for (int y = 0; y < lockBmp.Height; y++)
                {
                    List<double> vals = new List<double>();
                    for (int x = 0; x < lockBmp.Width; x++)
                    {
                        byte grayVal = grayVals[y, x];
                        lockBmp.SetPixel(x, y, Color.FromArgb(grayVal, grayVal, grayVal));
                    }
                }
                lockBmp.UnlockBits();
            }
            ImagePath = FolderHelper.GetImageFolder() + "latest.jpg";
            bmp.Save(ImagePath);
        }


        public byte[,] GetSparseGrayLevels(int grayLevelCnt)
        {
            return Convert2Gray(grayLevelCnt);
        }

        private byte[,] Convert2Gray( int grayLevelCnt = 256)
        {
           
            byte[,] vals = new byte[Height,Width];
            for (int i = 0; i < 255; i++)
            {
                GrayLevelCounts[i] = 0;
            }
            byte[] map = new byte[256];
            long[] lCounts = new long[256];
            //each gray level count
         
            int grayUnit = (int)((Max - Min) / grayLevelCnt);
            grayUnit = Math.Max(grayUnit, 1);
            grayValsInArray = new byte[Height * Width];
            int grayValsPerLevel = 256 / grayLevelCnt;
            unsafe
            {
                Parallel.Invoke(() =>
                 {
                    GetGrayVal(0,Height/4,grayUnit,grayValsPerLevel, vals, _allPixels,Width,Height);
                 },
                 () =>
                 {
                     GetGrayVal(Height / 4, Height / 2, grayUnit, grayValsPerLevel, vals, _allPixels, Width, Height);
                 },
                 () =>
                 {
                     GetGrayVal(Height / 2, Height * 3 / 4, grayUnit, grayValsPerLevel, vals, _allPixels, Width, Height);
                 },
                 () =>
                 {
                     GetGrayVal(Height * 3 / 4, Height, grayUnit, grayValsPerLevel, vals, _allPixels, Width, Height);
                 }
             );
            }
            return vals;
        }

        private void GetGrayVal(int yStart, int yEnd, int grayUnit, double grayValsPerLevel, 
            byte[,] vals, 
            LightPixelInfo[,] lightPixels,
            int Width,
            int Height)
        {

            for (int y = yStart; y < yEnd; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int grayLevels = ((int)(lightPixels[y, x].Y - Min)) / grayUnit;
                    int val = (int)(grayLevels * grayValsPerLevel);
                    if (val > 255)
                        val = 255;
                    vals[y, x] = (byte)val;
                }
            }
        }

        internal List<float> GetHorizontoalLineVals(int y)
        {
            List<float> vals = new List<float>();
            for (int x = 0; x < Width; x++)
                vals.Add(_allPixels[y, x].Y);
            return vals;
        }

        internal List<float> GetVerticalLineVals(int x)
        {
            List<float> vals = new List<float>();
            for (int y = 0; y < Height; y++)
                vals.Add(_allPixels[y, x].Y );
            return vals;
        }

        internal void ClearGrayImage()
        {
            ImagePath = "";
        }

        internal void ClearMinMaxPosition()
        {
            MinPosition = new System.Drawing.Point(-1, -1);
            MaxPosition = new System.Drawing.Point(-1, -1);
        }
    }
}
