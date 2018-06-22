using BrightMaster.settings;
using BrightMaster.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster.Settings
{
    [Serializable]
    public class Recipe : BindableBase
    {
        private Layout layout;
        private Constrains constrains;
        private CameraSettings cameraSettings;
        private AdjustRatio adjustRatio;
        private PowerSettings powerSettings = new PowerSettings();
        private string name;
        private List<AdjustRatio> adjustRatios;
        private bool analysisRegions;
        public Recipe()
        {

        }
        public Recipe(string name)
        {
            this.name = name;
            layout = new Layout();
            constrains = new Constrains();
            cameraSettings = new CameraSettings();
            adjustRatio = new AdjustRatio();
            powerSettings = new PowerSettings(12f,0.1f);
            analysisRegions = true;
        }

        public List<AdjustRatio> AdjustRatios
        {
            get
            {
                return adjustRatios;
            }
            set
            {
                adjustRatios = value;
            }
        }

        public bool AnalysisRegions
        {
            get
            {
                return analysisRegions;
            }
            set
            {
                SetProperty(ref analysisRegions, value);
            }
        }

        public PowerSettings PowerSettings
        {
            get
            {
                return powerSettings;
            }
            set
            {
                SetProperty(ref powerSettings, value);
            }
        }
        public AdjustRatio AdjustRatio
        {
            get
            {
                return adjustRatio;
            }
            set
            {
                SetProperty(ref adjustRatio, value);
            }
        }
        public CameraSettings CameraSettings
        {
            get
            {
                return cameraSettings;
            }
            set
            {
                SetProperty(ref cameraSettings, value);
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                SetProperty(ref name, value);
            }
        }
        public Layout Layout
        {
            get
            {
                return layout;
            }
            set
            {
                SetProperty(ref layout, value);
            }
        }

        public Constrains Constrains
        {
            get
            {
                return constrains;
            }
            set
            {
                SetProperty(ref constrains, value);
            }
        }

        internal void Save()
        {
            string sFile = FolderHelper.GetRecipeFolder() + name + ".xml";
            SerializeHelper.SaveSettings(this, sFile);
        }
    }
}
