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



        List<float> GetResults(Rect boundingRect)
        {
            var layout = GlobalVars.Instance.Layout;
            List<float> results = new List<float>();
            float xStart = (float)(boundingRect.X + layout.topLeft.X / layout.Width * boundingRect.Width);
            float yStart = (float)(boundingRect.Y + layout.topLeft.Y / layout.Height * boundingRect.Height);
            float xEnd = (float)(boundingRect.X + layout.bottomRight.X / layout.Width * boundingRect.Width);
            float yEnd = (float)(boundingRect.Y + layout.bottomRight.Y / layout.Height * boundingRect.Height);
            float radius = (float)(layout.Radius / layout.Width * boundingRect.Width);
            PointF ptStart = new PointF(xStart, yStart);
            PointF ptEnd = new PointF(xEnd, yEnd);
            for (int x = 0; x < layout.XCount; x++)
            {
                for (int y = 0; y < layout.YCount; y++)
                {
                    float xx = (ptEnd.X - ptStart.X) * x / (layout.XCount - 1) + ptStart.X;
                    float yy = (ptEnd.Y - ptStart.Y) * y / (layout.YCount - 1) + ptStart.Y;
                    results.Add(GetAvgVals(xx,yy,radius));
                }
            }
            return results;
            //layout.TopLeftX
        }

        private float GetAvgVals(float xx, float yy, float radius)
        {
            List<float> vals = new List<float>();
            int xStart = (int)(xx - radius);
            int yStart = (int)(yy - radius);
            int xEnd = (int)(xx + radius);
            int yEnd = (int)(yy + radius);
            for(int x = xStart; x < xEnd; x++)
            {
                for(int y = yStart; y< yEnd; y++)
                {
                    vals.Add(orgVals[y][x]);
                }
            }
            return vals.Average();
        }


        private void SaveImage()
        {
            Bitmap bmp = new Bitmap(grayVals.Count,grayVals[0].Count);
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
            double max = maxList.Max();
            double min = minList.Min();
            double grayUnit = (max - min) / 255;
            grayValsInArray = new byte[Height * Width];
            int pixelCnt = 0;
            for (int y = 0; y < Height; y++)
            {
                List<byte> thisLineGrayVals = new List<byte>();
                for (int x = 0; x < Width; x++)
                {
                    byte val = (byte)((orgVals[y][x] - min) / grayUnit);
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
