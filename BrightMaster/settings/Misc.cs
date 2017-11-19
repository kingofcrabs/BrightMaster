using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster.Settings
{
    [Serializable]
    public class Misc
    {
        public string SaveFolder { get; set; }
        public bool AutoFindBoundary { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public int ThresholdVal
        {
            get
            {
                return thresholdVal;
            }
            set
            {
                thresholdVal = value;
            }
        }
        public int thresholdVal;
        public Misc()
        {
            SaveFolder = FolderHelper.GetDefaultSaveFolder();
            thresholdVal = 40;
            AutoFindBoundary = false;
        }

        internal void Save()
        {
            string miscFolder = FolderHelper.GetMiscFolder();
            string file = miscFolder + "misc.xml";
            SerializeHelper.Save(this, file);
        }
    }
}
