using BrightMaster.data;
using BrightMaster.settings;
using BrightMaster.Settings;
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
        HistoryInfoCollection histroyInfoCollection = new HistoryInfoCollection();
        private RecipeCollection recipeCollection = new RecipeCollection();
        bool needBarcode = false;
        private GlobalVars()
        {
            needBarcode = bool.Parse(ConfigurationManager.AppSettings["InputBarcode"]);
            string miscFolder = FolderHelper.GetMiscFolder();
            string file = miscFolder + "misc.xml";
            MiscSettings = SerializeHelper.Load<Misc>(file);
            //autoFindRect = bool.Parse(ConfigurationManager.AppSettings["AutoFindRect"]);
        }


        public HistoryInfoCollection HistoryInfos
        {
            get
            {
                return histroyInfoCollection;
            }
            set
            {
                histroyInfoCollection = value;
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
                return @"D:\Projects\BrightMaster\trunk\param";
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

        public Layout Layout
        {
            get
            {
                return recipeCollection.SelectedRecipe.Layout;
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

        public string Barcode { get; set; }
    }
}
