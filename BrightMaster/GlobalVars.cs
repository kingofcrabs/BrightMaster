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

        public Layout Layout;
        bool isTest = false;

        private GlobalVars()
        {
            isTest = bool.Parse(ConfigurationManager.AppSettings["Test"]);
        }
        public string ParamPath
        {
            get
            {
                return @"D:\Projects\BrightMaster\trunk\param";
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
