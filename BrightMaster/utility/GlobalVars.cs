﻿using BrightMaster.data;
using BrightMaster.settings;
using BrightMaster.Settings;
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
        HistoryInfoCollection histroyInfoCollection = new HistoryInfoCollection();
        private RecipeCollection recipeCollection = new RecipeCollection();
        bool needBarcode = false;
        private GlobalVars()
        {
            needBarcode = bool.Parse(ConfigurationManager.AppSettings["InputBarcode"]);
            string miscFolder = FolderHelper.GetMiscFolder();
            string file = miscFolder + "misc.xml";
            MiscSettings = SerializeHelper.Load<Misc>(file);
            Barcode = "";
            //autoFindRect = bool.Parse(ConfigurationManager.AppSettings["AutoFindRect"]);
        }


        public HistoryInfoCollection HistoryInfoCollection
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
