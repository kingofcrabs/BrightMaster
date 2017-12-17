using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster.data
{
    [Serializable]
    public class TestResult
    {
        private float lmax;
        private float lmin;
        private PixelInfo pixelInfoCenter;
        private float uniform;
        private bool isOk;
        private string description;
        private string barcode;

        public string Label
        {
            get
            {
                return barcode;
            }
        }
        public bool IsOk
        { 
            get
            {
                return isOk;
            }
        
        }
        public float LMax
        {
            get
            {
                return lmax;
            }
        }


        public float LMin
        {
            get
            {
                return lmin;
            }
        }

        public float LCenter
        {
            get
            {
                return pixelInfoCenter.Y;
            }
        }

        public float x
        {
            get
            {
                return pixelInfoCenter.x;
            }
        }

        public float y
        {
            get
            {
                return pixelInfoCenter.y;
            }
        }
    
    

        public float Uniform
        {
            get
            {
                return uniform;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
        }

        public TestResult()
        {
            isOk = false;
        }
        internal static TestResult GetWholePanelResult(Brightness brightness)
        {
            float maxL = (float)brightness.MaxNoMargin;
            float minL = (float)brightness.MinNoMargin;
            
            float minUniform = GlobalVars.Instance.Constrains.MinUniform;
            float uniform = minL / maxL * 100;
            uniform = (float)Math.Round(uniform, 2);
            bool isOk = uniform > minUniform;
            TestResult testResult = new TestResult(maxL, minL, isOk, uniform, brightness.GetCenterInfo());
            return testResult;
        }
        internal static TestResult GetRegionResult(List<PixelInfo> pixelInfos)
        {
            float maxL = pixelInfos.Max(x => x.Y);
            float minL = pixelInfos.Min(x => x.Y);
            float minUniform = GlobalVars.Instance.Constrains.MinUniform;
            float uniform = minL / maxL * 100;
            uniform = (float)Math.Round(uniform, 2);
            bool isOk = uniform > minUniform;
            TestResult testResult = new TestResult(maxL, minL, isOk, uniform);
            return testResult;
        }
        public TestResult(float Lmax, float Lmin, bool isOk, float uniform, PixelInfo pixelInfoCenter = null)
        {
            this.lmax = Lmax;
            this.lmin = Lmin;
            this.uniform = uniform;
            this.pixelInfoCenter = pixelInfoCenter;
            this.isOk = isOk;
            description = isOk ? "Ok" : "NG";
            if (GlobalVars.Instance.NeedBarcode)
                barcode = GlobalVars.Instance.Barcode;
            else
                barcode = DateTime.Now.ToString("yyyyMMddHHmmss");
        }


        
    }
}
