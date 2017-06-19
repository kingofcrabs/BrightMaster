using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster
{
    class Brightness
    {
        public List<List<PixelInfo>> _allPixels = new List<List<PixelInfo>>();
        public List<List<byte>> grayVals = new List<List<byte>>();
        public List<List<float>> orgVals = new List<List<float>>();
        public byte[] grayValsInArray;
        public string pngFile;
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
