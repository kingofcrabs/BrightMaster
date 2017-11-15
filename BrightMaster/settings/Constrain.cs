using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster.Settings
{
    [Serializable]
    public class Constrains :BindableBase
    {
        float minx;
        float miny;
        float maxx;
        float maxy;
        float minUniform;
        float minL;

        
        public Constrains()
        {
            minx = 0;
            miny = 0;
            maxx = 8000;
            maxy = 8000;
            minUniform = 0;
            minL = 0;
        }

        public void CheckSetting()
        {
            if (minx < 0 || miny < 0 || maxx < 0 || maxy < 0 || minUniform < 0 || minL < 0)
                throw new Exception("所有限制条件必须大于0！");
        }
        public float Minx
        {
            get
            {
                return minx;
            }
            set
            {
                SetProperty(ref minx, value);
            }
        }
        public float Maxx
        {
            get
            {
                return maxx;
            }
            set
            {
                SetProperty(ref maxx, value);
            }
        }
        public float Miny
        {
            get
            {
                return miny;
            }
            set
            {
                SetProperty(ref miny, value);
            }
        }
        public float Maxy
        {
            get
            {
                return maxy;
            }
            set
            {
                SetProperty(ref maxy, value);
            }
        }
        public float MinUniform
        {
            get
            {
                return minUniform;
            }
            set
            {
                SetProperty(ref minUniform, value);
            }
        }
        public float MinL
        {
            get
            {
                return minL;
            }
            set
            {
                SetProperty(ref minL, value);
            }
        }

        
    }
}
