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
        double minx;
        double miny;
        double maxx;
        double maxy;
        double minUniform;
        double minL;

        
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
        public double Minx
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
        public double Maxx
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
        public double Miny
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
        public double Maxy
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
        public double MinUniform
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
        public double MinL
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
