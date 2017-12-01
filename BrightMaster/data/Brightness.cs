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
        public string jpgFilePath = "";
        public int[] GrayLevelCounts = new int[256];
        public int Width { get; set; }
        public int Height { get; set; }

        public double Max { get; set; }
        public double Min { get; set; }

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
            for(int i = 0; i< 4; i++)
            {
                maxArray[i] = 0;
                minArray[i] = 999999;
            }
            unsafe
            {

                Parallel.Invoke(() =>
                {
                    GetMaxMin(0, Height / 4, ref maxArray[0],ref minArray[0]);
                },
                 () =>
                 {
                     GetMaxMin(Height / 4,Height/2, ref maxArray[1], ref minArray[1]);
                 },
                 () =>
                 {
                     GetMaxMin(Height/2, Height*3/4, ref maxArray[2], ref minArray[2]);
                 },
                 () =>
                 {
                     GetMaxMin(Height*3/4, Height, ref maxArray[3], ref minArray[3]);
                 });
                for (int i = 0; i < 4; i++)
                {
                    if (Max < maxArray[i])
                        Max = maxArray[i];
                    if (Min > minArray[i])
                        Min = minArray[i];
                }
                Max *= GlobalVars.Instance.RecipeCollection.SelectedRecipe.AdjustRatio.YRatio;
                Min *= GlobalVars.Instance.RecipeCollection.SelectedRecipe.AdjustRatio.YRatio;
            }
            Debug.WriteLine(string.Format("Brightness find max min elapsed:{0}", watch.ElapsedMilliseconds));
            grayVals = Convert2Gray();
            Debug.WriteLine(string.Format("Brightness convert2Gray elapsed:{0}", watch.ElapsedMilliseconds));
            
            //SaveImage();
            Debug.WriteLine(string.Format("Brightness save image elapsed:{0}", watch.ElapsedMilliseconds));
            watch.Stop();
        }

        private void GetMaxMin(int yStart, int yEnd, ref float max, ref float min)
        {

            for (int y = yStart; y < yEnd; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (max < _allPixels[y, x].Y)
                        max = _allPixels[y, x].Y;
                    if (min > _allPixels[y, x].Y)
                        min = _allPixels[y, x].Y;
                }
            }
        }

        public List<PixelInfo> GetResults(List<System.Drawing.Point> pts)
        {
            if (pts.Count != 4)
            {
                return null;
            }
           
            List<PixelInfo> results = new List<PixelInfo>();
            var circles = GlobalVars.Instance.Layout.GetCircles(pts);
            int id = 1;
            foreach(var circle in circles)
            {
                results.Add(GetAvgVals(circle.x, circle.y, circle.radius,id++));
            }
            return results;
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

            X = vals.Average(val => val.X) * GlobalVars.Instance.RecipeCollection.SelectedRecipe.AdjustRatio.XRatio;
            Y = vals.Average(val => val.Y) * GlobalVars.Instance.RecipeCollection.SelectedRecipe.AdjustRatio.YRatio;
            Z = vals.Average(val => val.Z) * GlobalVars.Instance.RecipeCollection.SelectedRecipe.AdjustRatio.ZRatio;
            return new PixelInfo(ID,X, Y, Z);
        }

        private float GetDistance(int x, int y, float xx, float yy)
        {
            float xDis = xx- x;
            float yDis = yy - y;
            return (float)Math.Sqrt(xDis * xDis + yDis * yDis);
        }

        public void SaveImage()
        {
            if (jpgFilePath != "")
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
            jpgFilePath = FolderHelper.GetImageFolder() + "latest.jpg";
            bmp.Save(jpgFilePath);
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

            double k = GlobalVars.Instance.RecipeCollection.SelectedRecipe.AdjustRatio.YRatio * grayValsPerLevel / grayUnit;
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
                vals.Add(_allPixels[y, x].Y * GlobalVars.Instance.RecipeCollection.SelectedRecipe.AdjustRatio.YRatio);
            return vals;
        }

        internal List<float> GetVerticalLineVals(int x)
        {
            List<float> vals = new List<float>();
            for (int y = 0; y < Height; y++)
                vals.Add(_allPixels[y, x].Y * GlobalVars.Instance.RecipeCollection.SelectedRecipe.AdjustRatio.YRatio);
            return vals;
        }

        internal void ClearGrayImage()
        {
            jpgFilePath = "";
        }
    }
}
