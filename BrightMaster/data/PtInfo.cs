﻿using BrightMaster.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BrightMaster
{
    class PixelInfo:BindableBase
    {
        private float _X,_Y,_Z,_x,_y,_u,_v;
        bool _xValid, _yValid, _LValid;
        public float X { get
            {
                return _X;
            }
            
            set
            {
                SetProperty(ref _X, value);
            }
        }


        public float u
        {
            get
            {
                return _u;
            }
        }

        public float v
        {
            get
            {
                return _v;
            }
        }



        public float Y
        {
            get
            {
                return _Y;
            }
            set
            {
                SetProperty(ref _Y, value);
            }
        }
        public float Z
        {
            get
            {
                return _Z;
            }
            set
            {
                SetProperty(ref _Z, value);
            }
        }

        public float x
        {
            get
            {
                return _x;
            }
            set
            {
                SetProperty(ref _x, value);
            }
        }
        public float y
        {
            get
            {
                return _y;
            }
            set
            {
                SetProperty(ref _y, value);
            }
        }

        public Brush BrushColor
        {
            get
            {
                return Brushes.Green;
            }
        
        }

        public bool xValid
        {
            get
            {
                
                return _xValid;

            }
            set
            {
                SetProperty(ref _xValid, value);
            }
        }


        public bool yValid
        {
            get
            {
                return _yValid;

            }
            set
            {
                SetProperty(ref _yValid, value);
            }
        }

        public bool LValid
        {
            get
            {
                return _LValid;
            }
            set
            {
                SetProperty(ref _LValid, value);
            }
        }


       

        private int id;
        public int ID
        {
            get
            {
                return id;
            }
        }


        public PixelInfo(int id,float XX, float YY, float ZZ)
        {
            this.id = id;
            _X = XX;
            _Y = YY;
            _Z = ZZ;
            _x = _X / (_X + _Y + _Z);
            _y = _Y / (_X + _Y + _Z);
            _u =  4*_X /(_X + 15*_Y + 3*_Z);
            _v =  9*_Y / (_X + 15 * _Y + 3 *_Z);

            double max = GlobalVars.Instance.Constrains.Maxx;
            double min = GlobalVars.Instance.Constrains.Minx;
            xValid = _x > min && _x < max;

            max = GlobalVars.Instance.Constrains.Maxy;
            min = GlobalVars.Instance.Constrains.Miny;
            yValid = _y > min && _y < max;

            min = GlobalVars.Instance.Constrains.MinL;
            LValid = _Y > min;
        }
    }
}
