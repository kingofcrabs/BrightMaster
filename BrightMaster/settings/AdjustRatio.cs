using BrightMaster.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster.Settings
{
    [Serializable]
    public class AdjustRatio:BindableBase
    {
        float xRatio, yRatio, zRatio;
        public AdjustRatio()
        {
            xRatio = yRatio = zRatio = 1.0f;
        }


        public AdjustRatio(float xRatio,float yRatio, float zRatio)
        {
            this.xRatio = xRatio;
            this.yRatio = yRatio;
            this.zRatio = zRatio;
        }

        public float XRatio
        {
            get
            {
                return xRatio;
            }
            set
            {
                SetProperty(ref xRatio, value);
            }
        }
        public float YRatio
        {
            get
            {
                return yRatio;
            }
            set
            {
                SetProperty(ref yRatio, value);
            }
        }

        public float ZRatio
        {
            get
            {
                return zRatio;
            }
            set
            {
                SetProperty(ref zRatio, value);
            }
        }

    }
}
