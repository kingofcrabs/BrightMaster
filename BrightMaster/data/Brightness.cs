using EngineDll;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BrightMaster
{
    class Brightness
    {
        public List<List<PixelInfo>> _allPixels = new List<List<PixelInfo>>();
        public List<List<byte>> grayVals = new List<List<byte>>();
        public List<List<float>> orgVals = new List<List<float>>();
        public byte[] grayValsInArray;
        public string jpgFilePath;
        public int[] GrayLevelCounts = new int[256];
        public int Width { get; set; }
        public int Height { get; set; }

        public double Max { get; set; }
        public double Min { get; set; }

        public Brightness(List<List<PixelInfo>> pixels)
        {
            _allPixels = pixels;
            orgVals.Clear();
            foreach(var linePixel in pixels)
            {
                List<float> vals = linePixel.Select(x => x.Y).ToList();
                orgVals.Add(vals);
            }
            grayVals = Convert2Gray(orgVals);
            jpgFilePath = FolderHelper.GetImageFolder() + "latest.jpg";
            SaveImage();
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
            List<PixelInfo> vals = new List<PixelInfo>();
            int xStart = (int)(xx - radius);
            int yStart = (int)(yy - radius);
            int xEnd = (int)(xx + radius);
            int yEnd = (int)(yy + radius);
            for(int x = xStart; x < xEnd; x++)
            {
                for(int y = yStart; y< yEnd; y++)
                {
                    vals.Add(_allPixels[y][x]);
                }
            }

            float X,Y,Z;
            X = vals.Average(val => val.X);
            Y = vals.Average(val => val.Y);
            Z = vals.Average(val => val.Z);
            return new PixelInfo(ID,X, Y, Z);
        }

        private void SaveImage()
        {
            Bitmap bmp = new Bitmap(grayVals[0].Count, grayVals.Count);
            LockBitmap lockBmp = new LockBitmap(bmp);
            lockBmp.LockBits();
            for (int y = 0; y < lockBmp.Height; y++)
            {
                List<double> vals = new List<double>();
                for (int x = 0; x < lockBmp.Width; x++)
                {
                    byte grayVal = grayVals[y][x];
                    lockBmp.SetPixel(x,y,Color.FromArgb(grayVal,grayVal,grayVal));
                }
            }
            bmp.Save(jpgFilePath);
        }

        private List<List<byte>> Convert2Gray(List<List<float>> orgVals)
        {
            List<List<byte>> vals = new List<List<byte>>();
            for (int i = 0; i < 255; i++)
            {
                GrayLevelCounts[i] = 0;
            }
            byte[] map = new byte[256];
            long[] lCounts = new long[256];
            //each gray level count
            Height = orgVals.Count;
            Width = orgVals[0].Count;
            List<float> maxList = orgVals.Select(l => l.Max()).ToList();
            List<float> minList = orgVals.Select(l => l.Min()).ToList();
            Max = maxList.Max();
            Min = minList.Min();
            double grayUnit = (Max - Min) / 255;
            grayValsInArray = new byte[Height * Width];
            int pixelCnt = 0;
            
            for (int y = 0; y < Height; y++)
            {
                List<byte> thisLineGrayVals = new List<byte>();
                for (int x = 0; x < Width; x++)
                {
                    byte val = (byte)((orgVals[y][x] - Min) / grayUnit);
                    GrayLevelCounts[val]++;
                    thisLineGrayVals.Add(val);
                    grayValsInArray[pixelCnt++] = val;
                }
                vals.Add(thisLineGrayVals);
            }
            return vals;
        }
    }
}
