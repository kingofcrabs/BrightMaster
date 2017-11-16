using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BrightMaster.Settings
{
    [Serializable]
    public class CameraSettings:BindableBase
    {
        int workingDistance;
        int exposureTime;
        bool autoExposure;

        public CameraSettings()
        {
            workingDistance = 1000;
            exposureTime = 80;
            autoExposure = true;
        }

        public CameraSettings(int wd, int epTime,bool autoExp)
        {
            workingDistance = wd;
            exposureTime = epTime;
            autoExposure = autoExp;
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

        public bool AutoExposure
        {
            get
            {
                return autoExposure;
            }
            set
            {
                SetProperty(ref autoExposure, value);
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


    public class BooleanInverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}
