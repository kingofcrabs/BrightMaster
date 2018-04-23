using BrightMaster.data;
using BrightMaster.settings;
using BrightMaster.Settings;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BrightMaster
{
    class GlobalVars
    {
        static private GlobalVars instance;
        GridViewColumnWidth columnWidths = new GridViewColumnWidth();
        HistoryInfoCollection regionsHistroyInfoCollection = new HistoryInfoCollection();
        HistoryInfoCollection wholePanelHistroyInfoCollection = new HistoryInfoCollection();
        private RecipeCollection recipeCollection = new RecipeCollection();
        bool saveRefImage = false;
        bool needBarcode = false;
        bool isFirst = true;
        private GlobalVars()
        {
            needBarcode = bool.Parse(ConfigurationManager.AppSettings["InputBarcode"]);
            if (ConfigurationManager.AppSettings.AllKeys.Contains("SaveReferenceImage"))
                saveRefImage = bool.Parse(ConfigurationManager.AppSettings["SaveReferenceImage"]);
            string miscFolder = FolderHelper.GetMiscFolder();
            string file = miscFolder + "misc.xml";
            MiscSettings = SerializeHelper.Load<Misc>(file);
            StartTime = DateTime.Now.ToString("hhmmss");
        }
        public HistoryInfoCollection WholePanelHistoryInfoCollection
        {
            get
            {
                return wholePanelHistroyInfoCollection;
            }
            set
            {
                wholePanelHistroyInfoCollection = value;
            }
        }

        public bool SaveRefImage
        {
            get
            {
                return saveRefImage;
            }
        }

        public HistoryInfoCollection RegionsHistoryInfoCollection
        {
            get
            {
                return regionsHistroyInfoCollection;
            }
            set
            {
                regionsHistroyInfoCollection = value;
            }
        }

        public GridViewColumnWidth GridColumnWidth
        {
            get
            {
                return columnWidths;
            }
            set
            {
                columnWidths = value;
            }
        }

        public Misc MiscSettings { get; set; }
        public string StartTime { get; set; }
        public PowerSettings PowerSettings
        {
            get
            {
                return recipeCollection.SelectedRecipe.PowerSettings;
            }
        }
        public bool NeedBarcode
        {
            get
            {
                return needBarcode;
            }
        }
        public string ParamPath
        {
            get
            {
                string paramFolder = FolderHelper.GetExeParentFolder() + "param\\";
                if (!Directory.Exists(paramFolder))
                    throw new Exception(string.Format("找不到位于{0}param文件夹！", paramFolder));
                return paramFolder;
            }
        }
        public CameraSettings CameraSettings
        {
            get
            {
                return recipeCollection.SelectedRecipe.CameraSettings;
            }
        }
        public Constrains Constrains
        {
            get
            {
                return recipeCollection.SelectedRecipe.Constrains;
            }
        }

        public AdjustRatio AdjustRatio
        {
            get
            {
                return recipeCollection.SelectedRecipe.AdjustRatio;
            }
        }

        public Layout Layout
        {
            get
            {
                if (recipeCollection.SelectedRecipe == null)
                    return null;
                return recipeCollection.SelectedRecipe.Layout;
            }
        }

        public bool AnalysisRegions
        {
            get
            {
                if (recipeCollection.SelectedRecipe == null)
                    return true;
                return recipeCollection.SelectedRecipe.AnalysisRegions;
            }
        }

       


        public RecipeCollection RecipeCollection
        {
            get
            {
                return recipeCollection;
            }
            set
            {
                recipeCollection = value;
            }
        }

        private static UAContorller uaController = new UAContorller();

        public UAContorller UAController 
        { 
            get
            {
                return uaController;
            }
        }

      
        static public GlobalVars Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GlobalVars();
               
                }
                return instance;
            }
        }


        public bool AnalysisSuccess { get; set; }
        public string Barcode { get; set; }

        public bool IsFirst
        {
            get
            {
                if(isFirst)
                {
                    isFirst = false;
                    return true;
                }
                return isFirst;
            }
        }
    }
}
