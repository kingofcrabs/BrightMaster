using BrightMaster.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster.settings
{
    [Serializable]
    public class PowerSettings:BindableBase
    {
        private float voltage;
        private float current;
        public float Voltage
        {
            get
            {
                return voltage;
            }
            set
            {
                SetProperty(ref voltage, value);
            }
        }

        public float Current
        {
            get
            {
                return current;
            }
            set
            {
                SetProperty(ref current, value);
            }
        }

        public PowerSettings()
        {
            this.current = 0.2f;
            this.voltage = 12f;
        }

        

        public PowerSettings(float voltage, float current)
        {
            this.current = current;
            this.voltage = voltage;
        }

    }
}
