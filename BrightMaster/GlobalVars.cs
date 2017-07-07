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
