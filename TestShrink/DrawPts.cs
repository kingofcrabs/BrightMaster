using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestShrink
{
    public partial class DrawPts : Control
    {
        List<PointF> pts = new List<PointF>();
        public DrawPts()
        {
            InitializeComponent();
        }

        public void AddPt(PointF newPt)
        {
            pts.Add(newPt);
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            Random rnd = new Random(DateTime.Now.Millisecond);
            for(int i = 0; i< pts.Count; i++)
            {
                Color color = Color.FromArgb(rnd.Next() % 255, rnd.Next() % 255, rnd.Next() % 255);
                SolidBrush brush = new SolidBrush(color);
                pe.Graphics.DrawEllipse(new Pen(brush,2), new Rectangle((int)pts[i].X, (int)pts[i].Y, 2, 2));

            }
        }
    }
}
