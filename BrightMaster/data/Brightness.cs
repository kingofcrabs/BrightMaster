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

        public double MaxNoMargin { get; set; }
        public double MinNoMargin { get; set; }
        public double Avg { get; set; }

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
                int xMargin = GlobalVars.Instance.Layout.XMarginPixel;
                int yMargin = GlobalVars.Instance.Layout.YMarginPixel;
                Parallel.Invoke(() =>
                {
                    GetMaxMin(yMargin, Height / 4,xMargin,yMargin, ref maxArray[0], ref minArray[0],ref avg[0]);
                },
                 () =>
                 {
                     GetMaxMin(Height / 4, Height / 2, xMargin, yMargin, ref maxArray[1], ref minArray[1],ref avg[1]);
                 },
                 () =>
                 {
                     GetMaxMin(Height / 2, Height * 3 / 4, xMargin, yMargin, ref maxArray[2], ref minArray[2],ref avg[2]);
                 },
                 () =>
                 {
                     GetMaxMin(Height * 3 / 4, Height - yMargin, xMargin, yMargin, ref maxArray[3], ref minArray[3],ref avg[3]);
                 });
                float sum = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (MaxNoMargin < maxArray[i])
                        MaxNoMargin = maxArray[i];
                    if (MinNoMargin > minArray[i])
                        MinNoMargin = minArray[i];
                    sum += avg[i];
                }
                Avg = sum / 4;

                Parallel.Invoke(() =>
                {
                    GetMaxMin(0, yMargin, xMargin, yMargin, ref maxArray[0], ref minArray[0], ref avg[0]);
                },
                () =>
                {
                    GetMaxMin(Height-yMargin, Height, xMargin, yMargin, ref maxArray[1], ref minArray[1], ref avg[1]);
                },
                () =>
                {
                    GetMaxMinFromX(0, xMargin,  ref maxArray[2], ref minArray[2]);
                },
                () =>
                {
                    GetMaxMinFromX(Width - xMargin, Width, ref maxArray[3], ref minArray[3]);
                });
                for (int i = 0; i < 4; i++)
                {
                    if (Max < maxArray[i])
                        Max = maxArray[i];
                    if (Min > minArray[i])
                        Min = minArray[i];
                }
                Max = Math.Max(Max, MaxNoMargin);
                Min = Math.Min(Min, MinNoMargin);
            }
            Debug.WriteLine(string.Format("Brightness find max min elapsed:{0}", watch.ElapsedMilliseconds));
            grayVals = Convert2Gray();
            Debug.WriteLine(string.Format("Brightness convert2Gray elapsed:{0}", watch.ElapsedMilliseconds));
            
            //SaveImage();
            Debug.WriteLine(string.Format("Brightness save image elapsed:{0}", watch.ElapsedMilliseconds));
            watch.Stop();
        }

        private void GetMaxMin(int yStart, int yEnd,int xMargin, int yMargin, ref float max, ref float min,ref float avg)
        {
            int cnt = 0;
            double sum = 0;
            for (int y = yStart; y < yEnd; y++)
            {
                for (int x = xMargin; x < Width-xMargin; x++)
                {
                    if (max < _allPixels[y, x].Y)
                        max = _allPixels[y, x].Y;
                    if (min > _allPixels[y, x].Y)
                        min = _allPixels[y, x].Y;
                    sum += _allPixels[y, x].Y;
                    cnt++;
                }
            }
            avg = (float)(sum / cnt);
        }


        private void GetMaxMinFromX(int xStart, int xEnd, ref float max, ref float min)
        {

            for (int y = 0; y < Height; y++)
            {
                for (int x = xStart; x < xEnd; x++)
                {
                    if (max < _allPixels[y, x].Y)
                        max = _allPixels[y, x].Y;
                    if (min > _allPixels[y, x].Y)
                        min = _allPixels[y, x].Y;
                }
            }
        }

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
            if (ImagePath != "")
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

        private byte[,] Convert2Gray( int grayLevelCnt = 255)
        {
           
            byte[,] vals = new byte[Height,Width];
            for (int i = 0; i < 255; i++)
            {
                GrayLevelCounts[i] = 0;
            }
            byte[] map = new byte[256];
            long[] lCounts = new long[256];
            //each gray level count
         
            double grayUnit = (Max - Min) / grayLevelCnt;
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

        private void GetGrayVal(int yStart, int yEnd, double grayUnit, double grayValsPerLevel, 
            byte[,] vals, 
            LightPixelInfo[,] lightPixels,
            int Width,
            int Height)
        {

            double k = grayValsPerLevel / grayUnit;
            for (int y = yStart; y < yEnd; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    byte val = (byte)((int)(lightPixels[y, x].Y - Min)*k);
                    vals[y, x] = val;
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



        internal PixelInfo GetCenterInfo()
        {
            return new PixelInfo(1, _allPixels[Height / 2, Width / 2]);
        }
    }
}
