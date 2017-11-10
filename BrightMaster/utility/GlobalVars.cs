using BrightMaster.settings;
using BrightMaster.Settings;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster
{
    class GlobalVars
    {
        static private GlobalVars instance;

        private RecipeCollection recipeCollection = new RecipeCollection();
        bool isTest = false;

        private GlobalVars()
        {
           
        }
        public string ParamPath
        {
            get
            {
                return @"D:\Projects\BrightMaster\trunk\param";
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

        public bool IsTest
        {
            get
            {
                return isTest;
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
    }
}
