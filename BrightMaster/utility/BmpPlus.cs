using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster
{
    class BitmapPlus
    {
        private Bitmap bmp_ = null;
        private BitmapData img_ = null;

        public BitmapPlus(Bitmap original)
        {
            bmp_ = original;
        }

        public Bitmap Bitmap
        {
            get
            {

                return bmp_;
            }
        }

        public void BeginAccess()
        {
            img_ = bmp_.LockBits(new Rectangle(0, 0, bmp_.Width, bmp_.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        }
        public void EndAccess()
        {
            if (img_ != null)
            {
                bmp_.UnlockBits(img_);
                img_ = null;
            }
        }
        public void SetPixel(int x, int y, Color c)
        {
            if (img_ == null)
            {
                bmp_.SetPixel(x, y, c);
                return;
            }
            IntPtr adr = img_.Scan0;
            int pos = x * 3 + img_.Stride * y;
            System.Runtime.InteropServices.Marshal.WriteByte(adr, pos + 0, c.B);
            System.Runtime.InteropServices.Marshal.WriteByte(adr, pos + 1, c.G);
            System.Runtime.InteropServices.Marshal.WriteByte(adr, pos + 2, c.R);
        }
    }
}
