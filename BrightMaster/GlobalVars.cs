using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster
{
    class GlobalVars
    {
        static private GlobalVars instance;

        public Layout Layout;


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
