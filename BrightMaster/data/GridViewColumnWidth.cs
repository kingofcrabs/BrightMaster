﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster.data
{
    class GridViewColumnWidth
    {


        public int ID = 40;
        public int X = 60;
        public int Y = 60;
        public int Z = 60;
        public int x = 60;
        public int y = 60;
        public int u = 60;
        public int v = 60;
        public int L = 60;
        

        public int this[int index]
        {
            get
            {
                int val = 0;
                switch(index)
                {
                    case 0:
                        val = ID;
                        break;
                    case 1:
                        val = L;
                        break;
                    case 2:
                        val = X;
                        break;
                    case 3:
                        val = Y;
                        break;
                    case 4:
                        val = Z;
                        break;
                    case 5:
                        val = x;
                        break;
                    case 6:
                        val = y;
                        break;
                    case 7:
                        val = u;
                        break;
                    case 8:
                        val = v;
                        break;
                    default:
                        break;

                }
                return val;
            }
        }


    }
}
