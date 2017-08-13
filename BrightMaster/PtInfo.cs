using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster
{
    class PixelInfo
    {
        private float _X,_Y,_Z,_x,_y;
        public float X { get
            {
                return _X;
            }
            
            set
            {
                _X = value;
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
                _Y = value;
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
                _Z = value;
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
                _x = value;
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
                _y = value;
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
        }
    }
}
