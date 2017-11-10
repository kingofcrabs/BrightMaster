using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster.Settings
{
    [Serializable]
    public class CameraSettings:BindableBase
    {
        int workingDistance;
        int exposureTime;

        public CameraSettings()
        {
            workingDistance = 1000;
            exposureTime = 80;
        }

        public CameraSettings(int wd, int epTime)
        {
            workingDistance = wd;
            exposureTime = epTime;
        }
        public int WorkingDistance
        {
            get
            {
                return workingDistance;
            }
            set
            {
                SetProperty(ref workingDistance, value);
            }
        }

        public int ExposureTime
        {
            get
            {
                return exposureTime;
            }
            set
            {
                SetProperty(ref exposureTime, value);
            }
        }


        internal void CheckSetting()
        {
            if (workingDistance < 0)
                throw new Exception("工作距离必须大于0！");
            if (exposureTime < 0)
                throw new Exception("曝光时间必须大于等于0！");
        }
    }
}
