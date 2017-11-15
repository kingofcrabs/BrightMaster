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


        public TestResult(float Lmax, float Lmin, bool isOk, float uniform )
        {
            this.lmax = Lmax;
            this.lmin = Lmin;
            this.uniform = uniform;
            this.isOk = isOk;
            description = isOk ? "Ok" : "NG";
            if (GlobalVars.Instance.NeedBarcode)
                barcode = GlobalVars.Instance.Barcode;
            else
                barcode = DateTime.Now.ToString("yyyyMMddHHmmss");
        }

    }
}
