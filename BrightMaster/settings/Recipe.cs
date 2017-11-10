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
        private string name;

        public Recipe()
        {

        }
        public Recipe(string name)
        {
            this.name = name;
            layout = new Layout();
            constrains = new Constrains();
            cameraSettings = new CameraSettings();
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
