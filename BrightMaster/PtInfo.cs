using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster
{
    class PixelInfo
    {
        public float X;
        public float Y;
        public float Z;
        public float x;
        public float y;

        public PixelInfo(float XX, float YY, float ZZ)
        {
            X = XX;
            Y = YY;
            Z = ZZ;
            x = X / (X + Y + Z);
            y = Y / (X + Y + Z);
        }
    }
}
